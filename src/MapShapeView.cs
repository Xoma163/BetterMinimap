using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// A stencil mask for the small map only. No shared shader or large-map changes.
internal sealed class MapShapeView : IDisposable
{
    private readonly Minimap _map;
    private GameObject? _clip;
    private Texture2D? _texture;
    private Sprite? _sprite;
    private bool _mapMaskable;
    private readonly List<Layout> _moved = new List<Layout>();
    private readonly List<Graphic> _hiddenFrames = new List<Graphic>();
    private readonly List<(MaskableGraphic Graphic, bool Maskable)> _outside = new List<(MaskableGraphic, bool)>();
    private static readonly string[] StencilNames = {
        "_Stencil", "_StencilComp", "_StencilOp", "_StencilReadMask", "_StencilWriteMask", "_ColorMask"
    };
    private readonly int[] _stencil = new int[StencilNames.Length];

    public MapShapeView(Minimap map) { _map = map; }

    public void Apply(bool round)
    {
        if (!_map || !_map.m_mapImageSmall) return;
        if (!round) { Dispose(); return; }
        if (!_clip) Create();
        // Unity's stencil variant is a cached material; vanilla changes the source each frame.
        var image = _map.m_mapImageSmall;
        var source = image.material;
        var masked = image.materialForRendering;
        if (!source || !masked || source == masked) return;
        for (int i = 0; i < StencilNames.Length; i++)
            if (masked.HasProperty(StencilNames[i])) _stencil[i] = masked.GetInt(StencilNames[i]);
        masked.CopyPropertiesFromMaterial(source);
        for (int i = 0; i < StencilNames.Length; i++)
            if (masked.HasProperty(StencilNames[i])) masked.SetInt(StencilNames[i], _stencil[i]);
    }

    private void Create()
    {
        var mapRect = _map.m_mapImageSmall.rectTransform;
        _mapMaskable = _map.m_mapImageSmall.maskable;
        _clip = new GameObject("BetterMinimapCircle", typeof(RectTransform));
        var rect = (RectTransform)_clip.transform;
        rect.SetParent(mapRect.parent, false);
        rect.SetSiblingIndex(mapRect.GetSiblingIndex());
        rect.anchorMin = mapRect.anchorMin;
        rect.anchorMax = mapRect.anchorMax;
        rect.pivot = mapRect.pivot;
        rect.sizeDelta = mapRect.sizeDelta;
        rect.anchoredPosition3D = mapRect.anchoredPosition3D;
        rect.localRotation = mapRect.localRotation;
        rect.localScale = mapRect.localScale;
        const int size = 128;
        _texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        _texture.wrapMode = TextureWrapMode.Clamp;
        _texture.filterMode = FilterMode.Bilinear;
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x + .5f - size / 2f, dy = y + .5f - size / 2f;
            byte alpha = (byte)(255 * Mathf.Clamp01(size / 2f - Mathf.Sqrt(dx * dx + dy * dy)));
            pixels[y * size + x] = new Color32(255, 255, 255, alpha);
        }
        _texture.SetPixels32(pixels);
        _texture.Apply(false, true);
        _sprite = Sprite.Create(_texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f),
            100, 0, SpriteMeshType.FullRect);
        var circle = _clip.AddComponent<Image>();
        circle.sprite = _sprite;
        circle.preserveAspect = true;
        circle.raycastTarget = false;
        _clip.AddComponent<Mask>().showMaskGraphic = false;

        // Measure the graphic itself, not its children (a background can parent the HUD).
        // Include RawImage backgrounds, but never the actual map or text/marker graphics.
        var corners = new Vector3[4];
        // In Valheim 1.0.17 m_mapSmall is the inner "map", NOT its parent "small".
        // The parent's own Image is the square translucent background.
        foreach (var frame in mapRect.parent.GetComponentsInChildren<Graphic>(true))
        {
            if (!frame.enabled || frame.color.a <= 0 || frame == circle || frame == _map.m_mapImageSmall ||
                frame is TMP_Text || frame is Text ||
                frame.transform.IsChildOf(_map.m_smallMarker) ||
                frame.transform.IsChildOf(_map.m_smallShipMarker) ||
                frame.transform.IsChildOf(_map.m_pinRootSmall) ||
                frame.transform.IsChildOf(_map.m_pinNameRootSmall) ||
                frame.transform.IsChildOf(_map.m_windMarker)) continue;
            frame.rectTransform.GetWorldCorners(corners);
            var bounds = new Bounds(mapRect.InverseTransformPoint(corners[0]), Vector3.zero);
            for (int i = 1; i < corners.Length; i++) bounds.Encapsulate(mapRect.InverseTransformPoint(corners[i]));
            Rect area = mapRect.rect;
            if (Mathf.Abs(bounds.center.x - area.center.x) < 5 &&
                Mathf.Abs(bounds.center.y - area.center.y) < 5 &&
                bounds.size.x >= area.width * .95f && bounds.size.x <= area.width * 1.15f &&
                bounds.size.y >= area.height * .95f && bounds.size.y <= area.height * 1.15f)
            {
                _hiddenFrames.Add(frame);
                frame.enabled = false;
            }
        }
        Move(mapRect, rect);
        Move(_map.m_pinRootSmall, rect);
        Move(_map.m_pinNameRootSmall, rect);
        Move(_map.m_smallMarker, rect);
        Move(_map.m_smallShipMarker, rect);
        _map.m_mapImageSmall.maskable = true;
        _map.m_mapImageSmall.RecalculateMasking();
        // HudSpacing owns wind clipping for both map shapes and external positions.
        var biome = AccessTools.Field(typeof(Minimap), "m_biomeNameSmall")?.GetValue(_map) as Component;
        KeepOutsideMask(biome);
        // HudSpacing owns biome/wind outlines independently of the map shape.
    }

    private void KeepOutsideMask(Component? root)
    {
        if (!root) return;
        foreach (var graphic in root!.GetComponentsInChildren<MaskableGraphic>(true))
        {
            _outside.Add((graphic, graphic.maskable));
            graphic.maskable = false;
            graphic.RecalculateMasking();
        }
    }

    private void Move(RectTransform item, Transform target)
    {
        if (!item || item.IsChildOf(target)) return;
        _moved.Add(new Layout(item));
        item.SetParent(target, true);
    }

    public void Dispose()
    {
        // Detach children before destroying their temporary parent.
        for (int i = _moved.Count - 1; i >= 0; i--) _moved[i].Restore();
        _moved.Clear();
        foreach (var frame in _hiddenFrames) if (frame) frame.enabled = true;
        _hiddenFrames.Clear();
        foreach (var saved in _outside)
        {
            if (!saved.Graphic) continue;
            saved.Graphic.maskable = saved.Maskable;
            saved.Graphic.RecalculateMasking();
        }
        _outside.Clear();
        if (_clip && _map && _map.m_mapImageSmall)
        {
            _map.m_mapImageSmall.maskable = _mapMaskable;
            _map.m_mapImageSmall.RecalculateMasking();
        }
        if (_clip) { _clip!.SetActive(false); UnityEngine.Object.Destroy(_clip); }
        if (_sprite) UnityEngine.Object.Destroy(_sprite);
        if (_texture) UnityEngine.Object.Destroy(_texture);
        _clip = null; _sprite = null; _texture = null;
    }

    private sealed class Layout
    {
        private readonly RectTransform _item;
        private readonly Transform _parent;
        private readonly int _index;
        private readonly Vector2 _min, _max, _pivot, _size;
        private readonly Vector3 _position, _scale;
        private readonly Quaternion _rotation;
        public Layout(RectTransform item)
        {
            _item = item; _parent = item.parent; _index = item.GetSiblingIndex();
            _min = item.anchorMin; _max = item.anchorMax; _pivot = item.pivot;
            _size = item.sizeDelta; _position = item.anchoredPosition3D;
            _scale = item.localScale; _rotation = item.localRotation;
        }
        public void Restore()
        {
            if (!_item || !_parent) return;
            _item.SetParent(_parent, false);
            _item.SetSiblingIndex(_index);
            _item.anchorMin = _min; _item.anchorMax = _max; _item.pivot = _pivot;
            _item.sizeDelta = _size; _item.anchoredPosition3D = _position;
            _item.localScale = _scale; _item.localRotation = _rotation;
        }
    }
}
