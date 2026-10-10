using System;

namespace Shtab.RotatingMinimap;

public enum HudSlotContent { Empty, Biome, Wind, Coordinates }

// Slot indices run from the top of the screen to the bottom: 1, 2, map, 3, 4.
internal static class HudSlots
{
    public const double Gap = 2;
    public const float CoordinatesFontSize = 9;
    public static double BiomeHeight(double preferredHeight) => Math.Max(26, Math.Ceiling(preferredHeight + 4));

    public static double CenterOffset(int slot, double ownHeight, double innerHeight)
    {
        if (slot < 0 || slot > 3) throw new ArgumentOutOfRangeException(nameof(slot));
        double distance = Gap + ownHeight / 2;
        // Empty slots preserve order, not a blank row's worth of screen space.
        if ((slot == 0 || slot == 3) && innerHeight > 0) distance += innerHeight + Gap;
        return slot < 2 ? distance : -distance;
    }

    public static HudSlotContent[] Migrate(string biome, string wind)
    {
        var slots = new HudSlotContent[4];
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
