using System;
using UnityEngine;

namespace Shtab.RotatingMinimap;

internal sealed class MapScaleView : IDisposable
{
    private readonly Transform _root;
    private readonly Vector3 _original;
    private Vector3 _basePosition, _lastPosition;
    private bool _shifted;
    public MapScaleView(Minimap map)
    {
        // Capture before MapShapeView reparents the inner map into its mask.
        // The vanilla parent includes biome, wind, click area and both pin roots.
        _root = map.m_mapImageSmall.transform.parent;
        _original = _root.localScale;
    }

    public void Apply(int percent)
    {
        if (!_root) return;
        float factor = (float)MapMath.ScaleFactor(percent);
        _root.localScale = new Vector3(_original.x * factor, _original.y * factor, _original.z);
    }

    public void Dispose()
    {
        RestorePosition();
        if (_root) _root.localScale = _original;
    }

    public void RestorePosition()
    {
        if (_shifted && _root && (_root.localPosition - _lastPosition).sqrMagnitude < .001f)
            _root.localPosition = _basePosition;
        _shifted = false;
    }

    public void FitVertical(RectTransform map, float above, float below)
    {
        RestorePosition();
        if (!_root) return;
        var canvas = _root.GetComponentInParent<Canvas>();
        if (!canvas || !(canvas.rootCanvas.transform is RectTransform viewport)) return;
        var area = map.rect;
        float top = viewport.InverseTransformPoint(map.TransformPoint(new Vector3(area.center.x, area.yMax + above, 0))).y;
        float bottom = viewport.InverseTransformPoint(map.TransformPoint(new Vector3(area.center.x, area.yMin - below, 0))).y;
        float shift = (float)HudSlots.ViewportShift(top, bottom, viewport.rect.yMin + 8, viewport.rect.yMax - 8);
        if (Mathf.Abs(shift) < .01f) return;
        _basePosition = _root.localPosition;
        _root.position += viewport.TransformVector(new Vector3(0, shift, 0));
        _lastPosition = _root.localPosition;
        _shifted = true;
    }

    public Vector3 LeftExpansionWorld(int percent)
    {
        if (!_root || !_root.parent || !(_root is RectTransform rect)) return Vector3.zero;
        float extra = (float)MapMath.BuffShift(rect.rect.width, _original.x, rect.pivot.x, percent);
        return _root.parent.TransformVector(new Vector3(extra, 0, 0));
    }
}
