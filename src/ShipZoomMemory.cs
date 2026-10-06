namespace Shtab.RotatingMinimap;

// Two magnification values shared by all ships; independent of a world's base UV scale.
internal sealed class ShipZoomMemory
{
    private bool _enabled, _aboard;
    private double _land, _ship;
    public bool HasValues { get; private set; }
    public double Land => _land;
    public double Ship => _ship;
    public bool ChangedContext { get; private set; }
    public bool ShipMode => _enabled && _aboard;

    public double Update(bool enabled, bool aboard, double current)
    {
        ChangedContext = false;
        if (!enabled)
        {
            double result = _enabled && _aboard ? _land : current;
            ChangedContext = _enabled && _aboard;
            _enabled = false;
            return result;
        }
        if (!_enabled)
        {
            if (!HasValues) ResetValues(current, current);
            _enabled = true;
            _aboard = aboard;
            ChangedContext = aboard;
            return aboard ? _ship : _land;
        }
        if (aboard == _aboard)
        {
            Remember(current);
            return current;
        }
        if (_aboard) _ship = current;
        else _land = current;
        _aboard = aboard;
        ChangedContext = true;
        return aboard ? _ship : _land;
    }

    public void Remember(double current)
    {
        if (!_enabled || !Valid(current)) return;
        if (_aboard) _ship = current;
        else _land = current;
    }

    public void ResetValues(double land, double ship)
    {
        if (!Valid(land) || !Valid(ship)) return;
        _land = land; _ship = ship;
        HasValues = true;
    }

    public void Suspend() { _enabled = false; ChangedContext = false; }
    internal static bool Valid(double value) => value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
}
