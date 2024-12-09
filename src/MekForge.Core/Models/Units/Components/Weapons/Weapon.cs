namespace Sanet.MekForge.Core.Models.Units.Components.Weapons;

public abstract class Weapon : Component
{
    protected Weapon(string name,
        int damage,
        int heat,
        int minimumRange,
        int shortRange,
        int mediumRange,
        int longRange,
        WeaponType type,
        int battleValue,
        int size = 1,
        int clusters = 1,
        AmmoType ammoType = AmmoType.None) 
        : base(name, [],size)
    {
        Damage = damage;
        Heat = heat;
        MinimumRange = minimumRange;
        ShortRange = shortRange;
        MediumRange = mediumRange;
        LongRange = longRange;
        Type = type;
        BattleValue = battleValue;
        AmmoType = ammoType;
        Clusters = clusters;
    }

    public int Clusters { get; }

    public int Damage { get; }
    public int Heat { get; }
    public int MinimumRange { get; }
    public int ShortRange { get; }
    public int MediumRange { get; }
    public int LongRange { get; }
    public WeaponType Type { get; }
    public AmmoType AmmoType { get; }
}
