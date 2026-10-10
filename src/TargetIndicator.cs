using System;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Session-only selection. No PinData values, exploration, saves or network state are modified.
internal sealed class TargetIndicator : IDisposable
{
    private static readonly MethodInfo ScreenToWorld = AccessTools.Method(typeof(Minimap), "ScreenToWorldPoint")
        ?? throw new MissingMethodException("Minimap.ScreenToWorldPoint");
    private readonly Minimap _map;
    private readonly Transform _parent;
    private readonly HudContrast _contrast = new HudContrast();
    private Vector3? _target;
    private GameObject? _root;
    private TargetGraphic? _marker;
    private TextMeshProUGUI? _distance;
    private TargetGraphic? _largeMarker;

    public TargetIndicator(Minimap map)
    {
        _map = map;
        _parent = map.m_mapImageSmall.transform.parent;
    }

    public void Select()
    {
        // A release outside the map (for example after dragging) is not a waypoint selection.
        if (!RectTransformUtility.RectangleContainsScreenPoint(_map.m_mapImageLarge.rectTransform, ZInput.pointerPosition)) return;
        _target = (Vector3)ScreenToWorld.Invoke(_map, new object[] { ZInput.pointerPosition });
        var player = Player.m_localPlayer;
        var position = player.transform.position;
        double distance = TargetMath.Distance(position.x, position.z, _target.Value.x, _target.Value.z);
        player.Message(MessageHud.MessageType.Center, SettingsText.TargetSelected(distance, UiLanguage.Russian));
    }

    public void Clear()
    {
        _target = null;
        if (_root) _root!.SetActive(false);
        if (_largeMarker) _largeMarker!.gameObject.SetActive(false);
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, UiLanguage.Pick("Цель снята", "Target cleared"));
    }

    public void Update(bool enabled, float heading, bool round)
    {
        if (!enabled) _target = null;
        UpdateLargeMarker();
        bool visible = _target != null && Player.m_localPlayer && _map.m_mode == Minimap.MapMode.Small &&
            _map.m_mapImageSmall.gameObject.activeInHierarchy;
        if (!visible) { if (_root) _root!.SetActive(false); return; }
        EnsureVisuals();
        _root!.SetActive(true);
        var image = _map.m_mapImageSmall;
        Rect area = image.rectTransform.rect;
        Vector3 pos = _target!.Value;
        if (!MinimapProjection.TryProject(_map, image, pos, heading, out var point))
        { _root.SetActive(false); return; }
        var marker = TargetMath.Clamp(point.x, point.y, area.width, area.height, 14, round);
        _marker!.SetArrow(marker.Outside);
        _marker.rectTransform.position = image.rectTransform.TransformPoint(new Vector3(
            area.center.x + (float)marker.X, area.center.y + (float)marker.Y, 0));
        _marker.rectTransform.localRotation = marker.Outside
            ? Quaternion.Euler(0, 0, (float)TargetMath.ArrowAngle(point.x, point.y))
            : Quaternion.identity;
        // Bring the entire distance label inside the edge, including on circular maps.
        var label = TargetMath.Clamp(marker.X, marker.Y - 23, area.width, area.height, 46, round);
        _distance!.rectTransform.position = image.rectTransform.TransformPoint(new Vector3(
            area.center.x + (float)label.X, area.center.y + (float)label.Y, 0));
        var player = Player.m_localPlayer!.transform.position;
        float meters = (float)TargetMath.Distance(player.x, player.z, pos.x, pos.z);
        _distance.SetText(UiLanguage.Pick("{0:0} м", "{0:0} m"), Mathf.Round(meters));
    }

    private void UpdateLargeMarker()
    {
        bool visible = _target.HasValue && Player.m_localPlayer && _map.m_mode == Minimap.MapMode.Large &&
            _map.m_mapImageLarge.gameObject.activeInHierarchy;
        Vector2 point = Vector2.zero;
        var image = _map.m_mapImageLarge;
        var area = image.rectTransform.rect;
        if (visible) visible = MinimapProjection.TryProject(_map, image, _target!.Value, 0, out point) &&
            !TargetMath.Clamp(point.x, point.y, area.width, area.height, 10, false).Outside;
        if (visible && !_largeMarker)
        {
            var go = new GameObject("BetterMinimapWaypoint", typeof(RectTransform));
            go.transform.SetParent(image.transform, false);
            _largeMarker = go.AddComponent<TargetGraphic>();
            _largeMarker.raycastTarget = false;
            _largeMarker.rectTransform.sizeDelta = new Vector2(18, 18);
        }
        if (!_largeMarker) return;
        _largeMarker!.gameObject.SetActive(visible);
        if (visible) _largeMarker.rectTransform.position = image.rectTransform.TransformPoint(
            new Vector3(area.center.x + point.x, area.center.y + point.y, 0));
    }

    private void EnsureVisuals()
    {
        if (_root) return;
        _root = new GameObject("BetterMinimapTarget", typeof(RectTransform));
        _root.transform.SetParent(_parent, false);
        var arrow = new GameObject("TargetMarker", typeof(RectTransform));
        arrow.transform.SetParent(_root.transform, false);
        _marker = arrow.AddComponent<TargetGraphic>();
        _marker.raycastTarget = false;
        _marker.maskable = false;
        _marker.rectTransform.sizeDelta = new Vector2(18, 18);
        var label = new GameObject("Distance", typeof(RectTransform));
        label.transform.SetParent(_root.transform, false);
        _distance = label.AddComponent<TextMeshProUGUI>();
        var reference = _map.m_biomeNameSmall;
        if (reference && reference.font) _distance.font = reference.font;
        _distance.fontSize = 16;
        _distance.color = new Color(1, .85f, .25f);
        _distance.alignment = TextAlignmentOptions.Center;
        _distance.textWrappingMode = TextWrappingModes.NoWrap;
        _distance.raycastTarget = false;
        _distance.maskable = false;
        _distance.rectTransform.sizeDelta = new Vector2(88, 22);
        _contrast.Apply(_distance, true);
    }

    public void Dispose()
    {
        _target = null;
        _contrast.Dispose();
        if (_root) { _root!.SetActive(false); UnityEngine.Object.Destroy(_root); }
        if (_largeMarker) { _largeMarker!.gameObject.SetActive(false); UnityEngine.Object.Destroy(_largeMarker.gameObject); }
        _largeMarker = null;
        _root = null; _marker = null; _distance = null;
    }
}

public sealed class TargetGraphic : MaskableGraphic
{
    private bool _arrow;
    private Color32 _fill = new Color32(255, 217, 64, 255);
    public void SetTint(Color32 tint)
    {
        _fill = tint; SetVerticesDirty();
    }
    public void SetArrow(bool arrow)
    {
        if (_arrow == arrow) return;
        _arrow = arrow; SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        AddShape(vh, 9, new Color32(0, 0, 0, 230));
        AddShape(vh, 6, _fill);
    }
    private void AddShape(VertexHelper vh, float size, Color32 color)
    {
        int start = vh.currentVertCount;
        vh.AddVert(new Vector3(0, size), color, Vector2.zero);
        vh.AddVert(new Vector3(-size, -size), color, Vector2.zero);
        vh.AddVert(new Vector3(size, -size), color, Vector2.zero);
        if (_arrow) vh.AddTriangle(start, start + 1, start + 2);
        else
        {
            // A diamond marks an on-map goal without suggesting an arbitrary heading.
            vh.SetUIVertex(Vertex(new Vector3(-size, 0), color), start + 1);
            vh.SetUIVertex(Vertex(new Vector3(0, -size), color), start + 2);
            vh.AddVert(new Vector3(size, 0), color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }
    private static UIVertex Vertex(Vector3 position, Color32 color)
    {
        var vertex = UIVertex.simpleVert;
        vertex.position = position; vertex.color = color;
        return vertex;
    }
}

[HarmonyPatch(typeof(Minimap), "OnMapLeftUp")]
internal static class TargetSelectionPatch
{
    private static bool Prefix(Minimap __instance, ref float ___m_leftDownTime, ref float ___m_leftClickTime,
        ref bool ___m_dragView, float ___m_clickDuration)
    {
        var plugin = Plugin.Instance;
        if (plugin == null || !plugin.CanSelectTarget(__instance) ||
            !(ZInput.GetKey(KeyCode.LeftShift) || ZInput.GetKey(KeyCode.RightShift))) return true;
        bool click = ___m_leftDownTime != 0 && Time.time - ___m_leftDownTime < ___m_clickDuration;
        // Consume the whole gesture, not just OnMapLeftClick: vanilla also detects double clicks here.
        ___m_leftDownTime = 0;
        ___m_leftClickTime = float.NegativeInfinity;
        ___m_dragView = false;
        if (click) plugin.SelectTarget();
        return false;
    }
}

[HarmonyPatch(typeof(Minimap), "RemovePinUnderPointer")]
internal static class TargetClearPatch
{
    private static bool Prefix(Minimap __instance, ref float ___m_leftClickTime)
    {
        var plugin = Plugin.Instance;
        if (plugin == null || !plugin.CanSelectTarget(__instance) ||
            !(ZInput.GetKey(KeyCode.LeftShift) || ZInput.GetKey(KeyCode.RightShift))) return true;
        // This is the native right-click handler. Never let a clear gesture delete the pin below it.
        ___m_leftClickTime = float.NegativeInfinity;
        plugin.SelectTarget(clear: true);
        return false;
    }
}
