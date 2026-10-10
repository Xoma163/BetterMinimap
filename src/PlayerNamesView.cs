using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Shtab.RotatingMinimap;

// Use only the native list of players sharing their positions. Never discover hidden players.
internal sealed class PlayerNamesView : IDisposable
{
    private static readonly FieldInfo PlayerPins = AccessTools.Field(typeof(Minimap), "m_playerPins")
        ?? throw new MissingFieldException("Minimap.m_playerPins");
    private readonly Minimap _map;
    private readonly List<NameView> _labels = new List<NameView>();
    private readonly Vector3[] _corners = new Vector3[4];

    public PlayerNamesView(Minimap map) { _map = map; }

    public void Update(bool visible, int fontSize, bool outline, bool round)
    {
        int used = 0;
        var player = Player.m_localPlayer;
        visible &= _map.m_mode == Minimap.MapMode.Small && player && _map.m_mapImageSmall.gameObject.activeInHierarchy;
        if (visible)
        {
            // m_playerPins contains only other public-position players; never label the local marker.
            if (PlayerPins.GetValue(_map) is List<Minimap.PinData> pins)
            {
                foreach (var pin in pins)
                {
                    // Require the actual small-map icon, respecting native visibility filters.
                    if (!pin.m_uiElement || pin.m_uiElement.parent != _map.m_pinRootSmall ||
                        !pin.m_iconElement || !pin.m_iconElement.enabled) continue;
                    // UpdatePlayerPins already applies Valheim's UGC filter to m_name.
                    Show(pin.m_name, pin.m_uiElement, fontSize, outline, round, ref used);
                }
            }
        }
        for (int i = used; i < _labels.Count; i++) _labels[i].SetVisible(false);
    }

    private void Show(string name, RectTransform icon, int fontSize, bool outline, bool round, ref int used)
    {
        if (!icon || !icon.gameObject.activeInHierarchy || string.IsNullOrEmpty(name)) return;
        var mapRect = _map.m_mapImageSmall.rectTransform;
        var area = mapRect.rect;
        Vector3 center = mapRect.InverseTransformPoint(icon.TransformPoint(icon.rect.center));
        if (TargetMath.Clamp(center.x - area.center.x, center.y - area.center.y,
            area.width, area.height, 0, round).Outside) return;

        // Native player sprites have transparent margins; do not include all of that empty space in the gap.
        icon.GetWorldCorners(_corners);
        float bottom = float.PositiveInfinity, top = float.NegativeInfinity;
        for (int i = 0; i < 4; i++)
        {
            float y = mapRect.InverseTransformPoint(_corners[i]).y;
            bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
        }
        if (used == _labels.Count) _labels.Add(new NameView(_map));
        var label = _labels[used++];
        label.Show(name, fontSize, outline, Mathf.Max(20, area.width - 4),
            mapRect.TransformPoint(new Vector3(center.x, (float)PlayerNameLayout.Top(center.y, top - bottom), 0)));
    }

    public void Dispose()
    {
        foreach (var label in _labels) label.Dispose();
        _labels.Clear();
    }

    private sealed class NameView : IDisposable
    {
        private readonly RectTransform _root;
        private readonly TextMeshProUGUI _text;
        private readonly ExternalTextShadow _outline;
        private string _name = "";
        private int _fontSize = -1;
        private float _maxWidth = -1;

        public NameView(Minimap map)
        {
            _root = (RectTransform)new GameObject("BetterMinimapPlayerName", typeof(RectTransform)).transform;
            // This root is reparented with the small-map pins by MapShapeView, so names clip to both shapes.
            _root.SetParent(map.m_pinNameRootSmall, false);
            _root.pivot = new Vector2(.5f, 1);
            var go = new GameObject("Name", typeof(RectTransform));
            go.transform.SetParent(_root, false);
            _text = go.AddComponent<TextMeshProUGUI>();
            if (map.m_biomeNameSmall && map.m_biomeNameSmall.font) _text.font = map.m_biomeNameSmall.font;
            _text.alignment = TextAlignmentOptions.Top;
            _text.textWrappingMode = TextWrappingModes.NoWrap;
            _text.overflowMode = TextOverflowModes.Ellipsis;
            _text.enableAutoSizing = false;
            _text.richText = false;
            _text.color = Color.white;
            _text.raycastTarget = false;
            _text.maskable = true;
            var rect = _text.rectTransform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            _outline = go.AddComponent<ExternalTextShadow>();
            _outline.PixelWidth = 2;
            _outline.Initialize(_text, respectMask: true);
        }

        public void Show(string name, int fontSize, bool outline, float maxWidth, Vector3 top)
        {
            bool layoutChanged = _name != name || _fontSize != fontSize || _maxWidth != maxWidth;
            if (_name != name)
            {
                _name = name;
                _text.text = name.Replace('\r', ' ').Replace('\n', ' ');
            }
            if (layoutChanged)
            {
                _fontSize = fontSize; _maxWidth = maxWidth;
                _text.fontSize = fontSize;
                Vector2 preferred = _text.GetPreferredValues(_text.text);
                _root.sizeDelta = new Vector2(Mathf.Min(maxWidth, Mathf.Max(20, preferred.x + 4)),
                    Mathf.Max(fontSize * 2, preferred.y + 4));
            }
            _root.position = top;
            _outline.enabled = outline;
            SetVisible(true);
        }

        public void SetVisible(bool visible) => _root.gameObject.SetActive(visible);
        public void Dispose()
        {
            if (!_root) return;
            _root.gameObject.SetActive(false);
            _outline.Clear();
            UnityEngine.Object.Destroy(_root.gameObject);
        }
    }
}
