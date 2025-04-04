namespace Sanet.MakaMek.Core.Models.Units.Components.Weapons;

public class Ammo : Component
{
    private int _remainingShots;

    public Ammo(AmmoType type, int initialShots) : base($"{type} Ammo", [])
    {
        Type = type;
        _remainingShots = initialShots;
    }

    public AmmoType Type { get; }

    public int RemainingShots => _remainingShots;

    public bool UseShot()
    {
        if (_remainingShots <= 0)
            return false;

        _remainingShots--;
        return true;
    }

    public override void Hit()
    {
        base.Hit();
        _remainingShots = 0;
    }
}
