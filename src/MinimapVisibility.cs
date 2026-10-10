using System;
using UnityEngine;

namespace Shtab.RotatingMinimap;

// Hide just the small HUD subtree, never change MapMode (M must keep working).
// A CanvasGroup avoids deactivating/re-activating the entire UI hierarchy every frame.
internal sealed class MinimapVisibility : IDisposable
{
    private readonly GameObject _root;
    private CanvasGroup? _group;
    private bool _owned, _hidden, _interactable, _blocksRaycasts;
    private float _alpha;

    public MinimapVisibility(Minimap map) { _root = map.m_smallRoot; }

    public void Apply(bool visible)
    {
        if (visible) { Restore(); return; }
        if (!_root) return;
        if (!_group)
        {
            _group = _root.GetComponent<CanvasGroup>();
            _owned = !_group;
            if (_owned) _group = _root.AddComponent<CanvasGroup>();
        }
        if (!_hidden)
        {
            _alpha = _group!.alpha;
            _interactable = _group.interactable;
            _blocksRaycasts = _group.blocksRaycasts;
            _hidden = true;
        }
        _group!.alpha = 0;
        _group.interactable = false;
        _group.blocksRaycasts = false;
    }

    private void Restore()
    {
        if (!_hidden) return;
        if (_group)
        {
            _group!.alpha = _alpha;
            _group.interactable = _interactable;
            _group.blocksRaycasts = _blocksRaycasts;
        }
        _hidden = false;
    }

    public void Dispose()
    {
        Restore();
        if (_owned && _group) UnityEngine.Object.Destroy(_group);
        _group = null; _owned = false;
    }
}
