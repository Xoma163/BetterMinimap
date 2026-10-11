using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Temporarily promote native player icons after the map update. Restore before vanilla accesses
// their parent/layout again (UpdatePins otherwise destroys markers under an unexpected parent).
// A dedicated, normally sorted masked layer avoids overrideSorting canvases bypassing the stencil.
internal sealed class PlayerIconLayer : IDisposable
{
    private static readonly FieldInfo PlayerPins = AccessTools.Field(typeof(Minimap), "m_playerPins")
        ?? throw new MissingFieldException("Minimap.m_playerPins");
    private readonly Minimap _map;
    private readonly List<Layout> _moved = new List<Layout>();
    private readonly List<(Graphic Graphic, bool Raycast)> _raycasts = new List<(Graphic, bool)>();
    private readonly List<(MaskableGraphic Graphic, bool Maskable)> _maskables = new List<(MaskableGraphic, bool)>();
    private readonly List<Graphic> _graphics = new List<Graphic>();
    private readonly RectTransform _root;
    private readonly MapMaskGraphic _mask;

    public PlayerIconLayer(Minimap map)
    {
        _map = map;
        var go = new GameObject("BetterMinimapPlayerIconLayer", typeof(RectTransform));
        go.SetActive(false);
        _root = (RectTransform)go.transform;
        _root.SetParent(map.m_smallRoot.transform, false);
        _root.anchorMin = _root.anchorMax = _root.pivot = new Vector2(.5f, .5f);
        _mask = go.AddComponent<MapMaskGraphic>();
        _mask.raycastTarget = false;
        go.AddComponent<Mask>().showMaskGraphic = false;
    }

    public void BeforeMap()
    {
        for (int i = _moved.Count - 1; i >= 0; i--) _moved[i].Restore();
        _moved.Clear();
        foreach (var saved in _raycasts) if (saved.Graphic) saved.Graphic.raycastTarget = saved.Raycast;
        _raycasts.Clear();
        foreach (var saved in _maskables)
        {
            if (!saved.Graphic) continue;
            saved.Graphic.maskable = saved.Maskable;
            saved.Graphic.RecalculateMasking();
        }
        _maskables.Clear();
        if (_root) _root.gameObject.SetActive(false);
    }

    public void Update(bool showSmall, bool round)
    {
        BeforeMap();
        bool large = _map.m_mode == Minimap.MapMode.Large;
        if (!Player.m_localPlayer || (!large && (_map.m_mode != Minimap.MapMode.Small || !showSmall))) return;
        var image = large ? _map.m_mapImageLarge : _map.m_mapImageSmall;
        if (!image.gameObject.activeInHierarchy) return;
        var parent = large ? _map.m_largeRoot.transform : _map.m_smallRoot.transform;
        var mapRect = image.rectTransform;
        if (_root.parent != parent) _root.SetParent(parent, false);
        _root.SetAsLastSibling();
        _root.position = mapRect.TransformPoint(mapRect.rect.center);
        _root.rotation = mapRect.rotation;
        _root.sizeDelta = mapRect.rect.size;
        var mapScale = mapRect.lossyScale;
        var parentScale = parent.lossyScale;
        if (Mathf.Abs(parentScale.x) < .0001f || Mathf.Abs(parentScale.y) < .0001f) return;
        _root.localScale = new Vector3(mapScale.x / parentScale.x, mapScale.y / parentScale.y, 1);
        _mask.SetRound(!large && round);
        _root.gameObject.SetActive(true);
        var pinRoot = large ? _map.m_pinRootLarge : _map.m_pinRootSmall;
        if (PlayerPins.GetValue(_map) is List<Minimap.PinData> pins)
            foreach (var pin in pins)
                if (pin.m_uiElement && pin.m_uiElement.parent == pinRoot) Move(pin.m_uiElement);
        // The local arrow is last even when another player occupies the exact same position.
        Move(large ? _map.m_largeMarker : _map.m_smallMarker);
    }

    private void Move(RectTransform icon)
    {
        if (!icon || !icon.gameObject.activeInHierarchy) return;
        _moved.Add(new Layout(icon));
        icon.GetComponentsInChildren<Graphic>(true, _graphics);
        foreach (var graphic in _graphics)
        {
            _raycasts.Add((graphic, graphic.raycastTarget));
            graphic.raycastTarget = false;
            if (graphic is MaskableGraphic maskable)
            {
                _maskables.Add((maskable, maskable.maskable));
                maskable.maskable = true;
            }
        }
        _graphics.Clear();
        icon.SetParent(_root, true);
        icon.SetAsLastSibling();
    }

    public void Dispose()
    {
        BeforeMap();
        if (_root) UnityEngine.Object.Destroy(_root.gameObject);
    }

    private readonly struct Layout
    {
        private readonly RectTransform _rect;
        private readonly Transform _parent;
        private readonly int _index;
        private readonly Vector2 _min, _max, _pivot, _size;
        private readonly Vector3 _position, _scale;
        private readonly Quaternion _rotation;
        public Layout(RectTransform rect)
        {
            _rect = rect; _parent = rect.parent; _index = rect.GetSiblingIndex();
            _min = rect.anchorMin; _max = rect.anchorMax; _pivot = rect.pivot;
            _size = rect.sizeDelta; _position = rect.anchoredPosition3D;
            _scale = rect.localScale; _rotation = rect.localRotation;
        }
        public void Restore()
        {
            if (!_rect || !_parent) return;
            _rect.SetParent(_parent, false);
            _rect.SetSiblingIndex(_index);
            _rect.anchorMin = _min; _rect.anchorMax = _max; _rect.pivot = _pivot;
            _rect.sizeDelta = _size; _rect.anchoredPosition3D = _position;
            _rect.localScale = _scale; _rect.localRotation = _rotation;
        }
    }
}
