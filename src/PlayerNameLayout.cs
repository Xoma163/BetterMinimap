using System;

namespace Shtab.RotatingMinimap;

internal static class PlayerNameLayout
{
    public const int DefaultFontSize = 8;
    public const int MinFontSize = 4;
    public const int MaxFontSize = 20;

    // Native player sprites have transparent padding inside their UI rectangle.
    // Bring the text toward the visible icon rather than placing it below that empty padding.
    public static double Top(double centerY, double iconHeight) => centerY - Math.Max(0, iconHeight) * .3 - 1;
}
