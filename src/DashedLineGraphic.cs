using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

public sealed class DashedLineGraphic : MaskableGraphic
{
    private bool _dashed = true;
    public void SetDashed(bool dashed)
    {
        if (_dashed == dashed) return;
        _dashed = dashed;
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = rectTransform.rect;
        var ranges = _dashed ? MapMath.DashRanges(rect.height) :
            new[] { (Start: -(double)rect.height / 2, End: (double)rect.height / 2) };
        foreach (var dash in ranges)
        {
            int start = mesh.currentVertCount;
            float y0 = rect.center.y + (float)dash.Start;
            float y1 = rect.center.y + (float)dash.End;
            mesh.AddVert(new Vector3(rect.xMin, y0), color, Vector2.zero);
            mesh.AddVert(new Vector3(rect.xMin, y1), color, Vector2.zero);
            mesh.AddVert(new Vector3(rect.xMax, y1), color, Vector2.zero);
            mesh.AddVert(new Vector3(rect.xMax, y0), color, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
