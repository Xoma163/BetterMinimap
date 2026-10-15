using System;

namespace Shtab.RotatingMinimap;

internal static class MapOpacity
{
    public const int DefaultTransparency = 0;
    public const int MinTransparency = 0;
    public const int MaxTransparency = 100;
    public static float Alpha(int transparency) =>
        1f - Math.Max(MinTransparency, Math.Min(MaxTransparency, transparency)) / 100f;
}
