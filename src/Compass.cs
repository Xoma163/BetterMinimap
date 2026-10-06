using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

internal sealed class Compass
{
    private readonly GameObject _root;
    private readonly RectTransform[] _lines = new RectTransform[2];
    private readonly DashedLineGraphic[] _lineGraphics = new DashedLineGraphic[2];
    private readonly RectTransform[] _labels = new RectTransform[4];
    private readonly HudContrast _contrast = new HudContrast();

    public Compass(Minimap map)
    {
        var reference = map.GetComponentInChildren<TMP_Text>(true);
        _root = new GameObject("ShtabCompass", typeof(RectTransform));
        var root = (RectTransform)_root.transform;
        root.SetParent(map.m_mapImageSmall.transform, false);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        string[] names = { "N", "E", "S", "W" };
        for (int i = 0; i < 2; i++)
        {
            var go = new GameObject(i == 0 ? "NorthSouth" : "EastWest", typeof(RectTransform));
            go.transform.SetParent(root, false);
            var image = go.AddComponent<DashedLineGraphic>();
            image.raycastTarget = false;
            image.color = new Color(1f, .96f, .85f, .3f);
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            _lines[i] = rect;
            _lineGraphics[i] = image;
        }
        for (int i = 0; i < 4; i++)
        {
            var textGo = new GameObject(names[i], typeof(RectTransform));
            textGo.transform.SetParent(root, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            if (reference && reference.font) text.font = reference.font;
            text.text = names[i];
            text.fontSize = 18;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = i == 0 ? new Color(1f, .87f, .4f) : Color.white;
            text.raycastTarget = false;
            var label = text.rectTransform;
            label.anchorMin = label.anchorMax = label.pivot = new Vector2(.5f, .5f);
            label.sizeDelta = new Vector2(24, 24);
            _labels[i] = label;
            _contrast.Apply(text);
        }
    }

    public void Show(bool visible, Rect area, float heading, bool showLines, bool showLetters, bool round,
        Color lineColor, float thickness, bool dashed)
    {
        if (!_root) return;
        _root.SetActive(visible);
        if (!visible) return;
        for (int i = 0; i < 2; i++)
        {
            _lines[i].gameObject.SetActive(showLines);
            float angle = heading - i * 90f;
            var p = MapMath.CompassEdge(area.width, area.height, angle, 2, round);
            float length = (float)System.Math.Sqrt(p.X * p.X + p.Y * p.Y) * 2;
            _lines[i].sizeDelta = new Vector2(thickness, length);
            _lineGraphics[i].color = lineColor;
            _lineGraphics[i].SetDashed(dashed);
            _lines[i].localRotation = Quaternion.Euler(0, 0, angle);
        }
        for (int i = 0; i < 4; i++)
        {
            _labels[i].gameObject.SetActive(showLetters);
            var p = MapMath.CompassEdge(area.width, area.height, heading - i * 90f, 16, round);
            _labels[i].anchoredPosition = new Vector2((float)p.X, (float)p.Y);
        }
    }

    public void Destroy()
    {
        _contrast.Dispose();
        if (_root) UnityEngine.Object.Destroy(_root);
    }
}
