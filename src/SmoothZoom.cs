using System;

namespace Shtab.RotatingMinimap;

// Target and rendered scale are separate so rapid input never steps from an animation frame.
internal sealed class SmoothZoom
{
    public const double Duration = .2;
    public double Target { get; private set; }
    private double _start, _elapsed;

    public void Reset(double value) { Target = _start = value; _elapsed = Duration; }

    public void Request(double current, double target)
    {
        if (target <= 0 || double.IsNaN(target) || double.IsInfinity(target)) return;
        if (Target == target) return;
        _start = current;
        Target = target;
        _elapsed = 0;
    }

    public double Advance(double delta, bool animate)
    {
        if (!animate) _elapsed = Duration;
        else _elapsed = Math.Min(Duration, _elapsed + Math.Max(0, delta));
        if (_elapsed >= Duration || _start <= 0) return Target;
        double t = _elapsed / Duration;
        t = t * t * (3 - 2 * t);
        // Interpolate logarithmic scale: zooming in/out has the same perceived speed.
        return Math.Exp(Math.Log(_start) + (Math.Log(Target) - Math.Log(_start)) * t);
    }
}
