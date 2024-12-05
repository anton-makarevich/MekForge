namespace Sanet.MekForge.Core.Models.Units.Components.Weapons;

public class Ammo : Component
{
    private const int ShotsPerTon = 8;
    private int _remainingShots;

    public Ammo(AmmoType type) : base($"{type} Ammo", new[] { 0 })
    {
        Type = type;
        _remainingShots = ShotsPerTon;
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
}
