using System.Collections.Generic;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Models.Units;

public class UnitPart
{
    public UnitPart(string name, PartLocation location, int maxArmor, int maxStructure)
    {
        Name = name;
        Location = location;
        CurrentArmor = MaxArmor = maxArmor;
        CurrentStructure = MaxStructure = maxStructure;
        Components = new List<UnitComponent>();
    }

    public string Name { get; }
    public PartLocation Location { get; }
    
    // Armor and Structure
    public int MaxArmor { get; }
    public int CurrentArmor { get; private set; }
    public int MaxStructure { get; }
    public int CurrentStructure { get; private set; }
    
    // Components installed in this part
    public List<UnitComponent> Components { get; }

    public int ApplyDamage(int damage)
    {
        // First reduce armor
        var remainingDamage = damage;
        if (CurrentArmor > 0)
        {
            if (CurrentArmor >= remainingDamage)
            {
                CurrentArmor -= remainingDamage;
                return 0;
            }
            remainingDamage -= CurrentArmor;
            CurrentArmor = 0;
        }

        // Then apply to structure if armor is depleted
        if (remainingDamage > 0)
        {
            if (CurrentStructure >= remainingDamage)
            {
                CurrentStructure -= remainingDamage;
                return 0;
            }
            remainingDamage -= CurrentStructure;
            CurrentStructure = 0;
            return remainingDamage; // Return excess damage for transfer
        }

        return 0;
    }

    public T? GetComponent<T>() where T : UnitComponent
    {
        return Components.Find(c => c is T) as T;
    }

    public IEnumerable<T> GetComponents<T>() where T : UnitComponent
    {
        return Components.OfType<T>();
    }
}
