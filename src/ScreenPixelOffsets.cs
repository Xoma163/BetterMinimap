using UnityEngine;

namespace Shtab.RotatingMinimap;

// Convert screen pixels into the parent's UI coordinates, independent of HUD scaling.
internal static class ScreenPixelOffsets
{
    public static (Vector2 Right, Vector2 Up) InParent(RectTransform source)
    {
        var parent = source.parent as RectTransform;
        if (!parent) return (Vector2.zero, Vector2.zero);
        var canvas = source.GetComponentInParent<Canvas>();
        var root = canvas ? canvas.rootCanvas : null;
        var camera = root && root.renderMode != RenderMode.ScreenSpaceOverlay ? root.worldCamera : null;
        var screen = RectTransformUtility.WorldToScreenPoint(camera, source.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, camera, out var origin) &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen + Vector2.right, camera, out var right) &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen + Vector2.up, camera, out var up))
            return (right - origin, up - origin);
        return (Vector2.zero, Vector2.zero);
    }
}
