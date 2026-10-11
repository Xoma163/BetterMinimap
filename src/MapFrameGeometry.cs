using System;

namespace Shtab.RotatingMinimap;

// Shared geometry prevents the stencil mask and visible border from disagreeing at the edge.
internal static class MapFrameGeometry
{
    public const int Segments = 256;
    public const float OuterInset = -.6f;
    public const float InnerInset = .9f;
    public const float MaskInset = .15f;

    public static (double X, double Y) CirclePoint(double width, double height, int index, double inset)
    {
        double angle = index * Math.PI * 2 / Segments;
        double radius = Math.Max(0, Math.Min(width, height) / 2 - inset);
        return (Math.Cos(angle) * radius, Math.Sin(angle) * radius);
    }
}
