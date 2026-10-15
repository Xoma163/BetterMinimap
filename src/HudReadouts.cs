using System;
using System.Globalization;

namespace Shtab.RotatingMinimap;

internal sealed class RealClock
{
    private int _minute = -1;
    private string _text = "--:--";

    // The caller samples DateTime.Now once for all visible slots. No game-time dependency.
    public string Update(DateTime localNow)
    {
        int minute = localNow.Hour * 60 + localNow.Minute;
        if (minute != _minute)
        {
            _minute = minute;
            _text = localNow.ToString("HH:mm", CultureInfo.InvariantCulture);
        }
        return _text;
    }
}

internal sealed class WeightReadout
{
    private double _current = double.NaN, _maximum = double.NaN;
    private string _text = "-- / --";

    public string Update(double current, double maximum)
    {
        // Match InventoryGui: ceil each value independently, without clamping overweight loads.
        current = DisplayWeight(current);
        maximum = DisplayWeight(maximum);
        if (!current.Equals(_current) || !maximum.Equals(_maximum))
        {
            _current = current; _maximum = maximum;
            _text = Format(current) + " / " + Format(maximum);
        }
        return _text;
    }

    private static double DisplayWeight(double value) =>
        double.IsNaN(value) || double.IsInfinity(value) || value < 0 ? double.NaN : Math.Ceiling(value);
    private static string Format(double value) => double.IsNaN(value) ? "--" : value.ToString("0", CultureInfo.InvariantCulture);
}
