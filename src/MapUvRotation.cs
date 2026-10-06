using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Only UVs change: the quad, its mask, material and HUD hierarchy stay vanilla.
public sealed class MapUvRotation : BaseMeshEffect
{
    private float _heading;
    internal void SetHeading(float value)
    {
        if (Mathf.Approximately(value, _heading)) return;
        _heading = value;
        if (graphic) graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || Mathf.Approximately(_heading, 0f) || !(graphic is RawImage image)) return;
        Rect rect = image.rectTransform.rect, uv = image.uvRect;
        if (rect.width <= 0 || rect.height <= 0) return;
        var vertex = new UIVertex();
        for (int i = 0; i < mesh.currentVertCount; i++)
        {
            mesh.PopulateUIVertex(ref vertex, i);
            var rotated = MapMath.ScreenToMap(vertex.position.x - rect.center.x,
                vertex.position.y - rect.center.y, uv.center.x, uv.center.y,
                uv.width, uv.height, rect.width, rect.height, _heading);
            vertex.uv0.x = (float)rotated.U;
            vertex.uv0.y = (float)rotated.V;
            mesh.SetUIVertex(vertex, i);
        }
    }
}
