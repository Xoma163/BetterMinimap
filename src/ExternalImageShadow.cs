using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Separate UI sprites behind the original. No mesh effect touches its white face.
public sealed class ExternalImageShadow : MonoBehaviour
{
    private Image? _source;
    private readonly Image[] _copies = new Image[8];
    private static readonly Vector2[] Offsets = {
        new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1),
        new Vector2(-.7071f, -.7071f), new Vector2(-.7071f, .7071f),
        new Vector2(.7071f, -.7071f), new Vector2(.7071f, .7071f)
    };

    public void Initialize(Image source)
    {
        _source = source;
        for (int i = 0; i < _copies.Length; i++)
        {
            var go = new GameObject("BetterMinimapWindShadow", typeof(RectTransform));
            // Set maskable before OnEnable can register this Image with RectMask2D.
            go.SetActive(false);
            go.transform.SetParent(source.transform.parent, false);
            go.transform.SetSiblingIndex(source.transform.GetSiblingIndex());
            var image = go.AddComponent<Image>();
            image.raycastTarget = false;
            image.maskable = false;
            _copies[i] = image;
            go.SetActive(true);
            // maskable's setter only dirties stencil state, not RectMask2D registration.
            image.RecalculateClipping();
            image.RecalculateMasking();
            image.canvasRenderer.DisableRectClipping();
            image.canvasRenderer.cull = false;
        }
        LateUpdate();
    }

    private void LateUpdate()
    {
        if (!_source) return;
        var source = _source!;
        var rect = source.rectTransform;
        for (int i = 0; i < _copies.Length; i++)
        {
            var copy = _copies[i];
            if (!copy) continue;
            copy.enabled = source.enabled && source.gameObject.activeInHierarchy;
            copy.sprite = source.overrideSprite ? source.overrideSprite : source.sprite;
            copy.type = source.type;
            copy.preserveAspect = source.preserveAspect;
            copy.fillAmount = source.fillAmount;
            copy.fillMethod = source.fillMethod;
            copy.fillOrigin = source.fillOrigin;
            copy.fillClockwise = source.fillClockwise;
            copy.color = new Color(0, 0, 0, source.color.a * .85f);
            var target = copy.rectTransform;
            target.anchorMin = rect.anchorMin; target.anchorMax = rect.anchorMax;
            target.pivot = rect.pivot; target.sizeDelta = rect.sizeDelta;
            target.anchoredPosition3D = rect.anchoredPosition3D + (Vector3)(Offsets[i] * 1.25f);
            target.localScale = rect.localScale; target.localRotation = rect.localRotation;
        }
    }

    private void OnDisable()
    {
        foreach (var copy in _copies) if (copy) copy.enabled = false;
    }

    public void Clear()
    {
        enabled = false;
        _source = null;
        foreach (var copy in _copies)
            if (copy) { copy.enabled = false; Destroy(copy.gameObject); }
    }

    private void OnDestroy() => Clear();
}
