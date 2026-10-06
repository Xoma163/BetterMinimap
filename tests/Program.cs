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
foreach (string key in new[] { "Мод включён", "Вращение", "Режим", "Форма", "Размер мини-карты", "Положение биома",
    "Положение ветра", "Min zoom", "Max zoom", "Шаг зума", "Сохранять отдельный зум для кораблей",
    "Плавный зум", "Зум через + и -", "Приблизить", "Отдалить", "Показывать линии компаса",
    "Показывать направления", "Тип линии", "Цвет линии", "Толщина линии", "Освещение карты" })
{
    Check(SettingsText.HasEntry(key));
    Check(SettingsText.Name(key, true) == key);
    Check(!SettingsText.Name(key, false).Any(c => c >= '\u0400' && c <= '\u04ff'));
    Check(SettingsText.Description(key, true, "MISSING") != "MISSING");
    Check(!SettingsText.Description(key, false, "MISSING").Any(c => c >= '\u0400' && c <= '\u04ff'));
}
Check(SettingsText.Section("1. Общее", false) == "1. General");
Check(SettingsText.Section("2. Карта", false) == "2. Map");
Check(SettingsText.Section("3. Зум", false) == "3. Zoom");
Check(SettingsText.Section("4. Клавиши", false) == "4. Shortcuts");
Check(SettingsText.Section("5. Компас", false) == "5. Compass");
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
Check(SettingsText.Choice("BiomePosition", "Above", true) == "Сверху");
Check(SettingsText.Choice("WindPosition", "BottomLeft", false) == "Bottom left");
Check(SettingsText.PanelText("Save", true) == "Сохранить");
Check(SettingsText.PanelText("Save", false) == "Save");
Check(SettingsText.Name("unknown", false) == "unknown");
Check(SettingsText.Description("unknown", false, "fallback") == "fallback");
foreach (var key in new[] { "Вращение", "Показывать линии компаса", "Показывать направления",
    "Зум через + и -", "Плавный зум", "Сохранять отдельный зум для кораблей" })
{
    Check(ResetPreset.Value(key, true, true).Equals(false));
    Check(ResetPreset.Value(key, true, false).Equals(true));
}
Check(SettingsText.Choice("WindPosition", "Top", true) == "Сверху");
Check(SettingsText.Choice("WindPosition", "Bottom", true) == "Снизу");
Check(SettingsText.Choice("WindPosition", "Top", false) == "Top");
Check(SettingsText.Choice("WindPosition", "Bottom", false) == "Bottom");
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
Check(ResetPreset.Value("Положение биома", TestPosition.Above, true).Equals(TestPosition.Vanilla));
Check(ResetPreset.Value("Положение биома", TestPosition.Above, false).Equals(TestPosition.Above));
Check(ResetPreset.Value("Положение ветра", TestPosition.Bottom, true).Equals(TestPosition.Vanilla));
Check(ResetPreset.Value("Положение ветра", TestPosition.Bottom, false).Equals(TestPosition.Bottom));
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
Console.WriteLine($"OK: {checks} geometry/zoom/UI math checks");

enum TestRotation { NorthUp, CameraUp }
enum TestShape { Square, Circle }
enum TestPosition { Vanilla, Above, Bottom }
