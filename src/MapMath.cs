using System;

namespace Shtab.RotatingMinimap;

internal static class MapMath
{
    public static double Daylight(double fraction)
    {
        if (double.IsNaN(fraction) || double.IsInfinity(fraction)) return 1;
        fraction -= Math.Floor(fraction);
        // Midnight 0, noon .5; smooth dawn/dusk around .25 and .75.
        double height = -Math.Cos(fraction * 2 * Math.PI);
        double t = Math.Max(0, Math.Min(1, (height + .25) / .5));
        return t * t * (3 - 2 * t);
    }
    // Positive UI rotation is counter-clockwise; positive Unity yaw turns east.
    public static (double X, double Y) Rotate(double x, double y, double degrees)
    {
        double angle = degrees * Math.PI / 180d;
        double c = Math.Cos(angle), s = Math.Sin(angle);
        return (x * c - y * s, x * s + y * c);
    }

    public static (double X, double Y) MapToScreen(double u, double v,
        double centerU, double centerV, double spanU, double spanV,
        double width, double height, double heading)
    {
        return Rotate((u - centerU) / spanU * width, (v - centerV) / spanV * height, heading);
    }

    public static (double U, double V) ScreenToMap(double x, double y,
        double centerU, double centerV, double spanU, double spanV,
        double width, double height, double heading)
    {
        var p = Rotate(x, y, -heading);
        return (centerU + p.X / width * spanU, centerV + p.Y / height * spanV);
    }

    public static bool Inside(double x, double y, double width, double height) =>
        Math.Abs(x) < width / 2d && Math.Abs(y) < height / 2d;

    public static (double X, double Y) NorthEdge(double width, double height, double heading, double inset)
    {
        var north = Rotate(0, 1, heading);
        double halfWidth = Math.Max(0, width / 2 - inset);
        double halfHeight = Math.Max(0, height / 2 - inset);
        double xScale = Math.Abs(north.X) < 1e-10 ? double.PositiveInfinity : halfWidth / Math.Abs(north.X);
        double yScale = Math.Abs(north.Y) < 1e-10 ? double.PositiveInfinity : halfHeight / Math.Abs(north.Y);
        double scale = Math.Min(xScale, yScale);
        return (north.X * scale, north.Y * scale);
    }

    public static double ZoomStep(double current, bool zoomIn, bool zoomOut, double factor) =>
        zoomIn == zoomOut ? current : zoomIn ? current / factor : current * factor;

    public static double AnchoredZoomStep(double current, double baseline, bool zoomIn, bool zoomOut, double factor)
    {
        if (zoomIn == zoomOut || current <= 0 || baseline <= 0 || factor <= 1) return current;
        // Levels are factor^n, anchored at x1, not at the last clamped endpoint.
        double level = Math.Log(baseline / current) / Math.Log(factor);
        const double tolerance = 1e-5; // float-valued game/config input
        double next = zoomIn ? Math.Floor(level + tolerance) + 1 : Math.Ceiling(level - tolerance) - 1;
        return baseline / Math.Pow(factor, next);
    }

    public static double ClampZoom(double value, double baseline, double min, double max)
    {
        // uv span 1 is the complete world texture; zero means no custom far limit.
        if (double.IsNaN(value) || double.IsInfinity(value)) value = 1;
        if (min > max) (min, max) = (max, min);
        double near = baseline / Math.Max(.1, max);
        double far = min <= 0 ? 1 : baseline / min;
        far = Math.Min(1, Math.Max(near, far));
        return Math.Max(near, Math.Min(far, value));
    }

    public static double RoundSetting(double value, double min, double max) =>
        double.IsNaN(value) || double.IsInfinity(value) ? min :
        Math.Max(min, Math.Min(max, Math.Round(value, 1, MidpointRounding.AwayFromZero)));

    public static (double X, double Y) CompassEdge(double width, double height,
        double heading, double inset, bool round)
    {
        if (!round) return NorthEdge(width, height, heading, inset);
        var direction = Rotate(0, 1, heading);
        double radius = Math.Max(0, Math.Min(width, height) / 2 - inset);
        return (direction.X * radius, direction.Y * radius);
    }

    public static bool InsideCircle(double x, double y, double width, double height) =>
        x * x + y * y < Math.Pow(Math.Min(width, height) / 2, 2);

    public static double ScaleFactor(int percent) => Math.Max(50, Math.Min(150, percent)) / 100d;

    public static double BuffShift(double width, double originalScale, double pivot, int percent) =>
        Math.Max(0, width * originalScale * pivot * (ScaleFactor(percent) - 1));

    public static System.Collections.Generic.IEnumerable<(double Start, double End)> DashRanges(double length)
    {
        // Symmetrical dashes, with an empty center under the player marker.
        if (double.IsNaN(length) || double.IsInfinity(length) || length <= 0) yield break;
        double half = length / 2;
        for (double start = 5; start < half; start += 9)
        {
            double end = Math.Min(start + 4, half);
            yield return (start, end);
            yield return (-end, -start);
        }
    }
}
