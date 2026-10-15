using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Valheim's mapshader uses fixed Blend One Zero, so vertex/CanvasGroup alpha cannot fade terrain.
// Render only the terrain with its native shader, then composite it using the standard UI shader.
// The native RawImage keeps its material, UVs, hierarchy and children (including our border).
internal sealed class MapTransparencyView : IDisposable
{
    private readonly RawImage _image;
    private readonly Graphic? _backdrop;
    private readonly Vector3[] _corners = new Vector3[4];
    private readonly TransparencyFallback _fallback;
    private GameObject? _surface;
    private MapTerrainImage? _overlay;
    private Material? _material;
    private RenderTexture? _texture;
    private bool _overridden, _imageEnabled;
    private Color _backdropColor;

    public MapTransparencyView(Minimap map, Action<Exception> warn)
    {
        _image = map.m_mapImageSmall;
        // Capture before the circular mask reparents the map. Valheim's "small" Image is
        // a square translucent backing, not the terrain or any of its marker children.
        _backdrop = _image.transform.parent.GetComponent<Image>();
        _fallback = new TransparencyFallback(ApplyCore, Dispose, warn);
    }

    public void BeforeMap()
    {
        try
        {
            if (_overridden)
            {
                _overridden = false;
                try { if (_image) _image.enabled = _imageEnabled; }
                finally { if (_backdrop) _backdrop!.color = _backdropColor; }
            }
        }
        finally { if (_surface) _surface!.SetActive(false); }
    }

    public void Apply(int transparency, float heading, bool visible) => _fallback.Apply(transparency, heading, visible);

    private void ApplyCore(int transparency, float heading, bool visible)
    {
        BeforeMap();
        float alpha = MapOpacity.Alpha(transparency);
        if (alpha >= 1) { Dispose(); return; }
        if (!visible || !_image || !_image.isActiveAndEnabled) return;
        if (_image.rectTransform.rect.width <= 0 || _image.rectTransform.rect.height <= 0) return;

        if (alpha > 0)
        {
            EnsureSurface();
            RenderTerrain(heading);
            _overlay!.color = new Color(1, 1, 1, alpha);
            _overlay.gameObject.SetActive(true);
        }
        _imageEnabled = _image.enabled;
        if (_backdrop) _backdropColor = _backdrop!.color;
        _overridden = true; // Record rollback before mutating any native state.
        if (_backdrop)
        {
            var tint = _backdropColor;
            tint.a *= alpha;
            _backdrop.color = tint;
        }
        _image.enabled = false; // Only this Graphic; children, masks and markers stay enabled.
    }

    private void EnsureSurface()
    {
        if (!_overlay)
        {
            // Retain ownership even if setup/AddComponent fails partway through.
            _surface = new GameObject("BetterMinimapTerrain", typeof(RectTransform));
            _surface.transform.SetParent(_image.transform, false);
            _surface.transform.SetAsFirstSibling();
            _overlay = _surface.AddComponent<MapTerrainImage>();
            _overlay.raycastTarget = false;
            var rect = _overlay.rectTransform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        _image.rectTransform.GetWorldCorners(_corners);
        var canvas = _image.canvas;
        var camera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(camera, _corners[0]);
        int width = SurfaceSize(Vector2.Distance(bottomLeft, RectTransformUtility.WorldToScreenPoint(camera, _corners[3])));
        int height = SurfaceSize(Vector2.Distance(bottomLeft, RectTransformUtility.WorldToScreenPoint(camera, _corners[1])));
        if (_texture && (_texture!.width != width || _texture.height != height)) ReleaseTexture();
        if (!_texture)
        {
            _texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                name = "BetterMinimapTerrain", filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp, useMipMap = false
            };
        }
        if (!_texture!.IsCreated() && !_texture.Create()) throw new InvalidOperationException("Cannot create minimap render texture");
        _overlay!.texture = _texture;
        if (!_material) _material = new Material(_image.material) { name = "BetterMinimapTerrainPass" };
        _material!.CopyPropertiesFromMaterial(_image.material);
        // Off-screen rendering has no stencil. Keep alpha at the clear value of 1 even if
        // the native shader uses its output alpha for something other than opacity.
        _material.SetInt("_StencilComp", (int)CompareFunction.Always);
        _material.SetInt("_StencilOp", (int)StencilOp.Keep);
        _material.SetInt("_ColorMask", (int)(ColorWriteMask.Red | ColorWriteMask.Green | ColorWriteMask.Blue));
    }

    private static int SurfaceSize(float pixels) => Mathf.Clamp(Mathf.CeilToInt(pixels / 32) * 32, 64, 2048);

    private void RenderTerrain(float heading)
    {
        var previous = RenderTexture.active;
        bool previousSrgb = GL.sRGBWrite;
        GL.PushMatrix();
        try
        {
            RenderTexture.active = _texture;
            GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
            GL.Clear(false, true, Color.black);
            GL.LoadOrtho();
            if (!_material!.SetPass(0)) throw new InvalidOperationException("Cannot render minimap terrain pass");
            GL.Begin(GL.QUADS);
            try
            {
                GL.Color(Color.white);
                Vertex(0, 0, heading); Vertex(0, 1, heading);
                Vertex(1, 1, heading); Vertex(1, 0, heading);
            }
            finally { GL.End(); }
        }
        finally
        {
            GL.PopMatrix();
            RenderTexture.active = previous;
            GL.sRGBWrite = previousSrgb;
        }
    }

    private void Vertex(float x, float y, float heading)
    {
        Rect rect = _image.rectTransform.rect, uv = _image.uvRect;
        var point = MapMath.ScreenToMap((x - .5) * rect.width, (y - .5) * rect.height,
            uv.center.x, uv.center.y, uv.width, uv.height, rect.width, rect.height, heading);
        GL.TexCoord2((float)point.U, (float)point.V);
        GL.Vertex3(x, y, 0);
    }

    private void ReleaseTexture()
    {
        var texture = _texture;
        _texture = null;
        try { if (_overlay) _overlay!.texture = null; }
        finally
        {
            if (texture)
            {
                try { texture!.Release(); }
                finally { UnityEngine.Object.Destroy(texture); }
            }
        }
    }

    public void Dispose()
    {
        // Try every cleanup stage even if an earlier Unity resource operation fails.
        try { BeforeMap(); }
        finally
        {
            try { ReleaseTexture(); }
            finally
            {
                var surface = _surface;
                var material = _material;
                _surface = null; _overlay = null; _material = null;
                try { if (surface) UnityEngine.Object.Destroy(surface); }
                finally { if (material) UnityEngine.Object.Destroy(material); }
            }
        }
    }
}

// Distinct type keeps shape detection from mistaking the terrain copy for a native frame.
public sealed class MapTerrainImage : RawImage { }
