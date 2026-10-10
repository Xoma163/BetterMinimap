using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Read only the game's live pings; keep their native lifetime and network visibility.
internal sealed class PingIndicators : IDisposable
{
    private static readonly FieldInfo PingPins = AccessTools.Field(typeof(Minimap), "m_pingPins")
        ?? throw new MissingFieldException("Minimap.m_pingPins");
    private static readonly FieldInfo VisibleTypes = AccessTools.Field(typeof(Minimap), "m_visibleIconTypes")
        ?? throw new MissingFieldException("Minimap.m_visibleIconTypes");
    private readonly Minimap _map;
    private readonly Transform _parent;
    private readonly List<TargetGraphic> _icons = new List<TargetGraphic>();
    private readonly List<(Image Image, bool Enabled)> _hidden = new List<(Image, bool)>();

    public PingIndicators(Minimap map)
    {
        _map = map; _parent = map.m_mapImageSmall.transform.parent;
    }

    public void BeforeMap()
    {
        foreach (var saved in _hidden) if (saved.Image) saved.Image.enabled = saved.Enabled;
        _hidden.Clear();
    }

    public void Update(float heading, bool round)
    {
        BeforeMap();
        int used = 0;
        bool visible = _map.m_mode == Minimap.MapMode.Small && Player.m_localPlayer &&
            _map.m_mapImageSmall.gameObject.activeInHierarchy;
        if (VisibleTypes.GetValue(_map) is bool[] types && (int)Minimap.PinType.Ping < types.Length)
            visible &= types[(int)Minimap.PinType.Ping];
        if (visible && PingPins.GetValue(_map) is List<Minimap.PinData> pins)
        {
            var image = _map.m_mapImageSmall;
            Rect area = image.rectTransform.rect;
            foreach (var pin in pins)
            {
                if (!pin.m_icon || !MinimapProjection.TryProject(_map, image, pin.m_pos, heading, out var point)) continue;
                // Use the maximum animated size to avoid ring/arrow flicker as the ping pulses.
                float nativeRadius = _map.m_pinSizeSmall * (pin.m_doubleSize ? 1 : .5f);
                var edge = TargetMath.EdgePing(point.x, point.y, area.width, area.height, 14, round, nativeRadius);
                if (!edge.Visible) continue;
                if (pin.m_uiElement && pin.m_uiElement.parent == _map.m_pinRootSmall && pin.m_iconElement)
                {
                    _hidden.Add((pin.m_iconElement, pin.m_iconElement.enabled));
                    pin.m_iconElement.enabled = false; // avoid a clipped duplicate at a circular edge
                }
                if (used == _icons.Count) _icons.Add(CreateIcon());
                var icon = _icons[used++];
                icon.gameObject.SetActive(true);
                icon.rectTransform.localScale = Vector3.one * (.95f + .05f * Mathf.Sin(Time.time * 5));
                icon.rectTransform.localRotation = Quaternion.Euler(0, 0, (float)TargetMath.ArrowAngle(point.x, point.y));
                icon.rectTransform.position = image.rectTransform.TransformPoint(new Vector3(
                    area.center.x + (float)edge.X, area.center.y + (float)edge.Y, 0));
            }
        }
        // Expired/nearby pings disappear immediately, including on the large map or hidden HUD.
        for (int i = used; i < _icons.Count; i++) _icons[i].gameObject.SetActive(false);
    }

    private TargetGraphic CreateIcon()
    {
        var go = new GameObject("BetterMinimapEdgePing", typeof(RectTransform));
        go.transform.SetParent(_parent, false);
        var arrow = go.AddComponent<TargetGraphic>();
        arrow.raycastTarget = false; arrow.maskable = false;
        arrow.rectTransform.sizeDelta = new Vector2(18, 18);
        arrow.SetArrow(true);
        // The native ping ring's blue, rather than the personal waypoint's gold.
        arrow.SetTint(new Color32(112, 160, 255, 255));
        return arrow;
    }

    public void Dispose()
    {
        BeforeMap();
        foreach (var image in _icons)
            if (image) { image.gameObject.SetActive(false); UnityEngine.Object.Destroy(image.gameObject); }
        _icons.Clear();
    }
}
