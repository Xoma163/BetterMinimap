using System;

namespace Shtab.RotatingMinimap;

internal static class TargetMath
{
    // Clamp along the ray from the map center, preserving the target's direction.
    public static (double X, double Y, bool Outside) Clamp(double x, double y,
        double width, double height, double inset, bool round)
    {
        double halfX = Math.Max(0, width / 2 - inset), halfY = Math.Max(0, height / 2 - inset);
        double length = Math.Sqrt(x * x + y * y);
        if (length < 1e-9) return (0, 0, false);
        double scale;
        if (round) scale = Math.Min(halfX, halfY) / length;
        else scale = Math.Min(Math.Abs(x) < 1e-9 ? double.PositiveInfinity : halfX / Math.Abs(x),
            Math.Abs(y) < 1e-9 ? double.PositiveInfinity : halfY / Math.Abs(y));
        return scale < 1 ? (x * scale, y * scale, true) : (x, y, false);
    }

    public static double Distance(double x, double z, double targetX, double targetZ) =>
        Math.Sqrt((targetX - x) * (targetX - x) + (targetZ - z) * (targetZ - z));

    public static double ArrowAngle(double x, double y) => Math.Atan2(y, x) * 180 / Math.PI - 90;

    public static (double X, double Y, bool Visible) EdgePing(double x, double y,
        double width, double height, double inset, bool round, double nativeRadius = 0)
    {
        // Switch to an arrow before the native ring gets clipped, not just when its center leaves the map.
        if (!Clamp(x, y, width, height, Math.Max(0, nativeRadius), round).Outside) return (0, 0, false);
        double length = Math.Sqrt(x * x + y * y);
        double beyondEdge = (Math.Max(width, height) + inset + 1) / length;
        var edge = Clamp(x * beyondEdge, y * beyondEdge, width, height, inset, round);
        return (edge.X, edge.Y, true);
    }
}
