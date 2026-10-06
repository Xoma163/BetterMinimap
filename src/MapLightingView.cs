using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

internal sealed class MapLightingView : IDisposable
{
    private readonly RawImage _image;
    private Material? _source, _material;

    public MapLightingView(Minimap map) { _image = map.m_mapImageSmall; }

    public void Apply(MapLighting mode, float dayFraction)
    {
        if (!_image) return;
        if (!_material || _image.material != _material)
        {
            Dispose();
            _source = _image.material;
            if (!_source || !_source.HasProperty("_lightColor") || !_source.HasProperty("_ambientLightColor")) return;
            _material = new Material(_source) { name = "BetterMinimapLighting" };
            _image.material = _material;
        }
        // Vanilla updates its own material (zoom, exploration etc.). Keep those updates,
        // changing only this private minimap copy. Never modify global shader lighting.
        _material!.CopyPropertiesFromMaterial(_source!);
        float daylight = mode == MapLighting.Day ? 1 : mode == MapLighting.Night ? 0 :
            (float)MapMath.Daylight(dayFraction);
        var daylightColor = _source!.GetColor("_lightColor");
        var ambient = _source.GetColor("_ambientLightColor");
        _material.SetColor("_lightColor", daylightColor * Mathf.Lerp(.25f, 1, daylight));
        _material.SetColor("_ambientLightColor", Color.Lerp(
            new Color(ambient.r * .38f, ambient.g * .45f, ambient.b * .62f, ambient.a), ambient, daylight));
    }

    public void Dispose()
    {
        if (_image && _material && _image.material == _material) _image.material = _source;
        if (_material) UnityEngine.Object.Destroy(_material);
        _material = null; _source = null;
    }
}
