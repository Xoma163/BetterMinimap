using System;
using UnityEngine;

namespace Shtab.RotatingMinimap;

internal sealed class MapScaleView : IDisposable
{
    private readonly Transform _root;
    private readonly Vector3 _original;
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
        if (_root) _root.localScale = _original;
    }

    public Vector3 LeftExpansionWorld(int percent)
    {
        if (!_root || !_root.parent || !(_root is RectTransform rect)) return Vector3.zero;
        float extra = (float)MapMath.BuffShift(rect.rect.width, _original.x, rect.pivot.x, percent);
        return _root.parent.TransformVector(new Vector3(extra, 0, 0));
    }
}
