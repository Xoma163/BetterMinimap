using System;
using System.Globalization;

namespace Shtab.RotatingMinimap;

// Formats the game's solar day fraction, not wall-clock time or session duration.
internal sealed class GameClock
{
    private int _minute = -1;
    private string _text = "--:--";

    public string Update(double dayFraction)
    {
        int minute = -1;
        if (!double.IsNaN(dayFraction) && !double.IsInfinity(dayFraction))
        {
            double wrapped = dayFraction - Math.Floor(dayFraction);
            minute = Math.Min(1439, (int)Math.Floor(wrapped * 1440));
        }
        if (minute != _minute)
        {
            _minute = minute;
            _text = minute < 0 ? "--:--" : string.Format(CultureInfo.InvariantCulture,
                "{0:00}:{1:00}", minute / 60, minute % 60);
        }
        return _text;
    }
}
