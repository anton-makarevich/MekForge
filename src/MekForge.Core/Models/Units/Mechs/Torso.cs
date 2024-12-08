﻿namespace Sanet.MekForge.Core.Models.Units.Mechs;

public abstract class Torso : UnitPart
{
    public int MaxRearArmor { get; }
    public int CurrentRearArmor { get; private set; }

    protected Torso(string name, PartLocation location, int maxArmor, int maxRearArmor, int maxStructure) 
        : base(name, location, maxArmor, maxStructure, 12)
    {
        MaxRearArmor = maxRearArmor;
        CurrentRearArmor = maxRearArmor;
    }

    protected override int ReduceArmor(int damage, HitDirection direction)
    {
        if (direction != HitDirection.Rear) return base.ReduceArmor(damage, direction);
        // First reduce rear armor
        var remainingDamage = damage;
        if (CurrentRearArmor <= 0) return damage;
        if (CurrentRearArmor >= remainingDamage)
        {
            CurrentRearArmor -= remainingDamage;
            return 0;
        }
        remainingDamage -= CurrentRearArmor;
        CurrentRearArmor = 0;
        return remainingDamage;
    }
}