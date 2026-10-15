namespace Shtab.RotatingMinimap;

internal static class SlotRowLayout
{
    public const float SingleLineMinWidth = 720;
    public const float SingleLineHeight = 46;
    public const float TwoLineHeight = 88;
    public static bool SingleLine(double width) => width >= SingleLineMinWidth;
    public static float Height(double width) => SingleLine(width) ? SingleLineHeight : TwoLineHeight;
}
