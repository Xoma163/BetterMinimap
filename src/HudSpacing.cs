using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Slot visuals are separate from vanilla UI. Never reparent or change large-map labels.
internal sealed class HudSpacing : IDisposable
{
    private readonly Minimap _map;
    private readonly MapScaleView _scale;
    private readonly Transform _root;
    private readonly TMP_Text? _biome;
    private readonly SlotView?[] _slots = new SlotView?[4];
    private readonly float[] _heights = new float[4];
    private readonly bool[] _visible = new bool[4];
    private RectTransform? _buffs;
    private Vector2 _buffBase, _buffLast;
    private bool _buffShifted, _hidden;
    private readonly Graphic[] _originalGraphics;
    private readonly bool[] _originalEnabled;
    public float ZoomIndicatorOffset { get; private set; }

    public HudSpacing(Minimap map, MapScaleView scale)
    {
        _map = map; _scale = scale;
        _root = map.m_mapImageSmall.transform.parent;
        _biome = map.m_biomeNameSmall;
        var graphics = new List<Graphic>();
        if (_biome) graphics.AddRange(_biome.GetComponentsInChildren<Graphic>(true));
        if (map.m_windMarker) graphics.AddRange(map.m_windMarker.GetComponentsInChildren<Graphic>(true));
        _originalGraphics = graphics.ToArray();
        _originalEnabled = new bool[_originalGraphics.Length];
    }

    // Restore before vanilla updates so native visibility/text updates are still respected.
    public void BeforeMap()
    {
        _scale.RestorePosition();
        if (!_hidden) return;
        for (int i = 0; i < _originalGraphics.Length; i++)
            if (_originalGraphics[i]) _originalGraphics[i].enabled = _originalEnabled[i];
        _hidden = false;
    }

    public void Apply(int percent, HudSlotContent[] contents, float heading, bool showMinimap)
    {
        bool small = showMinimap && _map && _map.m_mode == Minimap.MapMode.Small && Player.m_localPlayer &&
            _map.m_mapImageSmall && _map.m_mapImageSmall.gameObject.activeInHierarchy;
        BeforeMap();
        ZoomIndicatorOffset = 0;
        for (int i = 0; i < 4; i++)
        {
            bool show = small && contents[i] != HudSlotContent.Empty;
            _visible[i] = show;
            _heights[i] = 0;
            if (show)
            {
                if (_slots[i] == null) _slots[i] = new SlotView(_root, _biome, _map!.m_windMarker, i);
                _heights[i] = _slots[i]!.Show(contents[i], _biome, _map!.m_windMarker,
                    Player.m_localPlayer!.transform.position, heading);
            }
            _slots[i]?.SetVisible(show);
        }
        if (small)
        {
            var image = _map!.m_mapImageSmall!.rectTransform;
            Rect area = image.rect;
            float topExtent = 0, bottomExtent = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!_visible[i]) continue;
                float extent = Mathf.Abs((float)HudSlots.CenterOffset(i, _heights[i], _heights[i < 2 ? 1 : 2])) + _heights[i] / 2;
                if (i < 2) topExtent = Mathf.Max(topExtent, extent);
                else bottomExtent = Mathf.Max(bottomExtent, extent);
            }
            _scale.FitVertical(image, topExtent, bottomExtent + 32);
            for (int i = 0; i < 4; i++)
            {
                if (!_visible[i]) continue;
                float offset = (float)HudSlots.CenterOffset(i, _heights[i], _heights[i < 2 ? 1 : 2]);
                _slots[i]!.Place(image.TransformPoint(new Vector3(area.center.x,
                    (i < 2 ? area.yMax : area.yMin) + offset, 0)), area.width, _heights[i]);
                if (i >= 2) ZoomIndicatorOffset = Mathf.Max(ZoomIndicatorOffset, -offset + _heights[i] / 2);
            }
            for (int i = 0; i < _originalGraphics.Length; i++)
            {
                if (!_originalGraphics[i]) continue;
                _originalEnabled[i] = _originalGraphics[i].enabled;
                _originalGraphics[i].enabled = false;
            }
            _hidden = true;
        }
        PlaceBuffs(small, percent);
    }

    private void PlaceBuffs(bool small, int percent)
    {
        var hud = Hud.instance;
        var buffs = hud ? hud.m_statusEffectListRoot : null;
        if (_buffs != buffs) { RestoreBuffs(); _buffs = buffs; }
        if (!small || percent <= 100 || !_buffs || !_buffs!.parent) { RestoreBuffs(); return; }
        Vector2 current = _buffs.anchoredPosition;
        if (!_buffShifted || !Mathf.Approximately(current.x, _buffLast.x)) _buffBase.x = current.x;
        _buffBase.y = current.y;
        float extra = _buffs.parent.InverseTransformVector(_scale.LeftExpansionWorld(percent)).x;
        _buffLast = _buffBase - new Vector2(Mathf.Max(0, extra), 0);
        _buffs.anchoredPosition = _buffLast;
        _buffShifted = true;
    }

    private void RestoreBuffs()
    {
        if (_buffShifted && _buffs)
        {
            var current = _buffs!.anchoredPosition;
            if (Mathf.Approximately(current.x, _buffLast.x))
                _buffs.anchoredPosition = new Vector2(_buffBase.x, current.y);
        }
        _buffShifted = false;
    }

    public void Dispose()
    {
        BeforeMap();
        RestoreBuffs();
        foreach (var slot in _slots) slot?.Dispose();
        _buffs = null;
    }

    private sealed class SlotView : IDisposable
    {
        private readonly RectTransform _root;
        private readonly TextMeshProUGUI _text;
        private readonly RectTransform _wind;
        private readonly ExternalTextShadow _textShadow;
        private readonly HudContrast _contrast = new HudContrast();

        public SlotView(Transform parent, TMP_Text? biome, RectTransform wind, int index)
        {
            _root = (RectTransform)new GameObject("BetterMinimapSlot" + (index + 1), typeof(RectTransform)).transform;
            _root.SetParent(parent, false);
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(_root, false);
            _text = go.AddComponent<TextMeshProUGUI>();
            if (biome && biome!.font) _text.font = biome.font;
            _text.alignment = TextAlignmentOptions.Center;
            _text.textWrappingMode = TextWrappingModes.NoWrap;
            // A short row must never make TMP discard the biome's entire line.
            _text.overflowMode = TextOverflowModes.Overflow;
            _text.raycastTarget = false;
            _text.maskable = false;
            _text.rectTransform.anchorMin = Vector2.zero;
            _text.rectTransform.anchorMax = Vector2.one;
            _text.rectTransform.offsetMin = _text.rectTransform.offsetMax = Vector2.zero;
            // Clone only the wind visual; vanilla keeps ownership of its original marker.
            _wind = UnityEngine.Object.Instantiate(wind, _root, false);
            _wind.name = "Wind";
            _wind.anchorMin = _wind.anchorMax = _wind.pivot = new Vector2(.5f, .5f);
            _wind.sizeDelta = wind.rect.size;
            _wind.anchoredPosition3D = Vector3.zero;
            foreach (var graphic in _wind.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
                if (graphic is MaskableGraphic masked) masked.maskable = false;
            }
            _contrast.Apply(_text, true);
            _textShadow = _text.GetComponent<ExternalTextShadow>();
            _contrast.Apply(_wind, true);
        }

        public float Show(HudSlotContent content, TMP_Text? biome, RectTransform wind, Vector3 position, float heading)
        {
            bool arrow = content == HudSlotContent.Wind;
            _wind.gameObject.SetActive(arrow);
            _text.gameObject.SetActive(!arrow);
            if (arrow)
            {
                _wind.rotation = Quaternion.Euler(0, 0, heading) * wind.rotation;
                // Reserve its diagonal, so no wind direction can overlap a neighboring row.
                return _wind.rect.size.magnitude * Mathf.Max(Mathf.Abs(_wind.localScale.x), Mathf.Abs(_wind.localScale.y));
            }
            _text.enableAutoSizing = false;
            _text.fontSize = content == HudSlotContent.Biome && biome ? biome!.fontSize : HudSlots.CoordinatesFontSize;
            _textShadow.PixelWidth = content == HudSlotContent.Coordinates ? 3 : 2;
            _text.fontStyle = content == HudSlotContent.Biome && biome ? biome!.fontStyle : FontStyles.Normal;
            _text.color = content == HudSlotContent.Biome && biome ? biome!.color : Color.white;
            if (content == HudSlotContent.Biome)
            {
                // The native label is animated. Its transient alpha must not hide our persistent slot.
                var color = _text.color; color.a = 1; _text.color = color;
                _text.text = biome ? biome!.text : "";
                return (float)HudSlots.BiomeHeight(_text.GetPreferredValues(_text.text).y);
            }
            else _text.SetText("X: {0:0}  Y: {1:0}  Z: {2:0}", Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
            return Mathf.Max(10, _text.fontSize * 1.2f);
        }

        public void Place(Vector3 position, float width, float height)
        {
            _root.position = position;
            _root.sizeDelta = new Vector2(width, height);
        }
        public void SetVisible(bool visible) => _root.gameObject.SetActive(visible);
        public void Dispose()
        {
            _contrast.Dispose();
            if (_root) { _root.gameObject.SetActive(false); UnityEngine.Object.Destroy(_root.gameObject); }
        }
    }
}
