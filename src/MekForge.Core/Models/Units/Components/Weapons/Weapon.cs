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
        AmmoType ammoType = AmmoType.None) 
        : base(name, [])
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
    }

    public int Damage { get; }
    public int Heat { get; }
    public int MinimumRange { get; }
    public int ShortRange { get; }
    public int MediumRange { get; }
    public int LongRange { get; }
    public WeaponType Type { get; }
    public int BattleValue { get; }
    public AmmoType AmmoType { get; }
}
