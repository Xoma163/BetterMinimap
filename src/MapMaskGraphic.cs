using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Geometry-only stencil: no low-resolution alpha texture extending beyond the brown border.
public sealed class MapMaskGraphic : MaskableGraphic
{
    private bool _round = true;
    public void SetRound(bool round)
    {
        if (_round == round) return;
        _round = round;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect area = rectTransform.rect;
        if (area.width <= 0 || area.height <= 0) return;
        if (!_round)
        {
            vh.AddVert(new Vector3(area.xMin, area.yMin), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(area.xMin, area.yMax), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(area.xMax, area.yMax), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(area.xMax, area.yMin), Color.white, Vector2.zero);
            vh.AddTriangle(0, 1, 2); vh.AddTriangle(0, 2, 3);
            return;
        }
        vh.AddVert(area.center, Color.white, Vector2.zero);
        for (int i = 0; i < MapFrameGeometry.Segments; i++)
        {
            var point = MapFrameGeometry.CirclePoint(area.width, area.height, i, MapFrameGeometry.MaskInset);
            vh.AddVert(area.center + new Vector2((float)point.X, (float)point.Y), Color.white, Vector2.zero);
        }
        for (int i = 0; i < MapFrameGeometry.Segments; i++)
            vh.AddTriangle(0, i + 1, (i + 1) % MapFrameGeometry.Segments + 1);
    }
}
