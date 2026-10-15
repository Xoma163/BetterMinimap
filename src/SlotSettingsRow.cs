using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Reuses StoreAndCraft's own widgets/listeners: only their layout changes, on our page.
public sealed class SlotSettingsRow : MonoBehaviour
{
    private RectTransform? _label, _choice, _sizeLabel, _sizeWidget;
    private LayoutElement? _layout;

    internal void Initialize(RectTransform label, RectTransform choice, RectTransform sizeLabel, RectTransform sizeWidget)
    {
        _label = label; _choice = choice; _sizeLabel = sizeLabel; _sizeWidget = sizeWidget;
        _layout = GetComponent<LayoutElement>();
        _sizeLabel.SetParent(transform, false);
        _sizeWidget.SetParent(transform, false);
        _sizeLabel.name = "SizeLabel"; _sizeWidget.name = "SizeWidget";
        // Native numeric widgets reserve 110px for the input. Three digits need less,
        // leaving room for the slider alongside the content selector.
        var field = _sizeWidget.Find("Field") as RectTransform;
        var slider = _sizeWidget.Find("Slider") as RectTransform;
        if (field && slider)
        {
            field!.offsetMin = new Vector2(-64, 2);
            field.offsetMax = new Vector2(0, -2);
            slider!.offsetMax = new Vector2(-76, -6);
        }
        Arrange();
    }

    private void OnRectTransformDimensionsChange() => Arrange();

    private void Arrange()
    {
        if (!_label || !_choice || !_sizeLabel || !_sizeWidget || !_layout) return;
        // Compact one-row layout at normal widths; two compact lines on narrow windows
        // instead of overlapping controls. No verbose per-slot description.
        float width = ((RectTransform)transform).rect.width;
        bool wide = SlotRowLayout.SingleLine(width);
        float height = SlotRowLayout.Height(width);
        if (_layout!.minHeight != height) _layout.minHeight = height;
        if (_layout.preferredHeight != height) _layout.preferredHeight = height;
        if (wide)
        {
            Place(_label!, 0, .1f, 0, 1);
            Place(_choice!, .1f, .37f, 0, 1);
            Place(_sizeLabel!, .39f, .65f, 0, 1);
            Place(_sizeWidget!, .65f, 1, 0, 1);
        }
        else
        {
            Place(_label!, 0, .3f, .5f, 1);
            Place(_choice!, .3f, 1, .5f, 1);
            Place(_sizeLabel!, 0, .48f, 0, .5f);
            Place(_sizeWidget!, .48f, 1, 0, .5f);
        }
    }

    private static void Place(RectTransform rect, float left, float right, float bottom, float top)
    {
        rect.anchorMin = new Vector2(left, bottom); rect.anchorMax = new Vector2(right, top);
        rect.offsetMin = new Vector2(4, 4); rect.offsetMax = new Vector2(-4, -4);
    }
}
