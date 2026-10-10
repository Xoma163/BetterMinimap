using System;
using System.Collections.Generic;
using System.Globalization;

namespace Shtab.RotatingMinimap;

// Stable config keys are never translated: only their presentation changes.
internal static class SettingsText
{
    private static readonly Dictionary<string, (string Name, string Ru, string En)> Entries =
        new Dictionary<string, (string, string, string)>
    {
        ["Мод включён"] = ("Enable mod", "Выключить все функции мода, сохранив настройки. Возвращается штатная мини-карта.", "Disable all mod features without losing settings. Restores the vanilla minimap."),
        ["Показывать мини-карту"] = ("Show minimap", "Показывать мини-карту и её слоты. По умолчанию включено. Большая карта M остаётся доступной. Требуется включённый мод.", "Show the minimap and its HUD slots. Enabled by default. The large M map remains available. Requires the mod to be enabled."),
        ["Вращение"] = ("Rotation", "Вращать мини-карту. Большая карта не изменяется.", "Rotate the minimap. The large map is unchanged."),
        ["Режим"] = ("Orientation", "Север сверху, по камере или по персонажу.", "Keep north up, or follow the camera or character heading."),
        ["Форма"] = ("Shape", "Квадратная или круглая мини-карта.", "Square or circular minimap."),
        ["Освещение карты"] = ("Map lighting", "По времени суток, всегда день или всегда ночь. Освещение мира и большой карты не меняется.", "Follow time of day, always day or always night. Does not affect world or large-map lighting."),
        ["Размер мини-карты"] = ("Minimap size", "Размер всего блока: 50–150%. При увеличении бафы сдвигаются влево.", "Scale the entire minimap: 50–150%. Enlarging it shifts status effects to the left."),
        ["Slot 1"] = ("Slot 1", "Верхний дальний слот. Пусто, биом, ветер или координаты; элементы можно повторять.", "Upper outer slot. Empty, biome, wind or coordinates; elements may be repeated."),
        ["Slot 2"] = ("Slot 2", "Слот непосредственно над мини-картой. По умолчанию — биом.", "Slot immediately above the minimap. Default: biome."),
        ["Slot 3"] = ("Slot 3", "Слот непосредственно под мини-картой. По умолчанию — ветер.", "Slot immediately below the minimap. Default: wind."),
        ["Slot 4"] = ("Slot 4", "Нижний дальний слот. По умолчанию — пусто. Выберите «Координаты» для показа X/Y/Z. Пустые слоты не занимают место.", "Lower outer slot. Default: empty. Select Coordinates to show X/Y/Z. Empty slots take no space."),
        ["Указатель цели"] = ("Target indicator", "Shift + ЛКМ по любой точке большой карты — указать цель. Shift + ПКМ — снять, не удаляя метки. Направление и расстояние по горизонтали на мини-карте. Только текущий сеанс.", "Shift + left-click anywhere on the large map to set a waypoint. Shift + right-click to clear without deleting pins. Shows direction and horizontal distance on the minimap. Current session only."),
        ["Показывать никнеймы"] = ("Show player names", "Никнеймы под иконками других игроков с открытой позицией. Имя своего персонажа не показывается. Большая карта не меняется.", "Names below the icons of other players sharing their position. Your own name is not shown. The large map is unchanged."),
        ["Размер шрифта никнеймов"] = ("Player name font size", "Размер текста под иконками игроков, 4–20, по умолчанию 8. Не влияет на биом и координаты.", "Font size below player icons, 4–20, default 8. Does not affect the biome or coordinates."),
        ["Обводка никнеймов"] = ("Player name outline", "Чёрная обводка никнеймов толщиной 2 пикселя. Можно отключить независимо от размера шрифта.", "Black 2-pixel outline around player names. Can be disabled independently of the font size."),
        ["Min zoom"] = ("Min zoom", "Минимальное приближение. 0 — без пользовательского ограничения отдаления.", "Minimum magnification. 0 removes the custom zoom-out limit."),
        ["Max zoom"] = ("Max zoom", "Максимальное приближение относительно ×1. При Min > Max пределы меняются местами.", "Maximum magnification relative to ×1. If Min exceeds Max, the limits are swapped."),
        ["Шаг зума"] = ("Zoom step", "Множитель ступеней зума, привязанных к ×1. Не меняет шаг штатных клавиш.", "Multiplier between zoom levels anchored at ×1. Does not change the native keys' step."),
        ["Сохранять отдельный зум для кораблей"] = ("Separate ship zoom", "Сохранять масштабы суши и корабля между запусками игры. Общие для миров в этом профиле, включая пассажиров.", "Remember land and ship zoom across game sessions. Shared by worlds in this profile, including passengers."),
        ["Плавный зум"] = ("Smooth zoom", "Плавный переход между масштабами мини-карты.", "Smooth transitions between minimap zoom levels."),
        ["Зум через + и -"] = ("Zoom with + and -", "Включить клавиши мода и NumPad +/−. Штатные клавиши игры не отключаются.", "Enable this mod's shortcuts and NumPad +/−. Native game shortcuts remain available."),
        ["Приблизить"] = ("Zoom in", "Клавиша приближения. По умолчанию = / +; None отключает эту привязку.", "Zoom-in shortcut. Default: = / +. None disables this binding."),
        ["Отдалить"] = ("Zoom out", "Клавиша отдаления. По умолчанию минус; None отключает эту привязку.", "Zoom-out shortcut. Default: minus. None disables this binding."),
        ["Показывать линии компаса"] = ("Show compass lines", "Линии север–юг и запад–восток. Не влияет на буквы.", "North–south and east–west lines. Direction letters are independent."),
        ["Показывать направления"] = ("Show directions", "Буквы N/E/S/W по краям карты. Всегда остаются вертикальными.", "N/E/S/W letters along the map edge. Letters always remain upright."),
        ["Тип линии"] = ("Line style", "Пунктирная или сплошная линия.", "Dashed or solid lines."),
        ["Цвет линии"] = ("Line color", "HEX #RRGGBB или #RRGGBBAA. Прозрачность: 00 — невидимо, FF — непрозрачно.", "HEX #RRGGBB or #RRGGBBAA. Alpha: 00 is invisible, FF is opaque."),
        ["Толщина линии"] = ("Line thickness", "Толщина линий: 0,1–3,0. Размер букв не меняется.", "Line thickness: 0.1–3.0. Does not change letter size.")
    };

    public static string Pick(bool ru, string russian, string english) => ru ? russian : english;
    public static string TargetSelected(double meters, bool ru) =>
        Pick(ru, "Цель выбрана (", "Target selected (") +
        Math.Round(meters, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) +
        Pick(ru, " м)", " m)");
    public static string ResetLabel(bool vanilla, bool ru) => vanilla
        ? Pick(ru, "Сбросить к настройкам игры", "Reset to Vanilla")
        : Pick(ru, "Сбросить к настройкам мода", "Reset to mod default");
    public static string PanelText(string english, bool ru)
    {
        if (!ru) return english;
        switch (english)
        {
            case "Save": return "Сохранить";
            case "Close": return "Закрыть";
            case "Saved": return "Сохранено";
            case "Unsaved changes.": return "Есть несохранённые изменения.";
            case "This value is not valid.": return "Некорректное значение.";
            case "(none)": return "(нет)";
            case "press a key...": return "нажмите клавишу…";
            case "Press the new key (Esc = cancel, Backspace = unbind).": return "Нажмите клавишу (Esc — отмена, Backspace — убрать привязку).";
            case "(not editable here)": return "(здесь не редактируется)";
            case "Changes go into this mod's config file after Save. Some mods need a restart. On a server, mods that sync their settings use the server's values.":
                return "Изменения применяются после сохранения. Перезапуск игры не требуется.";
            default: return english;
        }
    }
    public static bool HasEntry(string key) => Entries.ContainsKey(key);
    public static string Name(string key, bool ru) => !ru && Entries.TryGetValue(key, out var text) ? text.Name : key;
    public static string Description(string key, bool ru, string fallback) =>
        Entries.TryGetValue(key, out var text) ? (ru ? text.Ru : text.En) : fallback;
    public static string Section(string section, bool ru)
    {
        if (ru) return section;
        switch (section)
        {
            case "1. Общее": return "1. General";
            case "2. Карта": return "2. Map";
            case "3. Зум": return "3. Zoom";
            case "4. Клавиши": return "4. Shortcuts";
            case "5. Компас": return "5. Compass";
            case "6. HUD": return "6. HUD";
            case "7. Навигация": return "7. Navigation";
            case "8. Игроки": return "8. Players";
            default: return section;
        }
    }

    public static string Choice(string type, string value, bool ru)
    {
        switch (type + "." + value)
        {
            case "MapLighting.TimeOfDay": return Pick(ru, "По времени суток", "Time of day");
            case "MapLighting.Day": return Pick(ru, "Всегда день", "Always day");
            case "MapLighting.Night": return Pick(ru, "Всегда ночь", "Always night");
            case "HudSlotContent.Empty": return Pick(ru, "Пусто", "Empty");
            case "HudSlotContent.Biome": return Pick(ru, "Биом", "Biome");
            case "HudSlotContent.Wind": return Pick(ru, "Ветер", "Wind");
            case "HudSlotContent.Coordinates": return Pick(ru, "Координаты", "Coordinates");
            case "MapShape.Square": return Pick(ru, "Квадратная", "Square");
            case "MapShape.Circle": return Pick(ru, "Круглая", "Circle");
            case "RotationMode.NorthUp": return Pick(ru, "Север сверху", "North up");
            case "RotationMode.CameraUp": return Pick(ru, "По камере", "Camera up");
            case "RotationMode.CharacterUp": return Pick(ru, "По персонажу", "Character up");
            case "CompassLineType.Dashed": return Pick(ru, "Пунктирная", "Dashed");
            case "CompassLineType.Solid": return Pick(ru, "Сплошная", "Solid");
            default: return value;
        }
    }
}
