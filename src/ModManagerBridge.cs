using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Shtab.RotatingMinimap;

// Optional integration with StoreAndCraft's in-game mod settings window.
// Its UI keeps unsaved values outside ConfigEntry and does not sort sections.
// Every patch below is limited to this plugin's page/entries/sliders.
internal static class ModManagerBridge
{
    private static FieldInfo? _pageField;
    private static FieldInfo? _contentField;
    private static MethodInfo? _buildPage, _setPending, _makeButton, _endCapture;
    private static FieldInfo? _saveField, _statusField;
    private static Harmony? _harmony;
    private static bool _available, _resetZoomPending;

    public static void Install()
    {
        Uninstall();
        var panel = AccessTools.TypeByName("StoreAndCraft.ModManagerPanel");
        if (panel == null) return;
        // A separate owner allows complete rollback without touching the minimap patches.
        _harmony = new Harmony(Plugin.Id + ".settings");
        try
        {
            var page = RequireMethod(panel, "BuildPage");
            var number = RequireMethod(panel, "BuildNumber", typeof(RectTransform), typeof(ConfigEntryBase), typeof(bool), typeof(double), typeof(double), typeof(bool));
            var pending = RequireMethod(panel, "SetPending", typeof(ConfigEntryBase), typeof(object));
            var sliderSet = RequireMethod(typeof(Slider), "Set", typeof(float), typeof(bool));
            var valueText = RequireMethod(panel, "ValueText", typeof(ConfigEntryBase), typeof(object));
            var row = RequireMethod(panel, "AddEntryRow", typeof(ConfigEntryBase));
            var header = RequireMethod(panel, "AddHeader", typeof(string));
            var save = RequireMethod(panel, "Save");
            var rebuild = RequireMethod(panel, "Rebuild");
            var close = RequireMethod(panel, "Close");
            _pageField = RequireField(panel, "_page");
            var entries = RequireField(_pageField.FieldType, "Entries");
            if (entries.FieldType != typeof(List<ConfigEntryBase>)) throw new MissingFieldException("Entries type");
            _contentField = RequireField(panel, "_content", typeof(RectTransform));
            _saveField = RequireField(panel, "_save", typeof(Button));
            _statusField = RequireField(panel, "_status", typeof(TextMeshProUGUI));
            _endCapture = RequireMethod(panel, "EndCapture");
            var settingsPanel = AccessTools.TypeByName("StoreAndCraft.SettingsPanel") ?? throw new TypeLoadException("SettingsPanel");
            _makeButton = RequireMethod(settingsPanel, "MakeButton", typeof(RectTransform), typeof(string), typeof(string), typeof(UnityAction));
            if (_makeButton.ReturnType != typeof(Button)) throw new MissingMethodException("MakeButton return type");
            var loc = AccessTools.TypeByName("StoreAndCraft.Loc") ?? throw new TypeLoadException("Loc");
            var text = RequireMethod(loc, "T", typeof(string), typeof(string));
            _buildPage = page;
            _setPending = pending;
            _harmony.Patch(page, prefix: Patch(nameof(SortPage)), postfix: Patch(nameof(AddResetButton)));
            _harmony.Patch(number, postfix: Patch(nameof(MarkSlider)));
            _harmony.Patch(pending, prefix: Patch(nameof(RoundPending)), postfix: Patch(nameof(KeepResetSaveEnabled)));
            _harmony.Patch(sliderSet, prefix: Patch(nameof(SnapSlider)));
            _harmony.Patch(valueText, postfix: Patch(nameof(TranslateChoice)));
            _harmony.Patch(row, postfix: Patch(nameof(TranslateRow)));
            _harmony.Patch(header, prefix: Patch(nameof(TranslateHeader)));
            _harmony.Patch(text, postfix: Patch(nameof(TranslatePanelText)));
            _harmony.Patch(save, postfix: Patch(nameof(AfterSave)));
            _harmony.Patch(rebuild, prefix: Patch(nameof(CancelReset)));
            _harmony.Patch(close, prefix: Patch(nameof(CancelReset)));
            _available = true;
        }
        catch
        {
            Uninstall();
            throw;
        }
    }

    private static MethodInfo RequireMethod(Type type, string name, params Type[] args) =>
        AccessTools.Method(type, name, args) ?? throw new MissingMethodException(type.FullName, name);
    private static FieldInfo RequireField(Type type, string name, Type? expected = null)
    {
        var field = AccessTools.Field(type, name) ?? throw new MissingFieldException(type.FullName, name);
        if (expected != null && field.FieldType != expected) throw new MissingFieldException(type.FullName, name);
        return field;
    }

    public static void Uninstall()
    {
        _available = false;
        CancelReset();
        _harmony?.UnpatchSelf();
        _harmony = null;
    }

    private static void Safe(Action action)
    {
        if (!_available) return;
        try { action(); }
        catch (Exception error)
        {
            // Never throw our reflection/UI errors into the host settings window.
            try { Uninstall(); }
            catch (Exception cleanup) { Plugin.Instance?.WarnSettings(cleanup); }
            Plugin.Instance?.WarnSettings(error);
        }
    }

    private static HarmonyMethod Patch(string method) => new HarmonyMethod(typeof(ModManagerBridge), method);
    private static bool Own(ConfigEntryBase entry) => Plugin.Instance != null &&
        ReferenceEquals(entry.ConfigFile, Plugin.Instance.Config);

    private static bool ZoomNumber(ConfigEntryBase entry) => Own(entry) &&
        entry.SettingType == typeof(float) && (entry.Definition.Section == "3. Зум" ||
            entry.Definition.Key == "Толщина линии");

    private static List<ConfigEntryBase>? OwnEntries()
    {
        object? page = _pageField?.GetValue(null);
        if (page == null) return null;
        var entries = AccessTools.Field(page.GetType(), "Entries")?.GetValue(page) as List<ConfigEntryBase>;
        if (entries == null || entries.Count == 0 || entries.Exists(entry => !Own(entry))) return null;
        return entries;
    }

    private static void AddResetButton() => Safe(AddResetButtonCore);
    private static void AddResetButtonCore()
    {
        if (OwnEntries() == null || _makeButton == null) return;
        var content = _contentField?.GetValue(null) as RectTransform;
        if (!content) return;
        AddPresetButton(content!, true);
        AddPresetButton(content!, false);
    }

    private static void AddPresetButton(RectTransform content, bool vanilla)
    {
        var row = new GameObject("BetterMinimapResetDefaults", typeof(RectTransform), typeof(LayoutElement));
        row.transform.SetParent(content, false);
        var layout = row.GetComponent<LayoutElement>();
        layout.minHeight = layout.preferredHeight = 54;
        layout.flexibleWidth = 1;
        var button = _makeButton!.Invoke(null, new object[] {
            (RectTransform)row.transform, vanilla ? "ResetVanilla" : "ResetModDefaults",
            SettingsText.ResetLabel(vanilla, UiLanguage.Russian), new UnityAction(() => ResetDefaults(vanilla))
        }) as Button;
        if (!button) { UnityEngine.Object.Destroy(row); return; }
        var rect = (RectTransform)button!.transform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(8, 4); rect.offsetMax = new Vector2(-8, -4);
    }

    private static void ResetDefaults(bool vanilla) => Safe(() => ResetDefaultsCore(vanilla));
    private static void ResetDefaultsCore(bool vanilla)
    {
        var entries = OwnEntries();
        var content = _contentField?.GetValue(null) as RectTransform;
        if (entries == null || !content || _setPending == null || _buildPage == null) return;
        _endCapture?.Invoke(null, null);
        _resetZoomPending = true;
        // Only stage the values: Close/Back can discard them, Save applies them.
        foreach (var entry in entries)
        {
            if (!ResetPreset.Includes(entry.Definition.Key)) continue;
            _setPending.Invoke(null, new object[] { entry, ResetPreset.Value(entry.Definition.Key, entry.DefaultValue, vanilla) });
        }
        // Rebuild just the page content. Rebuilding the whole window would reset Save's state.
        for (int i = content!.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i).gameObject;
            child.SetActive(false);
            UnityEngine.Object.Destroy(child);
        }
        _buildPage.Invoke(null, null);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        var scroll = content.GetComponentInParent<ScrollRect>();
        if (scroll) scroll.verticalNormalizedPosition = 0;
        KeepResetSaveEnabled();
    }

    private static void SortPage() => Safe(SortPageCore);
    private static void SortPageCore()
    {
        var entries = OwnEntries();
        if (entries == null) return;
        entries.Sort((a, b) =>
        {
            int section = string.CompareOrdinal(a.Definition.Section, b.Definition.Section);
            return section != 0 ? section : Order(a.Definition.Key).CompareTo(Order(b.Definition.Key));
        });
    }

    private static int Order(string key)
    {
        switch (key)
        {
            case "Мод включён": return -1;
            case "Вращение": case "Min zoom": case "Зум через + и -": case "Показывать линии компаса": return 0;
            case "Режим": case "Max zoom": case "Приблизить": case "Показывать направления": return 1;
            case "Форма": case "Шаг зума": case "Отдалить": case "Тип линии": return 2;
            case "Размер мини-карты": case "Сохранять отдельный зум для кораблей": case "Цвет линии": return 3;
            case "Положение биома": case "Толщина линии": return 4;
            case "Положение ветра": return 5;
            case "Освещение карты": return 6;
            case "Плавный зум": return 4;
            default: return 100;
        }
    }

    private static void TranslateChoice(ConfigEntryBase __0, object __1, ref string __result)
    {
        string result = __result;
        Safe(() => { if (Own(__0) && __1 is Enum) result = SettingsText.Choice(__1.GetType().Name, __1.ToString(), UiLanguage.Russian); });
        __result = result;
    }

    private static void TranslateRow(ConfigEntryBase __0) => Safe(() =>
    {
        if (!Own(__0)) return;
        var content = _contentField!.GetValue(null) as RectTransform;
        if (!content || content!.childCount == 0) return;
        var row = content.GetChild(content.childCount - 1);
        var label = row.Find("Label")?.GetComponent<TMP_Text>();
        var description = row.Find("Desc")?.GetComponent<TMP_Text>();
        if (label) label!.text = SettingsText.Name(__0.Definition.Key, UiLanguage.Russian);
        if (description) description!.text = SettingsText.Description(__0.Definition.Key, UiLanguage.Russian, __0.Description.Description);
    });

    private static void TranslateHeader(ref string __0)
    {
        string text = __0;
        Safe(() => { if (OwnEntries() != null) text = SettingsText.Section(text, UiLanguage.Russian); });
        __0 = text;
    }

    private static void TranslatePanelText(string __0, ref string __result)
    {
        string text = __result;
        Safe(() => { if (OwnEntries() != null) text = SettingsText.PanelText(__0, UiLanguage.Russian); });
        __result = text;
    }

    private static void CancelReset()
    {
        _resetZoomPending = false;
    }
    private static void KeepResetSaveEnabled() => Safe(() =>
    {
        if (!_resetZoomPending || OwnEntries() == null) return;
        var save = _saveField!.GetValue(null) as Button;
        if (save) save!.interactable = true;
        var status = _statusField!.GetValue(null) as TMP_Text;
        if (status) status!.text = UiLanguage.Pick("Сброс и зум ×1 применятся после сохранения.", "Defaults and zoom ×1 will apply after Save.");
    });
    private static void AfterSave() => Safe(() =>
    {
        if (!_resetZoomPending || OwnEntries() == null) return;
        CancelReset();
        Plugin.Instance?.ResetZoomToDefault();
        var save = _saveField!.GetValue(null) as Button;
        if (save) save!.interactable = false;
        var status = _statusField!.GetValue(null) as TMP_Text;
        if (status) status!.text = UiLanguage.Pick("Сохранено.", "Saved.");
    });

    private static void MarkSlider(RectTransform __0, ConfigEntryBase __1) => Safe(() =>
    {
        if (!ZoomNumber(__1) || !__0) return;
        var slider = __0.GetComponentInChildren<Slider>(true);
        if (!slider) return;
        if (!slider.GetComponent<TenthStepSlider>()) slider.gameObject.AddComponent<TenthStepSlider>();
        slider.SetValueWithoutNotify((float)MapMath.RoundSetting(slider.value, slider.minValue, slider.maxValue));
    });

    private static void RoundPending(ConfigEntryBase __0, ref object __1)
    {
        object result = __1;
        Safe(() =>
        {
            if (!ZoomNumber(__0) || !(result is float value)) return;
            var range = __0.Description.AcceptableValues as AcceptableValueRange<float>;
            if (range != null) result = (float)MapMath.RoundSetting(value, range.MinValue, range.MaxValue);
        });
        __1 = result;
    }

    private static void SnapSlider(Slider __instance, ref float __0)
    {
        if (!_available || !__instance.GetComponent<TenthStepSlider>()) return;
        float result = __0;
        Safe(() => result = (float)MapMath.RoundSetting(result, __instance.minValue, __instance.maxValue));
        __0 = result;
    }
}

public sealed class TenthStepSlider : MonoBehaviour { }
