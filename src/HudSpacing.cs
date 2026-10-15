using System;
using System.Collections.Generic;
using System.Globalization;
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
    private readonly SlotView?[] _slots = new SlotView?[HudSlots.Count];
    private readonly double[] _heights = new double[HudSlots.Count];
    private readonly bool[] _visible = new bool[HudSlots.Count];
    private readonly RealClock _realClock = new RealClock();
    private readonly WeightReadout _weightReadout = new WeightReadout();
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

    public void Apply(int percent, HudSlotContent[] contents, int[] textPercents, float heading, bool showMinimap)
    {
        bool small = showMinimap && _map && _map.m_mode == Minimap.MapMode.Small && Player.m_localPlayer &&
            _map.m_mapImageSmall && _map.m_mapImageSmall.gameObject.activeInHierarchy;
        BeforeMap();
        ZoomIndicatorOffset = 0;
        var environment = EnvMan.instance;
        double dayFraction = small && environment ? environment.GetDayFraction() : double.NaN;
        string realTimeText = "", weightText = "";
        if (small)
        {
            bool needsRealTime = false, needsWeight = false;
            for (int i = 0; i < _slots.Length; i++)
            {
                needsRealTime |= contents[i] == HudSlotContent.RealTime;
                needsWeight |= contents[i] == HudSlotContent.Weight;
            }
            if (needsRealTime) realTimeText = _realClock.Update(DateTime.Now);
            if (needsWeight) weightText = _weightReadout.Update(Player.m_localPlayer!.GetInventory().GetTotalWeight(),
                Player.m_localPlayer.GetMaxCarryWeight());
        }
        for (int i = 0; i < _slots.Length; i++)
        {
            bool show = small && contents[i] != HudSlotContent.Empty;
            _visible[i] = show;
            _heights[i] = 0;
            if (show)
            {
                if (_slots[i] == null) _slots[i] = new SlotView(_root, _biome, _map!.m_windMarker, i);
                _heights[i] = _slots[i]!.Show(contents[i], _biome, _map!.m_windMarker,
                    Player.m_localPlayer!.transform.position, heading, dayFraction,
                    contents[i] == HudSlotContent.Weight ? weightText : realTimeText, textPercents[i]);
            }
            _slots[i]?.SetVisible(show);
        }
        if (small)
        {
            var image = _map!.m_mapImageSmall!.rectTransform;
            Rect area = image.rect;
            float topExtent = 0, bottomExtent = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_visible[i]) continue;
                float extent = (float)(Math.Abs(HudSlots.CenterOffset(i, _heights)) + _heights[i] / 2);
                if (i < HudSlots.AboveCount) topExtent = Mathf.Max(topExtent, extent);
                else bottomExtent = Mathf.Max(bottomExtent, extent);
            }
            _scale.FitVertical(image, topExtent, bottomExtent + 32);
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_visible[i]) continue;
                float offset = (float)HudSlots.CenterOffset(i, _heights);
                _slots[i]!.Place(image.TransformPoint(new Vector3(area.center.x,
                    (i < HudSlots.AboveCount ? area.yMax : area.yMin) + offset, 0)), area.width, (float)_heights[i]);
                if (i >= HudSlots.AboveCount) ZoomIndicatorOffset = Mathf.Max(ZoomIndicatorOffset, -offset + (float)_heights[i] / 2);
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
        private Vector3 _coordinates;
        private string? _coordinateText;
        private readonly GameClock _clock = new GameClock();
        private readonly RectTransform _wind;
        private readonly Vector3 _windBaseScale;
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
            _windBaseScale = _wind.localScale;
            foreach (var graphic in _wind.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
                if (graphic is MaskableGraphic masked) masked.maskable = false;
            }
            _contrast.Apply(_text, true);
            _textShadow = _text.GetComponent<ExternalTextShadow>();
            _contrast.Apply(_wind, true);
        }

        public float Show(HudSlotContent content, TMP_Text? biome, RectTransform wind, Vector3 position, float heading,
            double dayFraction, string readout, int textPercent)
        {
            bool arrow = content == HudSlotContent.Wind;
            _wind.gameObject.SetActive(arrow);
            _text.gameObject.SetActive(!arrow);
            if (arrow)
            {
                // Always scale from the captured native size, never from last frame's value.
                _wind.localScale = _windBaseScale * HudSlots.Scale(textPercent);
                _wind.rotation = Quaternion.Euler(0, 0, heading) * wind.rotation;
                // Reserve its diagonal, so no wind direction can overlap a neighboring row.
                return _wind.rect.size.magnitude * Mathf.Max(Mathf.Abs(_wind.localScale.x), Mathf.Abs(_wind.localScale.y));
            }
            _text.enableAutoSizing = false;
            _text.fontSize = HudSlots.FontSize(content, textPercent, biome ? biome!.fontSize : HudSlots.FallbackFontSize);
            _textShadow.PixelWidth = content == HudSlotContent.Coordinates ? 3 : 2;
            _text.fontStyle = content == HudSlotContent.Biome && biome ? biome!.fontStyle : FontStyles.Normal;
            _text.color = content == HudSlotContent.Biome && biome ? biome!.color : Color.white;
            if (content == HudSlotContent.Biome)
            {
                // The native label is animated. Its transient alpha must not hide our persistent slot.
                var color = _text.color; color.a = 1; _text.color = color;
                _text.text = biome ? biome!.text : "";
                return TextHeight();
            }
            if (content == HudSlotContent.Time)
            {
                // Assign even when the minute is unchanged: this slot may have just shown a biome/coordinates.
                // A cached string also keeps all TMP outline copies in sync.
                _text.text = _clock.Update(dayFraction);
                return TextHeight();
            }
            if (content == HudSlotContent.Weight || content == HudSlotContent.RealTime)
            {
                _text.text = readout;
                return TextHeight();
            }
            var rounded = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
            if (_coordinateText == null || rounded != _coordinates)
            {
                _coordinates = rounded;
                _coordinateText = string.Format(CultureInfo.InvariantCulture, "X: {0:0}  Y: {1:0}  Z: {2:0}",
                    rounded.x, rounded.y, rounded.z);
            }
            // TMP's numeric SetText leaves m_text stale after its backing string has been read once.
            // Use a stable string so ALL outline copies receive the same text, including while idle.
            _text.text = _coordinateText;
            return TextHeight();
        }

        private float TextHeight() => (float)HudSlots.TextHeight(_text.fontSize, _text.GetPreferredValues(_text.text).y);

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
