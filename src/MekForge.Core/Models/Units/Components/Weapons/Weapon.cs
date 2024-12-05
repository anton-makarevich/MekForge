namespace Sanet.MekForge.Core.Models.Units.Components.Weapons;

public abstract class Weapon : UnitComponent
{
    protected Weapon(string name, int slots, int damage, int heat, int minimumRange, int shortRange, 
        int mediumRange, int longRange, WeaponType type, int battleValue) 
        : base(name, slots)
    {
        Damage = damage;
        Heat = heat;
        MinimumRange = minimumRange;
        ShortRange = shortRange;
        MediumRange = mediumRange;
        LongRange = longRange;
        Type = type;
        BattleValue = battleValue;
    }

    public int Damage { get; }
    public int Heat { get; }
    public int MinimumRange { get; }
    public int ShortRange { get; }
    public int MediumRange { get; }
    public int LongRange { get; }
    public WeaponType Type { get; }
    public int BattleValue { get; }

    public override void ApplyDamage()
    {
        IsDestroyed = true;
        Deactivate();
    }
}
