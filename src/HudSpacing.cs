using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Only the small biome label and the status-effect group are moved; no effect data changes.
internal sealed class HudSpacing : IDisposable
{
    private readonly Minimap _map;
    private readonly MapScaleView _scale;
    private readonly TMP_Text? _biome;
    private RectTransform? _buffs;
    private Vector2 _buffBase, _buffLast;
    private bool _buffShifted, _raised;
    private Vector2 _anchorsMin, _anchorsMax, _pivot, _size;
    private Vector3 _position;
    private TextAlignmentOptions _alignment;
    private Vector3 _windOriginal;
    private bool _windMoved, _below;
    private readonly MaskableGraphic? _windGraphic;
    private readonly bool _windMaskable;
    private bool _windUnmasked;
    private readonly HudContrast _contrast = new HudContrast();
    private bool _outlineApplied;
    private float _bottomWindOffset;
    public float ZoomIndicatorOffset => Mathf.Max(_bottomWindOffset,
        _below && _biome ? _biome!.rectTransform.rect.height + 6 : 0);

    public HudSpacing(Minimap map, MapScaleView scale)
    {
        _map = map; _scale = scale;
        _biome = AccessTools.Field(typeof(Minimap), "m_biomeNameSmall")?.GetValue(map) as TMP_Text;
        _windGraphic = map.m_windMarker.GetComponent<MaskableGraphic>();
        _windMaskable = _windGraphic && _windGraphic!.maskable;
    }

    public void Apply(int percent, BiomePosition biomePosition, WindPosition windPosition, bool round)
    {
        bool small = _map && _map.m_mode == Minimap.MapMode.Small &&
            _map.m_mapImageSmall && _map.m_mapImageSmall.gameObject.activeInHierarchy;
        PlaceBiome(small ? biomePosition : BiomePosition.Vanilla);
        PlaceWind(small ? windPosition : WindPosition.Vanilla);
        SetWindUnmasked(small && (windPosition != WindPosition.Vanilla || round));
        if (!_outlineApplied && _map)
        {
            _contrast.Apply(_biome, true);
            _contrast.Apply(_map!.m_windMarker, true);
            _outlineApplied = true;
        }
        var hud = Hud.instance;
        var buffs = hud ? hud.m_statusEffectListRoot : null;
        if (_buffs != buffs)
        {
            RestoreBuffs();
            _buffs = buffs;
        }
        if (!small || percent <= 100 || !_buffs || !_buffs!.parent)
        {
            RestoreBuffs();
            return;
        }
        Vector2 current = _buffs.anchoredPosition;
        // Do not accumulate our own offset. Respect a fresh position from another HUD mod.
        if (!_buffShifted || !Mathf.Approximately(current.x, _buffLast.x)) _buffBase.x = current.x;
        _buffBase.y = current.y;
        float extra = _buffs.parent.InverseTransformVector(_scale.LeftExpansionWorld(percent)).x;
        _buffLast = _buffBase - new Vector2(Mathf.Max(0, extra), 0);
        _buffs.anchoredPosition = _buffLast;
        _buffShifted = true;
    }

    private void PlaceBiome(BiomePosition position)
    {
        if (!_biome) return;
        var label = _biome!.rectTransform;
        _below = position == BiomePosition.Below;
        if (position == BiomePosition.Vanilla)
        {
            if (!_raised) return;
            label.anchorMin = _anchorsMin; label.anchorMax = _anchorsMax;
            label.pivot = _pivot; label.sizeDelta = _size;
            label.anchoredPosition3D = _position;
            _biome.alignment = _alignment;
            _raised = false;
            return;
        }
        if (!_raised)
        {
            _anchorsMin = label.anchorMin; _anchorsMax = label.anchorMax;
            _pivot = label.pivot; _size = label.sizeDelta;
            _position = label.anchoredPosition3D; _alignment = _biome.alignment;
            _raised = true;
        }
        var mapRect = _map.m_mapImageSmall.rectTransform;
        Rect area = mapRect.rect;
        label.anchorMin = label.anchorMax = new Vector2(.5f, 1);
        label.pivot = new Vector2(.5f, _below ? 1 : 0);
        label.sizeDelta = new Vector2(area.width, _size.y);
        label.position = mapRect.TransformPoint(new Vector3(area.center.x, _below ? area.yMin - 6 : area.yMax + 6, 0));
        _biome.alignment = TextAlignmentOptions.Center;
    }

    private void PlaceWind(WindPosition position)
    {
        _bottomWindOffset = 0;
        if (!_map || !_map.m_windMarker) return;
        var wind = _map.m_windMarker;
        if (position == WindPosition.Vanilla)
        {
            if (_windMoved) wind.anchoredPosition3D = _windOriginal;
            _windMoved = false;
            return;
        }
        if (!_windMoved) { _windOriginal = wind.anchoredPosition3D; _windMoved = true; }
        var image = _map.m_mapImageSmall.rectTransform;
        var rect = image.rect;
        // Keep the entire rotating arrow outside the map, including its frame.
        // The half-diagonal reserves enough space for every wind direction.
        float radius = wind.rect.size.magnitude / 2;
        float paddingX = wind.rect.width / 2 + 2, paddingY = radius + 2;
        bool left = position == WindPosition.TopLeft || position == WindPosition.BottomLeft;
        bool centered = position == WindPosition.Top || position == WindPosition.Bottom;
        bool top = position == WindPosition.TopLeft || position == WindPosition.TopRight || position == WindPosition.Top;
        // If both elements occupy the center, place the arrow beyond the biome label.
        if (centered && _raised && _biome && top != _below)
            paddingY += _biome!.rectTransform.rect.height + 6;
        if (position == WindPosition.Bottom) _bottomWindOffset = paddingY + radius;
        wind.position = image.TransformPoint(new Vector3(centered ? rect.center.x : left ? rect.xMin + paddingX : rect.xMax - paddingX,
            top ? rect.yMax + paddingY : rect.yMin - paddingY, 0));
    }

    private void RestoreBuffs()
    {
        if (_buffShifted && _buffs)
        {
            var current = _buffs!.anchoredPosition;
            // Avoid undoing an unrelated external repositioning.
            if (Mathf.Approximately(current.x, _buffLast.x))
                _buffs.anchoredPosition = new Vector2(_buffBase.x, current.y);
        }
        _buffShifted = false;
    }

    private void SetWindUnmasked(bool outside)
    {
        if (!_windGraphic || _windUnmasked == outside) return;
        _windUnmasked = outside;
        _windGraphic!.maskable = outside ? false : _windMaskable;
        // RecalculateMasking handles stencil masks, but not RectMask2D's CPU clipping.
        // Remove that registration too, including a previously culled renderer.
        _windGraphic.RecalculateClipping();
        _windGraphic.RecalculateMasking();
        if (outside)
        {
            _windGraphic.canvasRenderer.DisableRectClipping();
            _windGraphic.canvasRenderer.cull = false;
        }
    }

    public void Dispose()
    {
        _contrast.Dispose();
        _outlineApplied = false;
        RestoreBuffs();
        PlaceBiome(BiomePosition.Vanilla);
        PlaceWind(WindPosition.Vanilla);
        SetWindUnmasked(false);
        _buffs = null;
    }
}
