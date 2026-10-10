using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

internal static class MinimapProjection
{
    public static bool TryProject(Minimap map, RawImage image, Vector3 world, float heading, out Vector2 point)
    {
        point = Vector2.zero;
        Rect uv = image.uvRect, area = image.rectTransform.rect;
        if (uv.width <= 0 || uv.height <= 0 || area.width <= 0 || area.height <= 0 ||
            map.m_pixelSize <= 0 || map.m_textureSize <= 0) return false;
        double u = (world.x / map.m_pixelSize + map.m_textureSize / 2) / map.m_textureSize;
        double v = (world.z / map.m_pixelSize + map.m_textureSize / 2) / map.m_textureSize;
        var projected = MapMath.MapToScreen(u, v, uv.center.x, uv.center.y, uv.width, uv.height,
            area.width, area.height, heading);
        point = new Vector2((float)projected.X, (float)projected.Y);
        return true;
    }
}
