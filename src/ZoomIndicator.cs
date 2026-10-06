using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Shtab.RotatingMinimap;

internal sealed class ZoomIndicator : IDisposable
{
    private readonly Minimap _map;
    private readonly Transform _hudRoot;
    private TextMeshProUGUI? _label;
    private readonly HudContrast _contrast = new HudContrast();
    private float _previous, _until;
    private bool _initialized;
    public ZoomIndicator(Minimap map)
    {
        _map = map;
        _hudRoot = map.m_mapImageSmall.transform.parent;
    }

    public void Update(float baseline, float current, bool ship, bool changedContext, float extraOffset)
    {
        bool changed = (_initialized && !Mathf.Approximately(current, _previous)) || changedContext;
        _initialized = true; _previous = current;
        if (changed && current > 0 && baseline > 0)
        {
            EnsureLabel();
            float factor = baseline / current;
            _label!.text = (ship ? UiLanguage.Pick("Зум корабля ", "Ship Zoom ") : "") + "×" +
                factor.ToString(factor < .1f ? "0.00" : "0.0", CultureInfo.InvariantCulture);
            _until = Time.unscaledTime + 2f;
        }
        if (!_label) return;
        bool visible = _map.m_mode == Minimap.MapMode.Small &&
            _map.m_mapImageSmall.gameObject.activeInHierarchy && Time.unscaledTime < _until;
        _label!.gameObject.SetActive(visible);
        var mapRect = _map.m_mapImageSmall.rectTransform;
        Rect area = mapRect.rect;
        _label.rectTransform.position = mapRect.TransformPoint(new Vector3(area.center.x, area.yMin - 6 - extraOffset, 0));
    }

    private void EnsureLabel()
    {
        if (_label) return;
        var reference = _map.GetComponentInChildren<TMP_Text>(true);
        var go = new GameObject("BetterMinimapZoomIndicator", typeof(RectTransform));
        // Outside the circular mask, but inside the scaled small HUD block.
        go.transform.SetParent(_hudRoot, false);
        _label = go.AddComponent<TextMeshProUGUI>();
        if (reference && reference.font) _label.font = reference.font;
        _label.fontSize = 18; _label.alignment = TextAlignmentOptions.Center;
        _label.color = Color.white; _label.raycastTarget = false; _label.maskable = false;
        _label.rectTransform.pivot = new Vector2(.5f, 1);
        _label.rectTransform.sizeDelta = new Vector2(200, 26);
        _contrast.Apply(_label);
    }

    public void Dispose()
    {
        _contrast.Dispose();
        if (_label) UnityEngine.Object.Destroy(_label!.gameObject);
        _label = null;
    }
}
