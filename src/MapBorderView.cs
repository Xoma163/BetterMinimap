using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

internal sealed class MapBorderView : IDisposable
{
    private readonly Minimap _map;
    private readonly MapBorderGraphic _graphic;

    public MapBorderView(Minimap map)
    {
        _map = map;
        var go = new GameObject("BetterMinimapBorder", typeof(RectTransform));
        go.transform.SetParent(map.m_mapImageSmall.transform, false);
        _graphic = go.AddComponent<MapBorderGraphic>();
        _graphic.raycastTarget = false;
        _graphic.maskable = false;
        var rect = _graphic.rectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        _graphic.RecalculateClipping();
        _graphic.RecalculateMasking();
        _graphic.canvasRenderer.DisableRectClipping();
    }

    public void Update(bool round)
    {
        _graphic.gameObject.SetActive(_map.m_mode == Minimap.MapMode.Small && Player.m_localPlayer &&
            _map.m_mapImageSmall.gameObject.activeInHierarchy);
        _graphic.SetRound(round);
    }

    public void Dispose()
    {
        if (_graphic) { _graphic.gameObject.SetActive(false); UnityEngine.Object.Destroy(_graphic.gameObject); }
    }
}

// One thin brown band, without a second dark outer border.
public sealed class MapBorderGraphic : MaskableGraphic
{
    private bool _round;
    public void SetRound(bool round)
    {
        if (_round == round) return;
        _round = round; SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Band(vh, -.6f, .9f, new Color32(137, 91, 48, 255));
    }

    private void Band(VertexHelper vh, float outerInset, float innerInset, Color32 tint)
    {
        Rect area = rectTransform.rect;
        if (area.width <= 3 || area.height <= 3) return;
        int segments = _round ? 128 : 4;
        for (int i = 0; i < segments; i++)
        {
            int start = vh.currentVertCount;
            vh.AddVert(Point(area, i, segments, outerInset), tint, Vector2.zero);
            vh.AddVert(Point(area, i + 1, segments, outerInset), tint, Vector2.zero);
            vh.AddVert(Point(area, i + 1, segments, innerInset), tint, Vector2.zero);
            vh.AddVert(Point(area, i, segments, innerInset), tint, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }

    private Vector3 Point(Rect rect, int index, int segments, float inset)
    {
        if (_round)
        {
            float angle = index * Mathf.PI * 2 / segments;
            float radius = Mathf.Min(rect.width, rect.height) / 2 - inset;
            return rect.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        int corner = index % 4;
        return new Vector3(corner == 0 || corner == 3 ? rect.xMin + inset : rect.xMax - inset,
            corner < 2 ? rect.yMin + inset : rect.yMax - inset, 0);
    }
}
