using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Live, client-loaded vehicles only. No prefab-name whitelist, saved pins, ZDO writes or RPCs.
internal sealed class TransportView : IDisposable
{
    private static readonly FieldInfo Carts = AccessTools.Field(typeof(Vagon), "m_instances")
        ?? throw new MissingFieldException("Vagon.m_instances");
    private readonly Minimap _map;
    private readonly List<TransportGraphic> _icons = new List<TransportGraphic>();
    private readonly List<(Graphic Graphic, bool Enabled)> _native = new List<(Graphic, bool)>();
    private readonly Graphic[] _smallShip, _largeShip;
    private int _iconSize = TransportLayout.DefaultIconSize;

    public TransportView(Minimap map)
    {
        _map = map;
        _smallShip = map.m_smallShipMarker.GetComponentsInChildren<Graphic>(true);
        _largeShip = map.m_largeShipMarker.GetComponentsInChildren<Graphic>(true);
    }

    public void BeforeMap()
    {
        foreach (var saved in _native) if (saved.Graphic) saved.Graphic.enabled = saved.Enabled;
        _native.Clear();
    }

    public void Update(bool enabled, bool showSmall, int iconSize, float heading, bool round)
    {
        _iconSize = TransportLayout.ClampSize(iconSize);
        BeforeMap();
        // Replace the native controlled-ship marker too: no duplicate, and Off hides all vehicle icons.
        HideNative(_smallShip);
        HideNative(_largeShip);
        int used = 0;
        bool large = _map.m_mode == Minimap.MapMode.Large;
        bool visible = enabled && Player.m_localPlayer &&
            (large || (_map.m_mode == Minimap.MapMode.Small && showSmall));
        if (visible)
        {
            foreach (var instance in Ship.Instances)
                if (instance is Ship ship) Show(ship, false, large, heading, round, ref used);
            if (Carts.GetValue(null) is List<Vagon> carts)
                foreach (var cart in carts) Show(cart, true, large, heading, round, ref used);
        }
        for (int i = used; i < _icons.Count; i++) _icons[i].gameObject.SetActive(false);
    }

    private void HideNative(Graphic[] graphics)
    {
        foreach (var graphic in graphics)
        {
            if (!graphic) continue;
            _native.Add((graphic, graphic.enabled));
            graphic.enabled = false;
        }
    }

    private void Show(MonoBehaviour vehicle, bool cart, bool large, float heading, bool round, ref int used)
    {
        if (!vehicle || !vehicle.isActiveAndEnabled) return;
        var view = vehicle.GetComponent<ZNetView>();
        // Exclude placement previews, destroyed objects and unloaded network instances.
        if (!view || !view.IsValid()) return;
        var image = large ? _map.m_mapImageLarge : _map.m_mapImageSmall;
        if (!image.gameObject.activeInHierarchy) return;
        var area = image.rectTransform.rect;
        if (!MinimapProjection.TryProject(_map, image, vehicle.transform.position, large ? 0 : heading, out var point) ||
            !TransportLayout.Visible(point.x, point.y, area.width, area.height, !large && round, _iconSize)) return;
        var parent = large ? _map.m_pinRootLarge : _map.m_pinRootSmall;
        if (used == _icons.Count)
        {
            var go = new GameObject("BetterMinimapTransport", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var graphic = go.AddComponent<TransportGraphic>();
            graphic.raycastTarget = false;
            graphic.maskable = true;
            graphic.rectTransform.sizeDelta = new Vector2(18, 18);
            _icons.Add(graphic);
        }
        var icon = _icons[used++];
        if (icon.transform.parent != parent) icon.transform.SetParent(parent, false);
        icon.rectTransform.localScale = Vector3.one * TransportLayout.Scale(_iconSize);
        icon.SetCart(cart);
        icon.rectTransform.position = image.rectTransform.TransformPoint(
            new Vector3(area.center.x + point.x, area.center.y + point.y, 0));
        icon.rectTransform.localRotation = Quaternion.identity;
        icon.gameObject.SetActive(true);
    }

    public void Dispose()
    {
        BeforeMap();
        foreach (var icon in _icons)
            if (icon) { icon.gameObject.SetActive(false); UnityEngine.Object.Destroy(icon.gameObject); }
        _icons.Clear();
    }
}

// Original UI silhouettes: cart with two wheels / boat with mast and sail.
public sealed class TransportGraphic : MaskableGraphic
{
    private bool _cart;
    public void SetCart(bool cart) { if (_cart == cart) return; _cart = cart; SetVerticesDirty(); }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        // Offset copies provide an outline without requiring a texture or a game sprite.
        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
            if (x != 0 || y != 0) Draw(vh, new Vector2(x, y), new Color32(25, 20, 15, 255));
        Draw(vh, Vector2.zero, new Color32(240, 224, 180, 255));
    }

    private void Draw(VertexHelper vh, Vector2 offset, Color32 tint)
    {
        if (_cart)
        {
            Quad(vh, offset, tint, -6, -2, 5, 4);
            Quad(vh, offset, tint, 4, 3, 8, 5);
            Wheel(vh, offset + new Vector2(-4, -5), tint);
            Wheel(vh, offset + new Vector2(4, -5), tint);
        }
        else
        {
            Polygon(vh, offset, tint, new Vector2(-8, -2), new Vector2(8, -2),
                new Vector2(4, -6), new Vector2(-4, -6));
            Quad(vh, offset, tint, -1, -2, 1, 8);
            Polygon(vh, offset, tint, new Vector2(2, 7), new Vector2(7, 0), new Vector2(2, 0));
        }
    }

    private static void Quad(VertexHelper vh, Vector2 offset, Color32 tint, float left, float bottom, float right, float top) =>
        Polygon(vh, offset, tint, new Vector2(left, bottom), new Vector2(left, top),
            new Vector2(right, top), new Vector2(right, bottom));

    private static void Wheel(VertexHelper vh, Vector2 center, Color32 tint)
    {
        var points = new Vector2[8];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = i * Mathf.PI / 4;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2;
        }
        Polygon(vh, center, tint, points);
    }

    private static void Polygon(VertexHelper vh, Vector2 offset, Color32 tint, params Vector2[] points)
    {
        int start = vh.currentVertCount;
        foreach (var point in points) vh.AddVert((Vector3)(offset + point), tint, Vector2.zero);
        for (int i = 1; i < points.Length - 1; i++) vh.AddTriangle(start, start + i, start + i + 1);
    }
}
