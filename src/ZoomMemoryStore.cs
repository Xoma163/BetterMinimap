using System;
using System.Globalization;
using System.IO;

namespace Shtab.RotatingMinimap;

// Local profile data, not game/world saves or hidden configuration switches.
internal static class ZoomMemoryStore
{
    public static bool TryLoad(string path, out double land, out double ship)
    {
        land = ship = 0;
        if (!File.Exists(path)) return false;
        string[] lines = File.ReadAllLines(path);
        return lines.Length == 3 && lines[0] == "BetterMinimap.ZoomMemory.v1" &&
            double.TryParse(lines[1], NumberStyles.Float, CultureInfo.InvariantCulture, out land) &&
            double.TryParse(lines[2], NumberStyles.Float, CultureInfo.InvariantCulture, out ship) &&
            ShipZoomMemory.Valid(land) && ShipZoomMemory.Valid(ship);
    }

    public static void Save(string path, double land, double ship)
    {
        if (!ShipZoomMemory.Valid(land) || !ShipZoomMemory.Valid(ship)) throw new ArgumentOutOfRangeException(nameof(land));
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllLines(temp, new[] { "BetterMinimap.ZoomMemory.v1",
                land.ToString("R", CultureInfo.InvariantCulture), ship.ToString("R", CultureInfo.InvariantCulture) });
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
