using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

public enum RotationMode { NorthUp, CameraUp, CharacterUp }
public enum MapShape { Square, Circle }
public enum BiomePosition { Vanilla, Above, Below }
public enum WindPosition { Vanilla, TopLeft, TopRight, BottomLeft, BottomRight, Top, Bottom }
public enum CompassLineType { Dashed, Solid }
public enum MapLighting { TimeOfDay, Day, Night }

[BepInPlugin(Id, "Better Minimap", "1.0.1")]
[BepInProcess("valheim.exe")]
[BepInDependency("com.morda.storeandcraft", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Id = "net.shtab.rotatingminimap";
    internal static Plugin? Instance;
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _modEnabled = null!;
    private Minimap? _zoomReferenceMap;
    private float _zoomReference;
    private bool _resetZoomOnEnable;
    private ConfigEntry<RotationMode> _mode = null!;
    private ConfigEntry<bool> _zoomKeys = null!;
    private ConfigEntry<KeyboardShortcut> _zoomInKey = null!;
    private ConfigEntry<KeyboardShortcut> _zoomOutKey = null!;
    private ConfigEntry<bool> _showLines = null!, _showLetters = null!;
    private ConfigEntry<MapShape> _shape = null!;
    private ConfigEntry<MapLighting> _lighting = null!;
    private MapLightingView? _lightingView;
    private ConfigEntry<int> _sizePercent = null!;
    private ConfigEntry<BiomePosition> _biomePosition = null!;
    private ConfigEntry<WindPosition> _windPosition = null!;
    private ConfigEntry<bool> _separateShipZoom = null!;
    private ConfigEntry<bool> _smoothZoomEnabled = null!;
    private readonly SmoothZoom _smoothZoom = new SmoothZoom();
    private bool _writingZoom;
    private ConfigEntry<string> _lineColor = null!;
    private ConfigEntry<float> _lineThickness = null!;
    private ConfigEntry<CompassLineType> _lineType = null!;
    private ShipZoomMemory _shipZoom = new ShipZoomMemory();
    private string _zoomMemoryPath = "";
    private double _savedLandZoom, _savedShipZoom;
    private bool _zoomPersistenceFailed;
    private ZoomIndicator? _zoomIndicator;
    private bool _zoomContextChanged;
    private MapScaleView? _scaleView;
    private HudSpacing? _hudSpacing;
    private MapShapeView? _shapeView;
    private ConfigEntry<float> _minZoom = null!, _maxZoom = null!, _zoomStep = null!;
    private Compass? _compass;
    private float _baseZoom;
    private Harmony? _harmony;
    private Minimap? _map;
    private MapUvRotation? _effect;
    private float _heading;
    private bool _rotating;
    private bool _failed;
    private bool _markersChanged;
    private Quaternion _playerRotation, _shipRotation, _windRotation;
    private FieldInfo? _pinDirty;
    private readonly HashSet<ConfigDefinition> _storedKeys = new HashSet<ConfigDefinition>();

    private void Awake()
    {
        Instance = this;
        _zoomMemoryPath = Config.ConfigFilePath + ".zoom";
        try
        {
            if (ZoomMemoryStore.TryLoad(_zoomMemoryPath, out var land, out var ship))
            {
                _shipZoom.ResetValues(land, ship);
                _savedLandZoom = land; _savedShipZoom = ship;
            }
            else if (File.Exists(_zoomMemoryPath)) Logger.LogWarning("Better Minimap: некорректный файл памяти зума; используются текущие масштабы.");
        }
        catch (Exception error) { Logger.LogWarning($"Better Minimap: не удалось прочитать память зума: {error.Message}"); }
        ReadStoredKeys();
        bool saveOnSet = Config.SaveOnConfigSet;
        Config.SaveOnConfigSet = false;
        _modEnabled = Config.Bind("1. Карта", "Мод включён", true,
            "Включить все функции Better Minimap. При выключении возвращается штатная мини-карта; настройки сохраняются.");
        _enabled = MoveSetting("1. Карта", "Вращение", "Включено", true,
            "Вращать только мини-карту в углу. Большая карта M не изменяется.");
        _mode = MoveSetting("1. Карта", "Режим", "Режим", RotationMode.CameraUp,
            "NorthUp — север сверху; CameraUp — по камере; CharacterUp — по персонажу. Применяется сразу.");
        _shape = Config.Bind("1. Карта", "Форма", MapShape.Circle,
            "Square — квадратная; Circle — круглая. Только мини-карта, применяется сразу.");
        _biomePosition = Config.Bind("1. Карта", "Положение биома", BiomePosition.Above,
            "По умолчанию / сверху / снизу. Для обеих форм карты.");
        _windPosition = Config.Bind("1. Карта", "Положение ветра", WindPosition.Bottom,
            "По умолчанию либо снаружи карты: сверху/снизу по центру или у левого/правого края. Направление стрелки не меняется.");
        // Consume obsolete entries before removing them, including BepInEx orphaned values.
        Config.Bind("1. Карта", "Обводка биома и ветра", true);
        Config.Remove(new ConfigDefinition("1. Карта", "Обводка биома и ветра"));
        _sizePercent = Config.Bind("1. Карта", "Размер мини-карты", 130,
            new ConfigDescription("Размер всего блока в процентах: карта, метки, компас, биом и ветер. 100 — штатный размер. Выше 100 бафы сдвигаются влево на добавленную ширину карты.",
                new AcceptableValueRange<int>(50, 150)));
        var obsoleteZoom = MoveSetting("3. Клавиши", "Зум клавишами", "Зум клавишами", true, "");
        Config.Remove(obsoleteZoom.Definition);
        _zoomInKey = MoveSetting("3. Клавиши", "Приблизить", "Приблизить", new KeyboardShortcut(KeyCode.Equals),
            "Клавиша или сочетание для приближения. По умолчанию = / +. None — отключить основную привязку.");
        _zoomOutKey = MoveSetting("3. Клавиши", "Отдалить", "Отдалить", new KeyboardShortcut(KeyCode.Minus),
            "Клавиша или сочетание для отдаления. По умолчанию минус. None — отключить основную привязку.");
        _zoomKeys = MoveSetting("3. Клавиши", "Зум через + и -", "Зум через NumPad", true,
            "Включить все клавиши зума этого мода: назначенные ниже и NumPad +/-. Штатные клавиши игры не отключаются. Не срабатывает в меню и при вводе текста.");
        bool hadOldCompass = _storedKeys.Contains(new ConfigDefinition("Мини-карта", "Отображать компас"));
        bool oldCompass = Config.Bind("Мини-карта", "Отображать компас", true).Value;
        Config.Remove(new ConfigDefinition("Мини-карта", "Отображать компас"));
        Config.Bind("Мини-карта", "Показывать север", true);
        Config.Remove(new ConfigDefinition("Мини-карта", "Показывать север"));
        _showLines = Config.Bind("4. Компас", "Показывать линии компаса", false,
            "Линии север–юг и запад–восток. Тип, цвет и толщина настраиваются ниже; буквы включаются отдельно.");
        _showLetters = Config.Bind("4. Компас", "Показывать направления", true,
            "Буквы N, E, S, W по краям карты, независимо от линий. Буквы остаются вертикальными.");
        _lineType = Config.Bind("4. Компас", "Тип линии", CompassLineType.Dashed,
            "Dashed — пунктирная; Solid — сплошная.");
        _lineColor = Config.Bind("4. Компас", "Цвет линии", "#505050BB",
            "Цвет в формате #RRGGBB или #RRGGBBAA. Последние две цифры задают прозрачность: 00 — невидимо, FF — непрозрачно. Не влияет на буквы.");
        _lineThickness = Config.Bind("4. Компас", "Толщина линии", .8f,
            new ConfigDescription("Толщина линий в единицах UI, не размер букв.", new AcceptableValueRange<float>(.1f, 3f)));
        if (hadOldCompass)
        {
            if (!_storedKeys.Contains(_showLines.Definition)) _showLines.Value = oldCompass;
            if (!_storedKeys.Contains(_showLetters.Definition)) _showLetters.Value = oldCompass;
        }
        _minZoom = MoveSetting("2. Зум", "Min zoom", "Min zoom", 0.1f,
            new ConfigDescription("Минимальное приближение относительно масштаба при входе (x1). 0 — без нашего ограничения отдаления, но не дальше полного мира. Значение округляется до 0,1.", new AcceptableValueRange<float>(0f, 20f)));
        _maxZoom = MoveSetting("2. Зум", "Max zoom", "Max zoom", 4f,
            new ConfigDescription("Максимальное приближение относительно масштаба при входе в мир. Больше — ближе. Если Min > Max, значения меняются местами при расчёте.", new AcceptableValueRange<float>(0.1f, 20f)));
        _zoomStep = MoveSetting("2. Зум", "Шаг зума", "Шаг зума", 1.5f,
            new ConfigDescription("Множитель одного шага дополнительных клавиш зума, с точностью до 0,1.", new AcceptableValueRange<float>(1.1f, 3f)));
        _separateShipZoom = Config.Bind("2. Зум", "Сохранять отдельный зум для кораблей", true,
            "Запоминать масштабы суши и корабля между запусками игры, в локальном профиле. Общие для всех миров и кораблей. Работает и для пассажиров.");
        _smoothZoomEnabled = Config.Bind("2. Зум", "Плавный зум", true,
            "Плавный переход между масштабами. Только мини-карта; ступени и ограничения не меняются.");
        Config.Bind("2. Зум", "Показывать зум", true);
        Config.Remove(new ConfigDefinition("2. Зум", "Показывать зум"));
        Config.Bind("2. Зум", "Штатные пределы зума", false);
        Config.Remove(new ConfigDefinition("2. Зум", "Штатные пределы зума"));
        _modEnabled = RebindSection(_modEnabled, "1. Общее");
        _enabled = RebindSection(_enabled, "2. Карта");
        _mode = RebindSection(_mode, "2. Карта");
        _shape = RebindSection(_shape, "2. Карта");
        _biomePosition = RebindSection(_biomePosition, "2. Карта");
        _windPosition = RebindSection(_windPosition, "2. Карта");
        _sizePercent = RebindSection(_sizePercent, "2. Карта");
        _minZoom = RebindSection(_minZoom, "3. Зум");
        _maxZoom = RebindSection(_maxZoom, "3. Зум");
        _zoomStep = RebindSection(_zoomStep, "3. Зум");
        _separateShipZoom = RebindSection(_separateShipZoom, "3. Зум");
        _smoothZoomEnabled = RebindSection(_smoothZoomEnabled, "3. Зум");
        _zoomInKey = RebindSection(_zoomInKey, "4. Клавиши");
        _zoomOutKey = RebindSection(_zoomOutKey, "4. Клавиши");
        _zoomKeys = RebindSection(_zoomKeys, "4. Клавиши");
        _showLines = RebindSection(_showLines, "5. Компас");
        _showLetters = RebindSection(_showLetters, "5. Компас");
        _lineType = RebindSection(_lineType, "5. Компас");
        _lineColor = RebindSection(_lineColor, "5. Компас");
        _lineThickness = RebindSection(_lineThickness, "5. Компас");
        _lighting = Config.Bind("2. Карта", "Освещение карты", MapLighting.TimeOfDay,
            "По времени суток, всегда день или всегда ночь. Только мини-карта, без изменения освещения мира.");
        RoundEntry(_minZoom, 0, 20);
        RoundEntry(_maxZoom, .1f, 20);
        RoundEntry(_zoomStep, 1.1f, 3);
        RoundEntry(_lineThickness, .1f, 3);
        Config.SaveOnConfigSet = saveOnSet;
        Config.Save();
        _harmony = new Harmony(Id);
        try
        {
            _pinDirty = AccessTools.Field(typeof(Minimap), "m_pinUpdateRequired")
                ?? throw new MissingFieldException("Minimap.m_pinUpdateRequired");
            _harmony.PatchAll(typeof(Plugin).Assembly);
            try { ModManagerBridge.Install(); }
            catch (Exception error) { Logger.LogWarning($"Интеграция окна настроек недоступна: {error.Message}"); }
            Logger.LogInfo("Вращение мини-карты загружено. Настройки доступны в менеджере конфигов BepInEx.");
        }
        catch (Exception error)
        {
            _harmony.UnpatchSelf();
            _failed = true;
            Logger.LogError($"Неподдерживаемый API игры; мод отключён: {error}");
        }
    }

    private ConfigEntry<T> MoveSetting<T>(string section, string key, string oldKey, T fallback, ConfigDescription description)
    {
        var old = Config.Bind("Мини-карта", oldKey, fallback);
        var entry = Config.Bind(section, key, fallback, description);
        // Preserve old saved values, but never turn them into the factory defaults.
        if (!_storedKeys.Contains(entry.Definition) && _storedKeys.Contains(old.Definition))
            entry.Value = old.Value;
        Config.Remove(old.Definition);
        return entry;
    }

    private ConfigEntry<T> RebindSection<T>(ConfigEntry<T> old, string section)
    {
        var entry = Config.Bind(section, old.Definition.Key, (T)old.DefaultValue, old.Description);
        if (!_storedKeys.Contains(entry.Definition)) entry.Value = old.Value;
        Config.Remove(old.Definition);
        return entry;
    }

    private void ReadStoredKeys()
    {
        if (!File.Exists(Config.ConfigFilePath)) return;
        string section = "";
        foreach (string line in File.ReadLines(Config.ConfigFilePath))
        {
            string text = line.Trim();
            if (text.Length == 0 || text.StartsWith("#") || text.StartsWith(";")) continue;
            if (text.StartsWith("[") && text.EndsWith("]"))
            {
                section = text.Substring(1, text.Length - 2).Trim();
                continue;
            }
            int equals = text.IndexOf('=');
            if (section.Length > 0 && equals > 0)
                _storedKeys.Add(new ConfigDefinition(section, text.Substring(0, equals).Trim()));
        }
    }

    private ConfigEntry<T> MoveSetting<T>(string section, string key, string oldKey, T fallback, string description) =>
        MoveSetting(section, key, oldKey, fallback, new ConfigDescription(description));

    private static void RoundEntry(ConfigEntry<float> entry, float min, float max)
    {
        void Normalize(object? sender, EventArgs args)
        {
            float rounded = (float)MapMath.RoundSetting(entry.Value, min, max);
            if (entry.Value != rounded) entry.Value = rounded;
        }
        entry.SettingChanged += Normalize;
        Normalize(null, EventArgs.Empty);
    }

    internal bool Active(Minimap map) => isActiveAndEnabled && _modEnabled.Value && !_failed && _rotating && _map == map &&
        map.m_mode == Minimap.MapMode.Small;

    internal void BeforeMap(Minimap map)
    {
        if (_failed || !isActiveAndEnabled) return;
        try
        {
            if (!_modEnabled.Value)
            {
                if (_map)
                {
                    var previousMap = _map!;
                    float originalZoom = _baseZoom;
                    Release();
                    // ControlsZoom is false: use the game's setter and native limits.
                    previousMap.SmallZoom = originalZoom;
                }
                return;
            }
            if (_map != map)
            {
                Release();
                _map = map;
                // Keep x1 stable when toggling the mod within the same world.
                if (_zoomReferenceMap != map)
                {
                    _zoomReferenceMap = map;
                    _zoomReference = map.SmallZoom;
                }
                _baseZoom = _zoomReference;
                _smoothZoom.Reset(_resetZoomOnEnable ? _baseZoom : map.SmallZoom);
                _resetZoomOnEnable = false;
                _scaleView = new MapScaleView(map);
                _hudSpacing = new HudSpacing(map, _scaleView);
                _zoomIndicator = new ZoomIndicator(map);
                _lightingView = new MapLightingView(map);
            }
            RestoreMarkers();
            _scaleView?.Apply(_sizePercent.Value);
            if (_shapeView == null) _shapeView = new MapShapeView(map);
            _shapeView.Apply(_shape.Value == MapShape.Circle);
            if (map.m_mode == Minimap.MapMode.Small)
            {
                if (Player.m_localPlayer)
                {
                    double wantedFactor = _shipZoom.Update(_separateShipZoom.Value,
                        Ship.GetLocalShip() != null, _baseZoom / _smoothZoom.Target);
                    float wanted = (float)(_baseZoom / wantedFactor);
                    _zoomContextChanged = _shipZoom.ChangedContext;
                    if (!Mathf.Approximately(wanted, (float)_smoothZoom.Target))
                    {
                        QueueZoom(map, wanted);
                        _pinDirty!.SetValue(map, true);
                    }
                }
                _smoothZoom.Request(map.SmallZoom, LimitZoom(map, (float)_smoothZoom.Target));
                RememberZoom();
            }
            // Finish off-screen transitions before returning from the large map.
            float rendered = (float)_smoothZoom.Advance(Time.unscaledDeltaTime,
                _smoothZoomEnabled.Value && map.m_mode == Minimap.MapMode.Small);
            if (map.m_mode == Minimap.MapMode.Small) rendered = LimitZoom(map, rendered);
            if (map.m_mode == Minimap.MapMode.Small && map.SmallZoom != rendered) WriteRenderedZoom(map, rendered);
            bool previous = _rotating;
            float oldHeading = _heading;
            var player = Player.m_localPlayer;
            var camera = Utils.GetMainCamera();
            _rotating = _enabled.Value && _mode.Value != RotationMode.NorthUp &&
                player && camera && map.m_mode == Minimap.MapMode.Small;
            _heading = !_rotating ? 0f : _mode.Value == RotationMode.CharacterUp
                ? player!.transform.eulerAngles.y : camera!.transform.eulerAngles.y;
            // Vanilla can skip UpdatePins when only the camera moves.
            if (previous != _rotating || !Mathf.Approximately(oldHeading, _heading))
                _pinDirty!.SetValue(map, true);
        }
        catch (Exception error) { Fail(error); }
    }

    internal void AfterMap(Minimap map)
    {
        if (_failed || !isActiveAndEnabled || !_modEnabled.Value) return;
        try
        {
            // Update can switch map mode or return early (death/menu).
            bool active = Active(map) && Player.m_localPlayer && map.m_mapImageSmall;
            _lightingView?.Apply(_lighting.Value, EnvMan.instance ? EnvMan.instance.GetDayFraction() : .5f);
            _shapeView?.Apply(_shape.Value == MapShape.Circle);
            _hudSpacing?.Apply(_sizePercent.Value, _biomePosition.Value, _windPosition.Value,
                _shape.Value == MapShape.Circle);
            _zoomIndicator?.Update(_baseZoom, (float)_smoothZoom.Target, _shipZoom.ShipMode, _zoomContextChanged,
                _hudSpacing?.ZoomIndicatorOffset ?? 0);
            _zoomContextChanged = false;
            UpdateNorth(map, active ? _heading : 0f);
            if (!active)
            {
                if (_effect) _effect.SetHeading(0);
                return;
            }
            if (!_effect)
                _effect = map.m_mapImageSmall!.gameObject.AddComponent<MapUvRotation>();
            _effect!.SetHeading(_heading);
            _playerRotation = map.m_smallMarker.rotation;
            _shipRotation = map.m_smallShipMarker.rotation;
            _windRotation = map.m_windMarker.rotation;
            _markersChanged = true;
            Quaternion offset = Quaternion.Euler(0, 0, _heading);
            map.m_smallMarker.rotation = offset * _playerRotation;
            map.m_smallShipMarker.rotation = offset * _shipRotation;
            map.m_windMarker.rotation = offset * _windRotation;
        }
        catch (Exception error) { Fail(error); }
    }

    private void RestoreMarkers()
    {
        if (!_markersChanged) return;
        _markersChanged = false;
        if (!_map) return;
        if (_map!.m_smallMarker) _map.m_smallMarker.rotation = _playerRotation;
        if (_map.m_smallShipMarker) _map.m_smallShipMarker.rotation = _shipRotation;
        if (_map.m_windMarker) _map.m_windMarker.rotation = _windRotation;
    }

    internal void Zoom(Minimap map, bool takeInput)
    {
        if (_failed || !isActiveAndEnabled || !_modEnabled.Value || !_zoomKeys.Value || !takeInput ||
            map.m_mode != Minimap.MapMode.Small || Cursor.visible || ZInput.VirtualKeyboardOpen) return;
        try
        {
            // Do not add a second step when the same key already triggers vanilla zoom.
            if (ZInput.GetButtonDown("MapZoomIn") || ZInput.GetButtonDown("MapZoomOut")) return;
            bool zoomIn = ShortcutDown(_zoomInKey.Value) ||
                ShortcutDown(new KeyboardShortcut(KeyCode.KeypadPlus));
            bool zoomOut = ShortcutDown(_zoomOutKey.Value) ||
                ShortcutDown(new KeyboardShortcut(KeyCode.KeypadMinus));
            if (zoomIn == zoomOut) return;
            QueueZoom(map, (float)MapMath.AnchoredZoomStep(_smoothZoom.Target, _baseZoom, zoomIn, zoomOut, _zoomStep.Value));
            _pinDirty!.SetValue(map, true);
        }
        catch (Exception error) { Fail(error); }
    }

    internal float LimitZoom(Minimap map, float value)
    {
        if (_failed || !isActiveAndEnabled || !_modEnabled.Value || _map != map || _baseZoom <= 0 ||
            map.m_mode != Minimap.MapMode.Small) return value;
        return (float)MapMath.ClampZoom(value, _baseZoom, _minZoom.Value, _maxZoom.Value);
    }

    internal float SetZoom(Minimap map, float current, float requested)
    {
        if (_writingZoom) return LimitZoom(map, requested);
        if (ZoomKeysPatch.Updating == map)
        {
            // Vanilla calls the setter even on idle frames. Its input step is relative
            // to the rendered zoom; apply that ratio to the target for rapid presses.
            if (requested == current) return current;
            if (current > 0) requested = (float)(_smoothZoom.Target * requested / current);
        }
        _smoothZoom.Request(current, LimitZoom(map, requested));
        RememberZoom();
        if (!_smoothZoomEnabled.Value) return (float)_smoothZoom.Advance(0, false);
        return LimitZoom(map, current);
    }

    private void QueueZoom(Minimap map, float target)
    {
        _smoothZoom.Request(map.SmallZoom, LimitZoom(map, target));
        RememberZoom();
        if (!_smoothZoomEnabled.Value) WriteRenderedZoom(map, (float)_smoothZoom.Advance(0, false));
    }

    private void WriteRenderedZoom(Minimap map, float value)
    {
        _writingZoom = true;
        try { map.SmallZoom = value; }
        finally { _writingZoom = false; }
        _pinDirty!.SetValue(map, true);
    }

    internal bool ControlsZoom(Minimap map) => !_failed && isActiveAndEnabled && _modEnabled.Value &&
        _map == map && _baseZoom > 0 && map.m_mode == Minimap.MapMode.Small;

    internal void ResetZoomToDefault()
    {
        if (_failed || !isActiveAndEnabled) return;
        _shipZoom.ResetValues(1, 1);
        _shipZoom.Suspend();
        SaveZoomMemory();
        if (!_modEnabled.Value || !_map || _baseZoom <= 0)
        {
            _resetZoomOnEnable = true;
            return;
        }
        _smoothZoom.Reset(_baseZoom);
        if (_map!.m_mode == Minimap.MapMode.Small) WriteRenderedZoom(_map, _baseZoom);
    }

    private void RememberZoom()
    {
        if (_baseZoom <= 0 || _smoothZoom.Target <= 0) return;
        _shipZoom.Remember(_baseZoom / _smoothZoom.Target);
        SaveZoomMemory();
    }

    private void SaveZoomMemory()
    {
        if (_zoomPersistenceFailed || !_shipZoom.HasValues ||
            (_savedLandZoom == _shipZoom.Land && _savedShipZoom == _shipZoom.Ship)) return;
        try
        {
            ZoomMemoryStore.Save(_zoomMemoryPath, _shipZoom.Land, _shipZoom.Ship);
            _savedLandZoom = _shipZoom.Land; _savedShipZoom = _shipZoom.Ship;
        }
        catch (Exception error)
        {
            _zoomPersistenceFailed = true;
            Logger.LogWarning($"Better Minimap: запись памяти зума недоступна до перезапуска; масштабы хранятся в текущем сеансе. {error.Message}");
        }
    }

    internal void WarnSettings(Exception error) => Logger.LogWarning(
        $"Better Minimap: интеграция F10 отключена; конфиг доступен напрямую. {error}");

    // Use the game's input layer instead of KeyboardShortcut.IsDown (legacy Unity Input).
    private static bool ShortcutDown(KeyboardShortcut shortcut)
    {
        if (shortcut.MainKey == KeyCode.None) return false;
        foreach (var key in shortcut.Modifiers)
            if (!ZInput.GetKey(key)) return false;
        foreach (var key in ShortcutModifiers)
        {
            if (!ZInput.GetKey(key) || key == shortcut.MainKey) continue;
            bool allowed = false;
            foreach (var modifier in shortcut.Modifiers)
                if (key == modifier) allowed = true;
            // Allow Shift with both default main-keyboard zoom bindings (+ and _).
            if ((shortcut.MainKey == KeyCode.Equals || shortcut.MainKey == KeyCode.Minus) &&
                (key == KeyCode.LeftShift || key == KeyCode.RightShift)) allowed = true;
            if (!allowed) return false;
        }
        return ZInput.GetKeyDown(shortcut.MainKey);
    }

    private static readonly KeyCode[] ShortcutModifiers = {
        KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt,
        KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftCommand, KeyCode.RightCommand
    };

    private void UpdateNorth(Minimap map, float heading)
    {
        if (!map.m_mapImageSmall) return;
        bool visible = (_showLines.Value || _showLetters.Value) && map.m_mode == Minimap.MapMode.Small && Player.m_localPlayer;
        if (visible && _compass == null) _compass = new Compass(map);
        _compass?.Show(visible, map.m_mapImageSmall.rectTransform.rect, heading,
            _showLines.Value, _showLetters.Value, _shape.Value == MapShape.Circle,
            ColorUtility.TryParseHtmlString(_lineColor.Value, out var color) ? color : new Color32(80, 80, 80, 187),
            _lineThickness.Value, _lineType.Value == CompassLineType.Dashed);
    }

    private void Fail(Exception error)
    {
        _failed = true;
        Release();
        Logger.LogError($"Вращение отключено после ошибки; возвращена штатная карта: {error}");
    }

    private void Release()
    {
        RestoreMarkers();
        _zoomIndicator?.Dispose();
        _zoomIndicator = null;
        _shipZoom.Suspend();
        _zoomContextChanged = false;
        _hudSpacing?.Dispose();
        _hudSpacing = null;
        _compass?.Destroy();
        _compass = null;
        _shapeView?.Dispose();
        _shapeView = null;
        _lightingView?.Dispose();
        _lightingView = null;
        _scaleView?.Dispose();
        _scaleView = null;
        _baseZoom = 0;
        _smoothZoom.Reset(0);
        if (_effect)
        {
            _effect!.SetHeading(0);
            Destroy(_effect);
        }
        if (_map) _pinDirty?.SetValue(_map, true);
        _effect = null;
        _map = null;
        _rotating = false;
    }

    private void OnDestroy()
    {
        ModManagerBridge.Uninstall();
        Release();
        _harmony?.UnpatchSelf();
        Instance = null;
    }

    private void OnDisable() => Release();

    internal Vector2 RotatePin(Vector2 position, Rect rect)
    {
        var rotated = MapMath.Rotate(position.x - rect.width / 2f,
            position.y - rect.height / 2f, _heading);
        return new Vector2((float)rotated.X + rect.width / 2f, (float)rotated.Y + rect.height / 2f);
    }

    internal bool IsVisible(Minimap map, Vector3 point, RawImage image)
    {
        Rect uv = image.uvRect, rect = image.rectTransform.rect;
        if (uv.width <= 0 || uv.height <= 0 || rect.width <= 0 || rect.height <= 0) return false;
        // Same conversion as vanilla WorldToMapPoint; no exploration or network state changes.
        double u = (point.x / map.m_pixelSize + map.m_textureSize / 2) / map.m_textureSize;
        double v = (point.z / map.m_pixelSize + map.m_textureSize / 2) / map.m_textureSize;
        var position = MapMath.MapToScreen(u, v, uv.center.x, uv.center.y,
            uv.width, uv.height, rect.width, rect.height, _heading);
        return MapMath.Inside(position.X, position.Y, rect.width, rect.height);
    }
}

[HarmonyPatch(typeof(Minimap), "UpdateMap")]
internal static class ZoomKeysPatch
{
    internal static Minimap? Updating;
    private static void Prefix(Minimap __instance, bool __2, out Minimap? __state)
    {
        __state = Updating;
        Updating = __instance;
        Plugin.Instance?.Zoom(__instance, __2);
    }
    private static Exception? Finalizer(Exception? __exception, Minimap? __state)
    {
        Updating = __state;
        return __exception;
    }
}

[HarmonyPatch(typeof(Minimap), "set_SmallZoom")]
internal static class ZoomLimitsPatch
{
    private static bool Prefix(Minimap __instance, float __0, ref float ___m_smallZoom)
    {
        var plugin = Plugin.Instance;
        if (plugin == null || !plugin.ControlsZoom(__instance)) return true;
        // Vanilla clamps to 0.01, which is already the default minimap zoom.
        // Override only the small-map value; never change shared min/max fields.
        ___m_smallZoom = plugin.SetZoom(__instance, ___m_smallZoom, __0);
        return false;
    }
}

[HarmonyPatch(typeof(Minimap), "Update")]
internal static class MapUpdatePatch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    private static void Prefix(Minimap __instance) => Plugin.Instance?.BeforeMap(__instance);
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    private static void Postfix(Minimap __instance) => Plugin.Instance?.AfterMap(__instance);
}

// Limit coordinate changes strictly to the UpdatePins call, never map clicks or large-map tools.
[HarmonyPatch(typeof(Minimap), "UpdatePins")]
internal static class PinScopePatch
{
    internal static Minimap? Updating;
    private static void Prefix(Minimap __instance, out Minimap? __state)
    {
        __state = Updating;
        Updating = __instance;
    }
    private static Exception? Finalizer(Exception? __exception, Minimap? __state)
    {
        Updating = __state;
        return __exception;
    }
}

[HarmonyPatch(typeof(Minimap), "MapPointToLocalGuiPos",
    new[] { typeof(float), typeof(float), typeof(Rect), typeof(Rect) })]
internal static class PinPositionPatch
{
    private static void Postfix(Minimap __instance, Rect __3, ref Vector2 __result)
    {
        var plugin = Plugin.Instance;
        if (plugin != null && PinScopePatch.Updating == __instance && plugin.Active(__instance))
            __result = plugin.RotatePin(__result, __3);
    }
}

[HarmonyPatch(typeof(Minimap), "IsPointVisible")]
internal static class PinVisibilityPatch
{
    private static void Postfix(Minimap __instance, Vector3 __0, RawImage __1, ref bool __result)
    {
        var plugin = Plugin.Instance;
        if (plugin != null && PinScopePatch.Updating == __instance && plugin.Active(__instance) &&
            __1 == __instance.m_mapImageSmall)
            __result = plugin.IsVisible(__instance, __0, __1);
    }
}
