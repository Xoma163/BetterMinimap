using TMPro;
using UnityEngine;

namespace Shtab.RotatingMinimap;

// Separate glyph copies work even when the source font shader has no underlay support.
// The original text and its material are never modified.
public sealed class ExternalTextShadow : MonoBehaviour
{
    private TMP_Text? _source;
    public float PixelWidth { get; set; } = 2;
    private bool _respectMask;
    private readonly TextMeshProUGUI[] _copies = new TextMeshProUGUI[8];
    private static readonly Vector2[] Offsets = {
        new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1),
        new Vector2(-.7071f, -.7071f), new Vector2(-.7071f, .7071f),
        new Vector2(.7071f, -.7071f), new Vector2(.7071f, .7071f)
    };

    public void Initialize(TMP_Text source, bool respectMask = false)
    {
        _source = source;
        _respectMask = respectMask;
        for (int i = 0; i < _copies.Length; i++)
        {
            var go = new GameObject("BetterMinimapBiomeOutline", typeof(RectTransform));
            go.SetActive(false);
            go.transform.SetParent(source.transform.parent, false);
            go.transform.SetSiblingIndex(source.transform.GetSiblingIndex());
            var copy = go.AddComponent<TextMeshProUGUI>();
            copy.raycastTarget = false;
            copy.maskable = respectMask && source.maskable;
            copy.overrideColorTags = true;
            _copies[i] = copy;
            go.SetActive(true);
            copy.RecalculateClipping();
            copy.RecalculateMasking();
            if (!respectMask)
            {
                copy.canvasRenderer.DisableRectClipping();
                copy.canvasRenderer.cull = false;
            }
        }
        LateUpdate();
    }

    private void LateUpdate()
    {
        if (!_source) return;
        var source = _source!;
        var rect = source.rectTransform;
        var pixels = ScreenPixelOffsets.InParent(rect);
        // Read once: TMP's numeric SetText backing-string getter is stateful in the shipped version.
        string text = source.text;
        for (int i = 0; i < _copies.Length; i++)
        {
            var copy = _copies[i];
            if (!copy) continue;
            bool visible = source.enabled && source.gameObject.activeInHierarchy;
            copy.gameObject.SetActive(visible);
            if (!visible) continue;
            // Use the font's base material, not the source's possibly unsupported underlay.
            copy.font = source.font;
            if (source.font) copy.fontSharedMaterial = source.font.material;
            copy.text = text;
            copy.richText = source.richText;
            copy.maskable = _respectMask && source.maskable;
            copy.isRightToLeftText = source.isRightToLeftText;
            copy.fontSize = source.fontSize;
            copy.fontStyle = source.fontStyle;
            copy.fontWeight = source.fontWeight;
            copy.enableAutoSizing = source.enableAutoSizing;
            copy.fontSizeMin = source.fontSizeMin;
            copy.fontSizeMax = source.fontSizeMax;
            copy.alignment = source.alignment;
            copy.characterSpacing = source.characterSpacing;
            copy.wordSpacing = source.wordSpacing;
            copy.lineSpacing = source.lineSpacing;
            copy.paragraphSpacing = source.paragraphSpacing;
            copy.margin = source.margin;
            copy.textWrappingMode = source.textWrappingMode;
            copy.overflowMode = source.overflowMode;
            copy.color = new Color(0, 0, 0, source.color.a * .85f);
            var target = copy.rectTransform;
            if (target.parent != rect.parent)
            {
                target.SetParent(rect.parent, false);
                target.SetSiblingIndex(rect.GetSiblingIndex());
            }
            target.anchorMin = rect.anchorMin; target.anchorMax = rect.anchorMax;
            target.pivot = rect.pivot; target.sizeDelta = rect.sizeDelta;
            target.anchoredPosition3D = rect.anchoredPosition3D +
                (Vector3)((pixels.Right * Offsets[i].x + pixels.Up * Offsets[i].y) * PixelWidth);
            target.localScale = rect.localScale; target.localRotation = rect.localRotation;
        }
    }

    private void OnDisable()
    {
        foreach (var copy in _copies) if (copy) copy.gameObject.SetActive(false);
    }

    public void Clear()
    {
        enabled = false;
        _source = null;
        foreach (var copy in _copies)
            if (copy) { copy.gameObject.SetActive(false); Destroy(copy.gameObject); }
    }

    private void OnDestroy() => Clear();
}
