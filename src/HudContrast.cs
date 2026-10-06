using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shtab.RotatingMinimap;

// Add a background shadow, without shrinking the original glyph or sprite face.
internal sealed class HudContrast : IDisposable
{
    private readonly List<Action> _restore = new List<Action>();
    private readonly HashSet<Graphic> _seen = new HashSet<Graphic>();

    public void Apply(Component? root, bool outline = false)
    {
        if (!root) return;
        foreach (var graphic in root!.GetComponentsInChildren<Graphic>(true))
        {
            if (!_seen.Add(graphic)) continue;
            if (graphic is TMP_Text text)
            {
                if (outline)
                {
                    var shadow = text.gameObject.AddComponent<ExternalTextShadow>();
                    shadow.Initialize(text);
                    _restore.Add(() =>
                    {
                        if (!shadow) return;
                        shadow.Clear();
                        UnityEngine.Object.Destroy(shadow);
                    });
                    continue;
                }
                var original = text.fontSharedMaterial;
                if (!original || !original.HasProperty("_UnderlayColor")) continue;
                var outlined = new Material(original) { name = "BetterMinimapTextShadow" };
                outlined.SetColor("_UnderlayColor", new Color(0, 0, 0, .9f));
                outlined.SetFloat("_UnderlayOffsetX", 0);
                outlined.SetFloat("_UnderlayOffsetY", 0);
                outlined.SetFloat("_UnderlayDilate", .25f);
                outlined.SetFloat("_UnderlaySoftness", outline ? .03f : .15f);
                outlined.DisableKeyword("UNDERLAY_INNER");
                outlined.EnableKeyword("UNDERLAY_ON");
                text.fontSharedMaterial = outlined;
                text.UpdateMeshPadding();
                _restore.Add(() =>
                {
                    if (text && text.fontSharedMaterial == outlined)
                    {
                        text.fontSharedMaterial = original;
                        text.UpdateMeshPadding();
                    }
                    if (outlined) UnityEngine.Object.Destroy(outlined);
                });
            }
            else if (graphic is Image image)
            {
                var shadow = image.gameObject.AddComponent<ExternalImageShadow>();
                shadow.Initialize(image);
                _restore.Add(() =>
                {
                    if (!shadow) return;
                    shadow.Clear();
                    UnityEngine.Object.Destroy(shadow);
                });
            }
        }
    }

    public void Dispose()
    {
        for (int i = _restore.Count - 1; i >= 0; i--) _restore[i]();
        _restore.Clear();
        _seen.Clear();
    }
}
