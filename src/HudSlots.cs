using System;
using System.Collections.Generic;

namespace Shtab.RotatingMinimap;

public enum HudSlotContent { Empty, Biome, Wind, Coordinates, Time, Weight, RealTime }

// Slot indices run from the top of the screen to the bottom: 1, 2, map, 3, 4, 5, 6.
internal static class HudSlots
{
    public const int Count = 6;
    public const int AboveCount = 2;
    public const double Gap = 2;
    public const float FallbackFontSize = 16;
    public const int DefaultTextPercent = 100;
    public const int MinTextPercent = 25;
    public const int MaxTextPercent = 400;

    public static string TextSizeKey(int slot)
    {
        if (slot < 0 || slot >= Count) throw new ArgumentOutOfRangeException(nameof(slot));
        return "Размер текста Slot " + (slot + 1);
    }

    public static float FontSize(HudSlotContent content, int percent, float biomeFontSize)
    {
        if (content == HudSlotContent.Empty || content == HudSlotContent.Wind) return 0;
        return biomeFontSize * Scale(percent);
    }

    public static float Scale(int percent) => Math.Max(MinTextPercent, Math.Min(MaxTextPercent, percent)) / 100f;

    public static bool TryGetTextSizeSlot(string key, out int slot)
    {
        for (slot = 0; slot < Count; slot++)
            if (key == TextSizeKey(slot)) return true;
        slot = -1;
        return false;
    }

    public static double TextHeight(double fontSize, double preferredHeight) =>
        Math.Max(10, Math.Max(fontSize * 1.2, Math.Ceiling(preferredHeight)));

    public static HudSlotContent[] Defaults() => new[] {
        HudSlotContent.Empty, HudSlotContent.Biome, HudSlotContent.Wind,
        HudSlotContent.Empty, HudSlotContent.Empty, HudSlotContent.Empty };

    public static double CenterOffset(int slot, IReadOnlyList<double> heights)
    {
        if (slot < 0 || slot >= Count) throw new ArgumentOutOfRangeException(nameof(slot));
        if (heights.Count != Count) throw new ArgumentException("Expected all HUD slot heights", nameof(heights));
        if (heights[slot] <= 0) return 0;
        double distance = Gap + heights[slot] / 2;
        // Empty slots preserve order, not a blank row's worth of screen space.
        int start = slot < AboveCount ? slot + 1 : AboveCount;
        int end = slot < AboveCount ? AboveCount : slot;
        for (int i = start; i < end; i++)
            if (heights[i] > 0) distance += heights[i] + Gap;
        return slot < AboveCount ? distance : -distance;
    }

    public static HudSlotContent[] Migrate(string biome, string wind)
    {
        var slots = new HudSlotContent[Count];
        int biomeSlot = biome == "Below" ? 2 : 1;
        slots[biomeSlot] = HudSlotContent.Biome;
        bool top = wind == "Top" || wind == "TopLeft" || wind == "TopRight";
        int windSlot = top ? 1 : 2;
        if (windSlot == biomeSlot) windSlot = top ? 0 : 3;
        slots[windSlot] = HudSlotContent.Wind;
        return slots;
    }

    // Remove the prototype's separate switch without unexpectedly showing previously hidden coordinates.
    public static HudSlotContent MigrateCoordinates(HudSlotContent content, bool hadSwitch, bool enabled) =>
        hadSwitch && !enabled && content == HudSlotContent.Coordinates ? HudSlotContent.Empty : content;

    public static double ViewportShift(double top, double bottom, double min, double max)
    {
        // If the entire layout cannot fit, prioritize the upper rows rather than oscillating.
        if (top - bottom > max - min) return max - top;
        if (top > max) return max - top;
        if (bottom < min) return min - bottom;
        return 0;
    }
}
