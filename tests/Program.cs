using Shtab.RotatingMinimap;

int checks = 0;
void Near(double actual, double expected)
{
    checks++;
    if (Math.Abs(actual - expected) > 1e-8)
        throw new Exception($"Expected {expected}, got {actual}");
}
void Check(bool result)
{
    checks++;
    if (!result) throw new Exception("Assertion failed");
}

// Looking east: east goes up, north goes left.
var east = MapMath.Rotate(1, 0, 90);
Near(east.X, 0); Near(east.Y, 1);
var north = MapMath.Rotate(0, 1, 90);
Near(north.X, -1); Near(north.Y, 0);

foreach (double width in new[] { 160d, 240d, 450d })
foreach (double height in new[] { 160d, 300d })
foreach (double zoom in new[] { .005, .02, .1 })
foreach (double angle in new[] { 0d, 45d, 90d, 179d, 270d, 359d, 360d, -45d })
foreach (double x in new[] { -width / 2, 0, width / 2 })
foreach (double y in new[] { -height / 2, 0, height / 2 })
{
    var uv = MapMath.ScreenToMap(x, y, .47, .62, zoom, zoom,
        width, height, angle);
    var screen = MapMath.MapToScreen(uv.U, uv.V, .47, .62, zoom, zoom,
        width, height, angle);
    Near(screen.X, x); Near(screen.Y, y);
    Check(double.IsFinite(uv.U) && double.IsFinite(uv.V));
}

// A pin outside the vanilla rectangle can rotate inside, and vice versa.
var entering = MapMath.Rotate(110, 0, 45);
Check(MapMath.Inside(entering.X, entering.Y, 200, 200));
var leaving = MapMath.Rotate(90, 90, 45);
Check(!MapMath.Inside(leaving.X, leaving.Y, 200, 200));
Check(MapMath.Inside(0, 0, 200, 200));
Check(!MapMath.Inside(100, 0, 200, 200));
Near(MapMath.Rotate(42, -17, 360).X, 42);
Near(MapMath.Rotate(42, -17, 360).Y, -17);
foreach (double angle in new[] { 0d, 45d, 90d, 180d, 270d, 359d })
foreach (double width in new[] { 160d, 300d })
{
    var edge = MapMath.NorthEdge(width, 200, angle, 16);
    Check(MapMath.Inside(edge.X, edge.Y, width, 200));
    Check(Math.Abs(Math.Abs(edge.X) - (width / 2 - 16)) < 1e-8 ||
          Math.Abs(Math.Abs(edge.Y) - 84) < 1e-8);
    var direction = MapMath.Rotate(0, 1, angle);
    Near(edge.X * direction.Y - edge.Y * direction.X, 0);
}
Near(MapMath.NorthEdge(200, 200, 0, 16).Y, 84);
Near(MapMath.NorthEdge(200, 200, 90, 16).X, -84);
Near(MapMath.NorthEdge(200, 200, 180, 16).Y, -84);
Near(MapMath.NorthEdge(200, 200, 270, 16).X, 84);
Near(MapMath.NorthEdge(10, 10, 45, 16).X, 0);
Near(MapMath.ZoomStep(.03, true, false, 1.5), .02);
Near(MapMath.ZoomStep(.02, false, true, 1.5), .03);
Near(MapMath.ZoomStep(.02, true, true, 1.5), .02);
Near(MapMath.ZoomStep(.02, false, false, 1.5), .02);
Near(MapMath.ClampZoom(.01 / 1.5, .01, .5, 4), .01 / 1.5);
Near(MapMath.ClampZoom(.0001, .01, .5, 4), .0025);
Near(MapMath.ClampZoom(1, .01, .5, 4), .02);
Near(MapMath.ClampZoom(1, .01, 4, .5), .02);
Near(MapMath.ClampZoom(.01, .01, 2, 2), .005);
foreach (int side in new[] { 0, 1, 2, 3 })
{
    var edge = MapMath.NorthEdge(200, 200, -side * 90, 6);
    Check(MapMath.Inside(edge.X, edge.Y, 200, 200));
}
Near(MapMath.ClampZoom(.04, .01, 0, 4), .04);
Near(MapMath.ClampZoom(100, .01, 0, 4), 1);
Near(MapMath.ClampZoom(double.PositiveInfinity, .01, 0, 4), 1);
Near(MapMath.ClampZoom(0, .01, 0, 4), .0025);
Near(MapMath.RoundSetting(.593, 0, 20), .6);
Near(MapMath.RoundSetting(7.686, .1, 20), 7.7);
Near(MapMath.RoundSetting(1.05, 1.1, 3), 1.1);
Near(MapMath.RoundSetting(double.NaN, 0, 20), 0);
foreach (double angle in new[] { 0d, 20d, 45d, 90d, 137d, 180d, 270d, 359d })
{
    var p = MapMath.CompassEdge(240, 200, angle, 16, true);
    Near(Math.Sqrt(p.X * p.X + p.Y * p.Y), 84);
    Check(MapMath.InsideCircle(p.X, p.Y, 240, 200));
    var opposite = MapMath.CompassEdge(240, 200, angle + 180, 16, true);
    Near(p.X, -opposite.X); Near(p.Y, -opposite.Y);
    var square = MapMath.CompassEdge(240, 200, angle, 16, false);
    var expected = MapMath.NorthEdge(240, 200, angle, 16);
    Near(square.X, expected.X); Near(square.Y, expected.Y);
}
Near(MapMath.ScaleFactor(50), .5);
Near(MapMath.ScaleFactor(100), 1);
Near(MapMath.ScaleFactor(150), 1.5);
Near(MapMath.ScaleFactor(-10), .5);
Near(MapMath.ScaleFactor(200), 1.5);
foreach (double length in new[] { 0d, 9d, 10d, 11d, 100d, 188d, 265.87d })
{
    var ranges = MapMath.DashRanges(length).ToArray();
    Check(ranges.Length % 2 == 0);
    for (int i = 0; i < ranges.Length; i += 2)
    {
        var positive = ranges[i]; var negative = ranges[i + 1];
        Check(positive.Start >= 5 && positive.End <= length / 2);
        Check(positive.End > positive.Start && positive.End - positive.Start <= 4);
        Near(negative.Start, -positive.End); Near(negative.End, -positive.Start);
        if (i > 0) Check(positive.Start - ranges[i - 2].End >= 5);
    }
}
Check(!MapMath.DashRanges(double.PositiveInfinity).Any());
Near(MapMath.BuffShift(200, 1, 1, 50), 0);
Near(MapMath.BuffShift(200, 1, 1, 100), 0);
Near(MapMath.BuffShift(200, 1, 1, 110), 20);
Near(MapMath.BuffShift(200, 1, 1, 150), 100);
Near(MapMath.BuffShift(200, 1.2, 1, 150), 120);
Near(MapMath.BuffShift(200, 1, .5, 150), 50);
double buffBase = -270;
foreach (int percent in new[] { 100, 110, 150, 110, 100, 50, 150, 100 })
{
    double position = buffBase - MapMath.BuffShift(200, 1, 1, percent);
    Check(position <= buffBase);
    if (percent <= 100) Near(position, buffBase);
}
// Repeated endpoint clamps must not shift the multiplicative scale away from x1.
foreach (double step in new[] { 1.1, 1.5, 2.0, 3.0 })
{
    const double baseline = .01;
    double current = baseline / 4;
    bool passedOneDown = false, passedOneUp = false;
    for (int i = 0; i < 100; i++)
    {
        double next = Math.Clamp(MapMath.AnchoredZoomStep((float)current, (float)baseline, false, true, (float)step), baseline / 4, baseline / .1);
        Check(next >= current - 1e-8);
        current = next;
        passedOneDown |= Math.Abs(current - baseline) < 1e-8;
    }
    Near(current, baseline / .1);
    for (int i = 0; i < 100; i++)
    {
        double next = Math.Clamp(MapMath.AnchoredZoomStep((float)current, (float)baseline, true, false, (float)step), baseline / 4, baseline / .1);
        Check(next <= current + 1e-8);
        current = next;
        passedOneUp |= Math.Abs(current - baseline) < 1e-8;
    }
    Near(current, baseline / 4);
    Check(passedOneDown && passedOneUp);
}
Near(MapMath.AnchoredZoomStep(.01, .01, true, true, 1.5), .01);
Near(MapMath.AnchoredZoomStep(.01, .01, false, false, 1.5), .01);
var smooth = new SmoothZoom();
foreach (double target in new[] { .0025, .1 })
foreach (int fps in new[] { 30, 60, 144 })
{
    smooth.Reset(.01);
    smooth.Request(.01, target);
    Near(smooth.Advance(0, true), .01);
    double previous = .01;
    for (int i = 0; i < fps; i++)
    {
        double rendered = smooth.Advance(1d / fps, true);
        Check(rendered >= Math.Min(.01, target) - 1e-12 && rendered <= Math.Max(.01, target) + 1e-12);
        Check(target < .01 ? rendered <= previous + 1e-12 : rendered >= previous - 1e-12);
        previous = rendered;
        // An unchanged target must not restart the transition every frame.
        smooth.Request(rendered, target);
    }
    Near(previous, target);
}
smooth.Reset(.01);
smooth.Request(.01, .005);
double midway = smooth.Advance(SmoothZoom.Duration / 2, true);
Near(midway, Math.Sqrt(.01 * .005));
// A second fast press is calculated from .005, not the interpolated value.
smooth.Request(midway, MapMath.AnchoredZoomStep(smooth.Target, .01, true, false, 2));
Near(smooth.Target, .0025);
Near(smooth.Advance(0, true), midway);
Near(smooth.Advance(SmoothZoom.Duration, true), .0025);
smooth.Request(.0025, .1);
Near(smooth.Advance(0, false), .1); // disabling snaps to the selected target
smooth.Reset(.01);
Near(smooth.Target, .01);
Near(smooth.Advance(1, true), .01);
foreach (string key in new[] { "Мод включён", "Показывать мини-карту", "Вращение", "Режим", "Форма", "Размер мини-карты",
    "Slot 1", "Slot 2", "Slot 3", "Slot 4", "Slot 5", "Slot 6", "Указатель цели", "Показывать никнеймы", "Размер шрифта никнеймов", "Обводка никнеймов", "Показывать транспорт", "Размер иконок транспорта", "Min zoom", "Max zoom", "Шаг зума", "Сохранять отдельный зум для кораблей",
    "Плавный зум", "Зум через + и -", "Приблизить", "Отдалить", "Показывать линии компаса",
    "Показывать направления", "Тип линии", "Цвет линии", "Толщина линии", "Освещение карты", "Прозрачность карты" })
{
    Check(SettingsText.HasEntry(key));
    Check(SettingsText.Name(key, true) == (key == "Зум через + и -" ? "Зум через доп. клавиши" : key));
    Check(!SettingsText.Name(key, false).Any(c => c >= '\u0400' && c <= '\u04ff'));
    Check(SettingsText.Description(key, true, "MISSING") != "MISSING");
    Check(!SettingsText.Description(key, false, "MISSING").Any(c => c >= '\u0400' && c <= '\u04ff'));
}
Check(SettingsText.Section("1. Общее", false) == "1. General");
Check(SettingsText.Section(SettingsLayout.Map, false) == "2. Map appearance");
Check(SettingsText.Section(SettingsLayout.Zoom, false) == "3. Zoom and controls");
Check(SettingsText.Section(SettingsLayout.Hud, false) == "4. Around the map");
Check(SettingsText.Section(SettingsLayout.Navigation, false) == "5. Compass and navigation");
Check(SettingsText.Section(SettingsLayout.Transport, false) == "7. Transport");
var settingGroups = new[] {
    (SettingsLayout.General, new[] { "Мод включён", "Показывать мини-карту" }),
    (SettingsLayout.Map, new[] { "Форма", "Размер мини-карты", "Освещение карты", "Прозрачность карты", "Вращение", "Режим" }),
    (SettingsLayout.Zoom, new[] { "Min zoom", "Max zoom", "Шаг зума", "Плавный зум", "Сохранять отдельный зум для кораблей", "Зум через + и -", "Приблизить", "Отдалить" }),
    (SettingsLayout.Hud, new[] { "Slot 1", "Размер текста Slot 1", "Slot 2", "Размер текста Slot 2",
        "Slot 3", "Размер текста Slot 3", "Slot 4", "Размер текста Slot 4", "Slot 5", "Размер текста Slot 5", "Slot 6", "Размер текста Slot 6" }),
    (SettingsLayout.Navigation, new[] { "Показывать направления", "Показывать линии компаса", "Тип линии", "Толщина линии", "Цвет линии", "Указатель цели" }),
    (SettingsLayout.Players, new[] { "Показывать никнеймы", "Размер шрифта никнеймов", "Обводка никнеймов" }),
    (SettingsLayout.Transport, new[] { "Показывать транспорт", "Размер иконок транспорта" })
};
Check(settingGroups.Length == 7);
Check(settingGroups.SelectMany(group => group.Item2).Distinct().Count() == 39);
foreach (var (section, keys) in settingGroups)
{
    Check(SettingsText.Section(section, true) == section);
    Check(!SettingsText.Section(section, false).Any(c => c >= '\u0400' && c <= '\u04ff'));
    for (int i = 0; i < keys.Length; i++)
    {
        Check(SettingsLayout.SectionFor(keys[i]) == section);
        Near(SettingsLayout.Order(keys[i]), i);
        Check(SettingsText.HasEntry(keys[i]));
    }
}
Check(SettingsText.SectionHint(SettingsLayout.Hud, true).Contains("Slot 1 → Slot 2 → мини-карта → Slot 3 → Slot 4 → Slot 5 → Slot 6"));
Check(SettingsText.SectionHint(SettingsLayout.Navigation, false).Contains("no separate toggle"));
Check(SettingsText.SectionHint(SettingsLayout.General, true) == "");
// Upgrade, restart, partial migration and conflicting old/new values: explicit new values win.
foreach (bool saved in new[] { false, true })
{
    Check(SettingsLayout.MigratedValue(saved, true, false) == saved);
    Near(SettingsLayout.MigratedValue(saved, 12, 19), saved ? 12 : 19);
    Near(SettingsLayout.MigratedValue(saved, .25, .75), saved ? .25 : .75);
    Check(SettingsLayout.MigratedValue(saved, HudSlotContent.Wind, HudSlotContent.Coordinates) ==
        (saved ? HudSlotContent.Wind : HudSlotContent.Coordinates));
    Check(SettingsLayout.MigratedValue(saved, "Q + LeftControl", "KeypadPlus") ==
        (saved ? "Q + LeftControl" : "KeypadPlus"));
}
int migratedIconSize = SettingsLayout.MigratedValue(false, 12, 19);
Near(SettingsLayout.MigratedValue(true, migratedIconSize, 12), 19);
Check(SettingsText.Choice("MapLighting", "TimeOfDay", true) == "По времени суток");
Check(SettingsText.Choice("MapLighting", "Night", false) == "Always night");
Near(MapMath.Daylight(0), 0);
Near(MapMath.Daylight(.5), 1);
Near(MapMath.Daylight(1), 0);
Near(MapMath.Daylight(.25), .5);
Near(MapMath.Daylight(.75), .5);
for (int i = 0; i <= 1000; i++)
{
    double time = i / 1000d;
    double light = MapMath.Daylight(time);
    Check(light >= 0 && light <= 1);
    Near(light, MapMath.Daylight(time + 1));
    Near(light, MapMath.Daylight(1 - time));
}
Check(SettingsText.Choice("HudSlotContent", "Biome", true) == "Биом");
Check(SettingsText.Choice("HudSlotContent", "Coordinates", false) == "Coordinates");
Check(SettingsText.Choice("HudSlotContent", "Time", true) == "Игровое время");
Check(SettingsText.Choice("HudSlotContent", "Time", false) == "In-game time");
Check(SettingsText.Choice("HudSlotContent", "Weight", true) == "Вес");
Check(SettingsText.Choice("HudSlotContent", "Weight", false) == "Weight");
Check(SettingsText.Choice("HudSlotContent", "RealTime", true) == "Реальное время");
Check(SettingsText.Choice("HudSlotContent", "RealTime", false) == "Real time");
Check(SettingsText.SectionHint(SettingsLayout.Hud, true).Contains("HH:mm"));
Check(SettingsText.SectionHint(SettingsLayout.Hud, false).Contains("Coordinates / In-game time / Weight / Real time"));
Check(SettingsText.PanelText("Save", true) == "Сохранить");
Check(SettingsText.PanelText("Save", false) == "Save");
Check(SettingsText.Name("unknown", false) == "unknown");
Check(SettingsText.Name("Зум через + и -", false) == "Zoom with extra keys");
Check(SettingsText.Description("unknown", false, "fallback") == "fallback");
foreach (var key in new[] { "Вращение", "Показывать линии компаса", "Показывать направления",
    "Зум через + и -", "Плавный зум", "Сохранять отдельный зум для кораблей", "Указатель цели", "Показывать никнеймы", "Показывать транспорт" })
{
    Check(ResetPreset.Value(key, true, true).Equals(false));
    Check(ResetPreset.Value(key, true, false).Equals(true));
}
Check(SettingsText.Choice("HudSlotContent", "Empty", true) == "Пусто");
Check(SettingsText.Choice("HudSlotContent", "Wind", true) == "Ветер");
Check(SettingsText.Choice("HudSlotContent", "Empty", false) == "Empty");
Check(SettingsText.Choice("HudSlotContent", "Wind", false) == "Wind");
Check(ResetPreset.Value("Размер мини-карты", 130, true).Equals(100));
Check(ResetPreset.Value("Размер мини-карты", 130, false).Equals(130));
Check(ResetPreset.Value("Min zoom", .1f, true).Equals(0f));
Check(ResetPreset.Value("Max zoom", 4f, true).Equals(1f));
Check(ResetPreset.Value("Min zoom", .1f, false).Equals(.1f));
Check(ResetPreset.Value("Max zoom", 4f, false).Equals(4f));
Check(!ResetPreset.Includes("Мод включён"));
Check(ResetPreset.Includes("Min zoom"));
foreach (bool enabled in new[] { false, true })
foreach (bool vanilla in new[] { false, true })
{
    object value = enabled;
    if (ResetPreset.Includes("Мод включён")) value = ResetPreset.Value("Мод включён", true, vanilla);
    Check(value.Equals(enabled));
}
Check(ResetPreset.Value("Цвет линии", "#505050BB", false).Equals("#505050BB"));
Check(ResetPreset.Value("Толщина линии", .8f, false).Equals(.8f));
Near(MapMath.RoundSetting(.1, .1, 3), .1);
Near(MapMath.RoundSetting(.04, .1, 3), .1);
Check(ResetPreset.Value("Режим", TestRotation.CameraUp, true).Equals(TestRotation.NorthUp));
Check(ResetPreset.Value("Режим", TestRotation.CameraUp, false).Equals(TestRotation.CameraUp));
Check(ResetPreset.Value("Форма", TestShape.Circle, true).Equals(TestShape.Square));
Check(ResetPreset.Value("Форма", TestShape.Circle, false).Equals(TestShape.Circle));
Check(ResetPreset.Value("Slot 2", HudSlotContent.Biome, true).Equals(HudSlotContent.Biome));
Check(ResetPreset.Value("Slot 3", HudSlotContent.Wind, false).Equals(HudSlotContent.Wind));
Near(MapOpacity.DefaultTransparency, 0);
Near(MapOpacity.Alpha(0), 1);
Near(MapOpacity.Alpha(25), .75);
Near(MapOpacity.Alpha(50), .5);
Near(MapOpacity.Alpha(75), .25);
Near(MapOpacity.Alpha(100), 0);
Near(MapOpacity.Alpha(int.MinValue), 1);
Near(MapOpacity.Alpha(int.MaxValue), 0);
// Fault injection uses the same guard as the Unity view. A rendering failure must
// restore once, warn once and allow the caller (the rest of the mod) to continue.
int renderAttempts = 0, restores = 0, warnings = 0, continued = 0;
bool failRender = true, nativeVisible = true, overlayAllocated = false;
float capturedHeading = 0;
bool capturedVisible = false;
Exception warningError = null;
var fallback = new TransparencyFallback((percent, heading, visible) =>
{
    renderAttempts++;
    if (percent == 0) { nativeVisible = true; overlayAllocated = false; return; }
    capturedHeading = heading; capturedVisible = visible;
    overlayAllocated = true;
    nativeVisible = false;
    if (failRender) throw new InvalidOperationException("Injected RenderTexture/SetPass failure");
}, () => { restores++; nativeVisible = true; overlayAllocated = false; }, error =>
{
    warnings++;
    warningError = error;
});
for (int frame = 0; frame < 100; frame++) { fallback.Apply(50, 90, true); continued++; }
Check(renderAttempts == 1 && restores == 1 && warnings == 1 && continued == 100);
Near(capturedHeading, 90);
Check(capturedVisible && warningError is InvalidOperationException);
Check(nativeVisible && !overlayAllocated);
fallback.Apply(75, 90, true);
fallback.Apply(100, 90, true);
fallback.Apply(50, 90, false);
Check(renderAttempts == 1); // neither nonzero edits nor hidden/reopened maps retry
fallback.Apply(0, 0, false); // explicit reset, even while the minimap is hidden
Check(renderAttempts == 2 && nativeVisible);
fallback.Apply(50, 90, true);
Check(renderAttempts == 3 && warnings == 2 && restores == 2);
failRender = false;
fallback.Apply(0, 0, true);
fallback.Apply(50, 90, true);
Check(renderAttempts == 5 && restores == 2 && warnings == 2 && !nativeVisible && overlayAllocated);
// Cleanup/logging failures are contained too, including a failing zero-percent cleanup.
int brokenAttempts = 0, brokenWarnings = 0;
Exception brokenError = null;
var brokenFallback = new TransparencyFallback((percent, heading, visible) =>
{
    brokenAttempts++;
    throw new InvalidOperationException("Render failure");
}, () => throw new Exception("Cleanup failure"), error =>
{
    brokenWarnings++;
    brokenError = error;
    throw new Exception("Logger failure");
});
for (int frame = 0; frame < 100; frame++) brokenFallback.Apply(0, 0, true);
Check(brokenAttempts == 1 && brokenWarnings == 1);
Check(brokenError is AggregateException aggregate && aggregate.InnerExceptions.Count == 2);
for (int percent = 0; percent <= 100; percent++)
{
    Check(Math.Abs(MapOpacity.Alpha(percent) - (1 - percent / 100d)) < 1e-6);
    if (percent > 0) Check(MapOpacity.Alpha(percent) < MapOpacity.Alpha(percent - 1));
}
foreach (bool vanilla in new[] { false, true })
    Check(ResetPreset.Value("Прозрачность карты", MapOpacity.DefaultTransparency, vanilla).Equals(0));
Check(SettingsLayout.SectionFor("Прозрачность карты") == SettingsLayout.Map);
Near(SettingsLayout.MigratedValue(true, 63, MapOpacity.DefaultTransparency), 63);
// Both presets keep the original slot defaults; the clock is opt-in, not a new setting.
foreach (bool vanilla in new[] { false, true })
{
    var defaults = HudSlots.Defaults();
    for (int i = 0; i < defaults.Length; i++)
    {
        Check(ResetPreset.Includes("Slot " + (i + 1)));
        Check(ResetPreset.Value("Slot " + (i + 1), defaults[i], vanilla).Equals(defaults[i]));
    }
}
Check(SettingsText.ResetLabel(true, false) == "Reset to Vanilla");
Check(SettingsText.ResetLabel(false, false) == "Reset to mod default");
Check(SettingsText.ResetLabel(true, true) == "Сбросить к настройкам игры");
Check(SettingsText.ResetLabel(false, true) == "Сбросить к настройкам мода");
var shipZoom = new ShipZoomMemory();
Near(shipZoom.Update(false, false, .01), .01);
Near(shipZoom.Update(true, false, .01), .01);
Check(!shipZoom.ChangedContext && !shipZoom.ShipMode);
Near(shipZoom.Update(true, true, .01), .01);
Check(shipZoom.ChangedContext && shipZoom.ShipMode);
Near(shipZoom.Update(true, true, .025), .025);
Check(!shipZoom.ChangedContext);
Near(shipZoom.Update(true, false, .025), .01);
Check(shipZoom.ChangedContext && !shipZoom.ShipMode);
Near(shipZoom.Update(true, false, .005), .005);
Near(shipZoom.Update(true, true, .005), .025);
Near(shipZoom.Update(false, true, .025), .005);
Check(shipZoom.ChangedContext && !shipZoom.ShipMode);
Near(shipZoom.Update(false, false, .008), .008);
Near(shipZoom.Update(true, true, .008), .025);
Check(shipZoom.ShipMode && shipZoom.ChangedContext);
shipZoom.Remember(.02);
Near(shipZoom.Ship, .02);
shipZoom.Suspend();
Near(shipZoom.Update(true, false, 1), .005);
Near(shipZoom.Update(true, true, .005), .02);
shipZoom.ResetValues(1, 1);
shipZoom.Suspend();
Near(shipZoom.Update(true, true, .02), 1);

string memoryDir = Path.Combine(Path.GetTempPath(), "BetterMinimap-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(memoryDir);
var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
try
{
    string path = Path.Combine(memoryDir, "zoom.memory");
    Check(!ZoomMemoryStore.TryLoad(path, out _, out _));
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
    ZoomMemoryStore.Save(path, 1.5, .444444444444);
    Check(ZoomMemoryStore.TryLoad(path, out var land, out var ship));
    Near(land, 1.5); Near(ship, .444444444444);
    var nextSession = new ShipZoomMemory();
    nextSession.ResetValues(land, ship);
    Near(nextSession.Update(true, false, 1), 1.5);
    Near(nextSession.Update(true, true, 1.5), .444444444444);
    var startsOnShip = new ShipZoomMemory();
    startsOnShip.ResetValues(land, ship);
    Near(startsOnShip.Update(true, true, 1), .444444444444);
    ZoomMemoryStore.Save(path, 2.25, .5);
    Check(ZoomMemoryStore.TryLoad(path, out land, out ship));
    Near(land, 2.25); Near(ship, .5);
    Check(Directory.GetFiles(memoryDir).Length == 1);
    foreach (string bad in new[] { "broken", "BetterMinimap.ZoomMemory.v1\nNaN\n1", "BetterMinimap.ZoomMemory.v1\n0\n1" })
    {
        File.WriteAllText(path, bad);
        Check(!ZoomMemoryStore.TryLoad(path, out _, out _));
    }
}
finally
{
    System.Globalization.CultureInfo.CurrentCulture = previousCulture;
    Directory.Delete(memoryDir, true);
}
// Slot defaults/migration: never lose a biome or wind element, including old corner values.
Check(HudSlots.Migrate("Above", "Bottom").SequenceEqual(new[] {
    HudSlotContent.Empty, HudSlotContent.Biome, HudSlotContent.Wind, HudSlotContent.Empty, HudSlotContent.Empty, HudSlotContent.Empty }));
Check(HudSlots.Defaults().SequenceEqual(HudSlots.Migrate("Above", "Bottom")));
Check(!SettingsText.HasEntry("Показывать координаты"));
foreach (HudSlotContent content in Enum.GetValues<HudSlotContent>())
foreach (bool hadSwitch in new[] { false, true })
foreach (bool wasEnabled in new[] { false, true })
{
    var migrated = HudSlots.MigrateCoordinates(content, hadSwitch, wasEnabled);
    Check(migrated == (hadSwitch && !wasEnabled && content == HudSlotContent.Coordinates ? HudSlotContent.Empty : content));
    // Once the switch is removed, explicit coordinate choices must persist over subsequent starts.
    Check(HudSlots.MigrateCoordinates(migrated, false, false) == migrated);
}
foreach (string biome in new[] { "Vanilla", "Above", "Below" })
foreach (string wind in new[] { "Vanilla", "TopLeft", "TopRight", "BottomLeft", "BottomRight", "Top", "Bottom" })
{
    var slots = HudSlots.Migrate(biome, wind);
    Check(slots.Length == HudSlots.Count && slots.Length == 6);
    Check(slots[4] == HudSlotContent.Empty && slots[5] == HudSlotContent.Empty);
    Check(slots.Count(x => x == HudSlotContent.Biome) == 1);
    Check(slots.Count(x => x == HudSlotContent.Wind) == 1);
    Check(slots.Count(x => x == HudSlotContent.Coordinates) == 0);
    Check(slots[biome == "Below" ? 2 : 1] == HudSlotContent.Biome);
    int windIndex = Array.IndexOf(slots, HudSlotContent.Wind);
    Check(wind.StartsWith("Top") ? windIndex < 2 : windIndex >= 2);
}
// Wind reserves its full diagonal; outer slots must not overlap the inner rows.
foreach (double inner in new[] { 10d, 19.2d, 22d, 36d, 56d, 100d })
foreach (double outer in new[] { 10d, 19.2d, 22d, 36d, 56d, 100d })
{
    var heights = new[] { outer, inner, inner, outer, 0d, 0d };
    double upperInner = HudSlots.CenterOffset(1, heights);
    double upperOuter = HudSlots.CenterOffset(0, heights);
    Near(upperInner - inner / 2, HudSlots.Gap);
    Near(upperOuter - outer / 2 - (upperInner + inner / 2), HudSlots.Gap);
    Near(HudSlots.CenterOffset(2, heights), -upperInner);
    Near(HudSlots.CenterOffset(3, heights), -upperOuter);
}
Check(SettingsText.Section(SettingsLayout.Navigation, false) == "5. Compass and navigation");
Near(HudSlots.Gap, 2);
Near(HudSlots.FallbackFontSize, 16);
Near(HudSlots.DefaultTextPercent, 100);
Near(HudSlots.MinTextPercent, 25);
Near(HudSlots.MaxTextPercent, 400);
for (int slot = 0; slot < HudSlots.Count; slot++)
{
    string key = HudSlots.TextSizeKey(slot);
    Check(key == "Размер текста Slot " + (slot + 1));
    Check(SettingsText.HasEntry(key));
    Check(SettingsText.Name(key, true) == "Размер текста слот " + (slot + 1));
    Check(SettingsText.Name(key, false) == "Slot " + (slot + 1) + " text size");
    Check(SettingsText.Description(key, true, "MISSING") == "");
    Check(SettingsText.Description(key, false, "MISSING") == "");
    Check(HudSlots.TryGetTextSizeSlot(key, out int foundSlot) && foundSlot == slot);
    Check(SettingsLayout.SectionFor(key) == SettingsLayout.Hud);
    Near(SettingsLayout.Order(key), SettingsLayout.Order("Slot " + (slot + 1)) + 1);
    Check(ResetPreset.Includes(key));
    foreach (bool vanilla in new[] { false, true })
        Check(ResetPreset.Value(key, HudSlots.DefaultTextPercent, vanilla).Equals(100));
    // A saved size is independent of content and wins on reload; new keys start at 100.
    Near(SettingsLayout.MigratedValue(true, 75 + slot * 20, HudSlots.DefaultTextPercent), 75 + slot * 20);
}
Check(!HudSlots.TryGetTextSizeSlot("Размер текста Slot 7", out _));
Check(!HudSlots.TryGetTextSizeSlot("Slot 1", out _));
Check(!HudSlots.TryGetTextSizeSlot("Размер шрифта никнеймов", out _));
foreach (int percent in new[] { 25, 50, 75, 100, 150, 200, 400 })
{
    foreach (var content in new[] { HudSlotContent.Coordinates, HudSlotContent.Time, HudSlotContent.RealTime, HudSlotContent.Weight, HudSlotContent.Biome })
        Near(HudSlots.FontSize(content, percent, 30), 30d * percent / 100);
    Near(HudSlots.Scale(percent), percent / 100d); // wind uses the same factor from its native base scale
    Near(HudSlots.FontSize(HudSlotContent.Wind, percent, 30), 0);
    Near(HudSlots.FontSize(HudSlotContent.Empty, percent, 30), 0);
}
Near(HudSlots.FontSize(HudSlotContent.Coordinates, int.MinValue, 30), 7.5);
Near(HudSlots.FontSize(HudSlotContent.Time, int.MaxValue, 30), 120);
Near(HudSlots.Scale(int.MinValue), .25);
Near(HudSlots.Scale(int.MaxValue), 4);
Check(!SlotRowLayout.SingleLine(0) && !SlotRowLayout.SingleLine(719));
Check(SlotRowLayout.SingleLine(720) && SlotRowLayout.SingleLine(1100));
Near(SlotRowLayout.Height(719), 88);
Near(SlotRowLayout.Height(720), 46);
Near(HudSlots.TextHeight(4.5, 6), 10);
Near(HudSlots.TextHeight(9, 9), 10.8);
Near(HudSlots.TextHeight(12, 12), 14.4);
Near(HudSlots.TextHeight(24, 24), 28.8);
Near(HudSlots.TextHeight(24, 31.2), 32); // actual font metrics can exceed nominal size
// Different sizes in repeated-content slots remain independent and their rows do not overlap.
foreach (var content in new[] { HudSlotContent.Coordinates, HudSlotContent.Weight, HudSlotContent.Time, HudSlotContent.RealTime, HudSlotContent.Biome })
{
    int[] percents = { 25, 75, 100, 150, 200, 400 };
    var heights = new double[HudSlots.Count];
    for (int slot = 0; slot < HudSlots.Count; slot++)
    {
        float font = HudSlots.FontSize(content, percents[slot], 30);
        heights[slot] = HudSlots.TextHeight(font, font * 1.3);
    }
    double lowerEdge = 0;
    for (int slot = HudSlots.AboveCount; slot < HudSlots.Count; slot++)
    {
        double center = Math.Abs(HudSlots.CenterOffset(slot, heights));
        Near(center - heights[slot] / 2 - lowerEdge, HudSlots.Gap);
        lowerEdge = center + heights[slot] / 2;
    }
}
// Append enum values so legacy numeric slot selections remain valid.
Check((int)HudSlotContent.Empty == 0 && (int)HudSlotContent.Biome == 1 &&
    (int)HudSlotContent.Wind == 2 && (int)HudSlotContent.Coordinates == 3 && (int)HudSlotContent.Time == 4);
Check((int)HudSlotContent.Weight == 5 && (int)HudSlotContent.RealTime == 6);
var realClock = new RealClock();
var localNow = new DateTime(2026, 10, 12, 14, 35, 0, DateTimeKind.Local);
Check(realClock.Update(localNow) == "14:35");
Check(ReferenceEquals(realClock.Update(localNow), realClock.Update(localNow.AddSeconds(59))));
Check(realClock.Update(localNow.AddMinutes(1)) == "14:36");
Check(realClock.Update(localNow.AddHours(-1)) == "13:35"); // system time/zone correction
Check(realClock.Update(new DateTime(2026, 10, 12, 23, 59, 59)) == "23:59");
Check(realClock.Update(new DateTime(2026, 10, 13, 0, 0, 0)) == "00:00");
var weight = new WeightReadout();
Check(weight.Update(0, 300) == "0 / 300");
Check(weight.Update(123.1, 300) == "124 / 300");
Check(ReferenceEquals(weight.Update(123.1, 300), weight.Update(123.9, 300)));
Check(weight.Update(300.01, 300) == "301 / 300"); // don't hide overload
Check(weight.Update(301, 450) == "301 / 450"); // equipment/status effect changes
Check(weight.Update(301, 450.2) == "301 / 451");
Check(weight.Update(0, 0) == "0 / 0");
Check(weight.Update(99999.5, 100000) == "100000 / 100000");
Check(weight.Update(double.NaN, 300) == "-- / 300");
Check(weight.Update(10, double.PositiveInfinity) == "10 / --");
Check(weight.Update(-1, double.NegativeInfinity) == "-- / --");
Check(weight.Update(15, 300) == "15 / 300"); // recover after invalid data/world change
var clock = new GameClock();
Check(clock.Update(double.NaN) == "--:--");
Check(clock.Update(0) == "00:00");
Check(clock.Update(.25) == "06:00");
Check(clock.Update(.5) == "12:00");
Check(clock.Update(.75) == "18:00");
Check(clock.Update((14 * 60 + 35.5) / 1440) == "14:35");
Check(clock.Update(1 - 1e-9) == "23:59");
Check(clock.Update(1) == "00:00");
Check(clock.Update(2.25) == "06:00");
Check(clock.Update(-.25) == "18:00");
Check(clock.Update(double.PositiveInfinity) == "--:--");
Check(clock.Update(double.NegativeInfinity) == "--:--");
// Sleep/time changes and a new world must not retain the previous minute.
Check(clock.Update(.95) == "22:48");
Check(clock.Update(.25) == "06:00");
Check(clock.Update(.5) == "12:00");
Check(ReferenceEquals(clock.Update(.5), clock.Update(.5 + .1 / 1440)));
var clockCulture = System.Globalization.CultureInfo.CurrentCulture;
try
{
    System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fi-FI");
    for (int minute = 0; minute < 1440; minute++)
    {
        Check(clock.Update((minute + .5) / 1440) == $"{minute / 60:00}:{minute % 60:00}");
        Check(realClock.Update(localNow.Date.AddMinutes(minute)) == $"{minute / 60:00}:{minute % 60:00}");
    }
    Check(weight.Update(150.3, 450.1) == "151 / 451");
}
finally { System.Globalization.CultureInfo.CurrentCulture = clockCulture; }
Check(SettingsText.Section(SettingsLayout.Players, false) == "6. Players");
Check(ResetPreset.Value("Размер шрифта никнеймов", PlayerNameLayout.DefaultFontSize, false).Equals(8));
Near(PlayerNameLayout.DefaultFontSize, 8);
Near(PlayerNameLayout.MinFontSize, 4);
Near(PlayerNameLayout.MaxFontSize, 20);
Near(PlayerNameLayout.Top(0, 32), -10.6);
foreach (double iconHeight in new[] { 16d, 24d, 32d, 48d })
foreach (double y in new[] { -80d, 0d, 60d })
{
    double nameTop = PlayerNameLayout.Top(y, iconHeight);
    Check(nameTop < y);
    Check(nameTop > y - iconHeight / 2 - 2); // closer than the old full-rectangle bottom + gap
    Near(y - nameTop, iconHeight * .3 + 1);
}
Check(ResetPreset.Value("Обводка никнеймов", true, false).Equals(true));
Near(HudSlots.CenterOffset(0, new[] { 10d, 0, 0, 0, 0, 0 }), HudSlots.CenterOffset(1, new[] { 0d, 10, 0, 0, 0, 0 }));
Near(HudSlots.CenterOffset(3, new[] { 0d, 0, 0, 10, 0, 0 }), HudSlots.CenterOffset(2, new[] { 0d, 0, 10, 0, 0, 0 }));
Near(HudSlots.CenterOffset(2, new[] { 0d, 0, 22, 10, 0, 0 }), -13);
Near(HudSlots.CenterOffset(3, new[] { 0d, 0, 22, 10, 0, 0 }), -31);
// Every empty/filled combination: lower rows accumulate ALL preceding filled rows.
for (int mask = 0; mask < (1 << HudSlots.Count); mask++)
{
    var heights = new double[HudSlots.Count];
    for (int i = 0; i < heights.Length; i++) heights[i] = (mask & (1 << i)) != 0 ? 10 + i * 7.2 : 0;
    foreach (var order in new[] { new[] { 1, 0 }, new[] { 2, 3, 4, 5 } })
    {
        double edge = 0;
        foreach (int slot in order)
        {
            double center = HudSlots.CenterOffset(slot, heights);
            if (heights[slot] == 0) { Near(center, 0); continue; }
            Check(slot < HudSlots.AboveCount ? center > 0 : center < 0);
            Near(Math.Abs(center) - heights[slot] / 2 - edge, HudSlots.Gap);
            edge = Math.Abs(center) + heights[slot] / 2;
        }
    }
}
foreach (HudSlotContent saved in Enum.GetValues<HudSlotContent>())
{
    Check(SettingsLayout.MigratedValue(true, saved, HudSlotContent.Empty) == saved);
    Check(SettingsLayout.MigratedValue(false, saved, HudSlotContent.Empty) == HudSlotContent.Empty);
}
Check(ResetPreset.Value("Показывать мини-карту", true, true).Equals(true));
Check(ResetPreset.Value("Показывать мини-карту", true, false).Equals(true));
Check(SettingsText.TargetSelected(350.4, true) == "Цель выбрана (350 м)");
Check(SettingsText.TargetSelected(350.5, true) == "Цель выбрана (351 м)");
Check(SettingsText.TargetSelected(0, true) == "Цель выбрана (0 м)");
Check(SettingsText.TargetSelected(12345.9, false) == "Target selected (12346 m)");
Near(HudSlots.ViewportShift(480, 200, -500, 500), 0);
Near(HudSlots.ViewportShift(550, 200, -500, 500), -50);
Near(HudSlots.ViewportShift(-200, -550, -500, 500), 50);
Near(HudSlots.ViewportShift(600, -600, -500, 500), -100);
foreach (HudSlotContent content in Enum.GetValues<HudSlotContent>())
{
    Check(SettingsText.Choice(nameof(HudSlotContent), content.ToString(), true) != content.ToString());
    Check(!SettingsText.Choice(nameof(HudSlotContent), content.ToString(), false).Any(c => c >= '\u0400' && c <= '\u04ff'));
}

// Target projection remains on the same ray through rotation and clamps to both shapes.
foreach (bool round in new[] { false, true })
foreach (double width in new[] { 160d, 240d, 300d })
foreach (double height in new[] { 160d, 240d })
foreach (double heading in new[] { 0d, 45d, 90d, 135d, 180d, 270d, 359d })
foreach (double x in new[] { -10000d, -70d, 0d, 30d, 10000d })
foreach (double y in new[] { -10000d, -70d, 0d, 30d, 10000d })
{
    var rotated = MapMath.Rotate(x, y, heading);
    var point = TargetMath.Clamp(rotated.X, rotated.Y, width, height, 14, round);
    Check(double.IsFinite(point.X) && double.IsFinite(point.Y));
    Check(Math.Abs(point.X) <= width / 2 - 14 + 1e-8);
    Check(Math.Abs(point.Y) <= height / 2 - 14 + 1e-8);
    if (round) Check(Math.Sqrt(point.X * point.X + point.Y * point.Y) <= Math.Min(width, height) / 2 - 14 + 1e-8);
    // Relative tolerance for the cross product of long rays.
    Check(Math.Abs(point.X * rotated.Y - point.Y * rotated.X) < 1e-6);
    if (!point.Outside) { Near(point.X, rotated.X); Near(point.Y, rotated.Y); }
}
var centerTarget = TargetMath.Clamp(0, 0, 200, 200, 14, true);
Check(!centerTarget.Outside); Near(centerTarget.X, 0); Near(centerTarget.Y, 0);
Near(TargetMath.Distance(0, 0, 3, 4), 5);
Near(TargetMath.Distance(-3, -4, 0, 0), 5);
Near(TargetMath.Distance(123, -456, 123, -456), 0);
// A corner is visible on the square but requires an edge arrow on the circle.
Check(!TargetMath.Clamp(70, 70, 200, 200, 14, false).Outside);
Check(TargetMath.Clamp(70, 70, 200, 200, 14, true).Outside);
var tiny = TargetMath.Clamp(100, -100, 20, 20, 46, false);
Near(tiny.X, 0); Near(tiny.Y, 0); Check(tiny.Outside);
// Nearby pings keep their native icon; no duplicate edge icon, even near the inner inset.
foreach (bool round in new[] { false, true })
{
    Check(!TargetMath.EdgePing(0, 0, 200, 200, 17, round).Visible);
    Check(!TargetMath.EdgePing(95, 0, 200, 200, 17, round).Visible);
    foreach (double heading in new[] { 0d, 45d, 90d, 135d, 180d, 270d, 359d })
    {
        var far = MapMath.Rotate(1000, 0, heading);
        var edge = TargetMath.EdgePing(far.X, far.Y, 200, 200, 17, round);
        Check(edge.Visible);
        Check(Math.Abs(edge.X * far.Y - edge.Y * far.X) < 1e-6);
        Check(Math.Abs(edge.X) <= 83 + 1e-8 && Math.Abs(edge.Y) <= 83 + 1e-8);
        if (round) Near(Math.Sqrt(edge.X * edge.X + edge.Y * edge.Y), 83);
        // Moving back into view must hand rendering back to vanilla.
        Check(!TargetMath.EdgePing(far.X / 20, far.Y / 20, 200, 200, 17, round).Visible);
    }
}
Check(TargetMath.EdgePing(80, 80, 200, 200, 17, true).Visible);
Check(!TargetMath.EdgePing(80, 80, 200, 200, 17, false).Visible);
// A ring whose center fits but whose radius would cross the border becomes an edge arrow too.
foreach (bool round in new[] { false, true })
{
    Check(!TargetMath.EdgePing(60, 0, 200, 200, 14, round, 20).Visible);
    var clippedRing = TargetMath.EdgePing(85, 0, 200, 200, 14, round, 20);
    Check(clippedRing.Visible); Near(clippedRing.X, 86); Near(clippedRing.Y, 0);
    var largeRing = TargetMath.EdgePing(70, 0, 200, 200, 14, round, 32);
    Check(largeRing.Visible); Near(largeRing.X, 86);
}
Near(TargetMath.ArrowAngle(0, 1), 0);
Near(TargetMath.ArrowAngle(1, 0), -90);
Near(TargetMath.ArrowAngle(-1, 0), 90);
Near(TargetMath.ArrowAngle(0, -1), -180);
foreach (double heading in new[] { 0d, 45d, 90d, 180d, 270d, 359d })
{
    var dir = MapMath.Rotate(1, 0, heading);
    double angle = (TargetMath.ArrowAngle(dir.X, dir.Y) + 90) * Math.PI / 180;
    Near(Math.Cos(angle), dir.X); Near(Math.Sin(angle), dir.Y);
}
// Vehicle icons are real positions, not edge-clamped or saved pins.
Near(TransportLayout.DefaultIconSize, 12);
Near(TransportLayout.MinIconSize, 5);
Near(TransportLayout.MaxIconSize, 20);
Near(TransportLayout.ClampSize(0), 5);
Near(TransportLayout.ClampSize(99), 20);
Check(SettingsText.HasEntry("Размер иконок транспорта"));
Check(SettingsText.Name("Размер иконок транспорта", false) == "Transport icon size");
Check(SettingsText.Description("Размер иконок транспорта", true, "").Contains("5–20"));
foreach (bool vanilla in new[] { false, true })
    Check(ResetPreset.Value("Размер иконок транспорта", TransportLayout.DefaultIconSize, vanilla).Equals(12));
for (int size = 5; size <= 20; size++)
{
    Check(Math.Abs(TransportLayout.Scale(size) * 18 - size) < .00001);
    double margin = TransportLayout.EdgeMargin(size);
    Check(margin >= size * Math.Sqrt(2) / 2);
    foreach (bool round in new[] { false, true })
    {
        Check(TransportLayout.Visible(100 - margin - .01, 0, 200, 200, round, size));
        Check(!TransportLayout.Visible(100 - margin + .01, 0, 200, 200, round, size));
    }
}
Check(ResetPreset.Value("Показывать транспорт", true, false).Equals(true));
Check(SettingsText.Name("Показывать транспорт", false) == "Show transport");
foreach (bool round in new[] { false, true })
{
    Check(TransportLayout.Visible(0, 0, 200, 200, round));
    Check(!TransportLayout.Visible(0, 0, 26, 200, round));
    Check(!TransportLayout.Visible(0, 0, 200, 0, round));
    Check(!TransportLayout.Visible(0, 0, -100, 200, round));
    Check(TransportLayout.Visible(86, 0, 200, 200, round));
    Check(!TransportLayout.Visible(88, 0, 200, 200, round));
    Check(!TransportLayout.Visible(0, -88, 200, 200, round));
    Check(!TransportLayout.Visible(1000, 1000, 200, 200, round));
    foreach (double heading in new[] { 0d, 45d, 90d, 180d, 270d })
    {
        var inside = MapMath.Rotate(30, 40, heading);
        Check(TransportLayout.Visible(inside.X, inside.Y, 200, 200, round));
        var outside = MapMath.Rotate(200, 200, heading);
        Check(!TransportLayout.Visible(outside.X, outside.Y, 200, 200, round));
    }
}
Check(TransportLayout.Visible(80, 80, 200, 200, false));
Check(!TransportLayout.Visible(80, 80, 200, 200, true));
// The entire stencil edge lies within the single brown band, including polygon midpoints.
Check(MapFrameGeometry.OuterInset < MapFrameGeometry.MaskInset);
Check(MapFrameGeometry.MaskInset < MapFrameGeometry.InnerInset);
foreach (double size in new[] { 64d, 128d, 256d, 384d, 512d })
{
    var first = MapFrameGeometry.CirclePoint(size, size, 0, MapFrameGeometry.MaskInset);
    var last = MapFrameGeometry.CirclePoint(size, size, MapFrameGeometry.Segments, MapFrameGeometry.MaskInset);
    Near(first.X, last.X); Near(first.Y, last.Y);
    for (int i = 0; i < MapFrameGeometry.Segments; i++)
    foreach (double t in new[] { 0d, .5d, 1d })
    {
        double RadiusSquared(double inset)
        {
            var a = MapFrameGeometry.CirclePoint(size, size, i, inset);
            var b = MapFrameGeometry.CirclePoint(size, size, i + 1, inset);
            double x = a.X + (b.X - a.X) * t, y = a.Y + (b.Y - a.Y) * t;
            return x * x + y * y;
        }
        Check(RadiusSquared(MapFrameGeometry.InnerInset) < RadiusSquared(MapFrameGeometry.MaskInset));
        Check(RadiusSquared(MapFrameGeometry.MaskInset) < RadiusSquared(MapFrameGeometry.OuterInset));
    }
}
Console.WriteLine($"OK: {checks} geometry/zoom/UI math checks");

enum TestRotation { NorthUp, CameraUp }
enum TestShape { Square, Circle }
