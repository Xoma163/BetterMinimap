using System;

namespace Shtab.RotatingMinimap;

internal static class TransportLayout
{
    public const int DefaultIconSize = 12;
    public const int MinIconSize = 5;
    public const int MaxIconSize = 20;
    public static int ClampSize(int size) => Math.Max(MinIconSize, Math.Min(MaxIconSize, size));
    public static float Scale(int size) => ClampSize(size) / 18f;
    public static double EdgeMargin(int size) => Math.Max(13, ClampSize(size) * Math.Sqrt(2) / 2 + 1);

    // Preserve the minimum gap, and reserve enough room for the largest icon's diagonal too.
    public static bool Visible(double x, double y, double width, double height, bool round, int size = DefaultIconSize)
    {
        double margin = EdgeMargin(size);
        return width > margin * 2 && height > margin * 2 &&
            !TargetMath.Clamp(x, y, width, height, margin, round).Outside;
    }
}
