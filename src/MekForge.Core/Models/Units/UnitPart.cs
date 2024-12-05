using System.Collections.Generic;
using System.Linq;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Models.Units;

public class UnitPart
{
    public UnitPart(string name, PartLocation location, int maxArmor, int maxStructure, int slots)
    {
        Name = name;
        Location = location;
        CurrentArmor = MaxArmor = maxArmor;
        CurrentStructure = MaxStructure = maxStructure;
        TotalSlots = slots;
        Components = new List<Component>();
    }

    public string Name { get; }
    public PartLocation Location { get; }
    
    // Armor and Structure
    public int MaxArmor { get; }
    public int CurrentArmor { get; private set; }
    public int MaxStructure { get; }
    public int CurrentStructure { get; private set; }
    
    // Slots management
    public int TotalSlots { get; }
    public int UsedSlots => Components.Sum(c => c.SlotsCount);
    public int AvailableSlots => TotalSlots - UsedSlots;
    public bool IsDestroyed => CurrentStructure <= 0;
    
    // Components installed in this part
    public List<Component> Components { get; }

    public bool CanAddComponent(Component component)
    {
        if (component.SlotsCount > AvailableSlots)
            return false;

        // Check if any required slots would be out of bounds
        if (component.RequiredSlots.Any(s => s >= TotalSlots))
            return false;

        // Check if any of the required slots are already occupied
        var occupiedSlots = Components.Where(c => c.IsMounted)
                                    .SelectMany(c => c.OccupiedSlots)
                                    .ToHashSet();
        
        return !component.RequiredSlots.Intersect(occupiedSlots).Any();
    }

    public bool TryAddComponent(Component component)
    {
        if (!CanAddComponent(component))
            return false;

        component.Mount();
        Components.Add(component);
        return true;
    }

    public Component? GetComponentAtSlot(int slot)
    {
        return Components.FirstOrDefault(c => c.IsMounted && c.OccupiedSlots.Contains(slot));
    }

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

    public T? GetComponent<T>() where T : Component
    {
        return Components.OfType<T>().FirstOrDefault();
    }

    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        return Components.OfType<T>();
    }
}
