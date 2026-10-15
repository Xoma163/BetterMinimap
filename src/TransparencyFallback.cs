using System;

namespace Shtab.RotatingMinimap;

// Isolate this optional renderer from the plugin's global fail/disable path.
// Delegates are captured once, not allocated for each frame. Also testable without Unity.
internal sealed class TransparencyFallback
{
    private readonly Action<int, float, bool> _apply;
    private readonly Action _restore;
    private readonly Action<Exception> _warn;
    private int _lastTransparency = -1;
    private bool _blocked;

    public TransparencyFallback(Action<int, float, bool> apply, Action restore, Action<Exception> warn)
    {
        _apply = apply; _restore = restore; _warn = warn;
    }

    public void Apply(int transparency, float heading, bool visible)
    {
        // An explicit change to 0 rearms the feature; merely reopening M or changing
        // between nonzero values must not retry a failing GPU allocation every frame.
        if (transparency == 0 && _lastTransparency != 0) _blocked = false;
        _lastTransparency = transparency;
        if (_blocked) return;
        try { _apply(transparency, heading, visible); }
        catch (Exception error)
        {
            _blocked = true;
            try { _restore(); }
            catch (Exception cleanupError) { error = new AggregateException(error, cleanupError); }
            // A logger failure must not defeat the isolation of this optional feature.
            try { _warn(error); }
            catch { }
        }
    }
}
