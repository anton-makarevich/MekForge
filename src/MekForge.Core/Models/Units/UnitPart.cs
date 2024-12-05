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
    public int UsedSlots => Components.Sum(c => c.Slots);
    public int AvailableSlots => TotalSlots - UsedSlots;
    public bool IsDestroyed => CurrentStructure <= 0;
    
    // Components installed in this part
    public List<Component> Components { get; }

    public bool CanAddComponent(Component component, int startingSlot = -1)
    {
        if (component.Slots > AvailableSlots)
            return false;

        // If no specific slot requested, check if we have enough consecutive slots anywhere
        if (startingSlot == -1)
            return FindFirstAvailableSlot(component.Slots) != -1;

        // Check if requested slots are available
        var endSlot = startingSlot + component.Slots - 1;
        if (endSlot >= TotalSlots)
            return false;

        return !Components.Any(c => 
            c.IsMounted && // Only check mounted components
            !(endSlot < c.FirstOccupiedSlot || startingSlot > c.LastOccupiedSlot)); // Check for overlap
    }

    public bool TryAddComponent(Component component, int startingSlot = -1)
    {
        if (!CanAddComponent(component, startingSlot))
            return false;

        if (startingSlot == -1)
            startingSlot = FindFirstAvailableSlot(component.Slots);

        component.Mount(startingSlot);
        Components.Add(component);
        return true;
    }

    private int FindFirstAvailableSlot(int requiredSlots)
    {
        if (requiredSlots > AvailableSlots)
            return -1;

        // If no components yet, start at 0
        if (!Components.Any(c => c.IsMounted))
            return 0;

        // Try each possible starting position
        for (int startSlot = 0; startSlot <= TotalSlots - requiredSlots; startSlot++)
        {
            var endSlot = startSlot + requiredSlots - 1;
            if (!Components.Any(c => 
                c.IsMounted && 
                !(endSlot < c.FirstOccupiedSlot || startSlot > c.LastOccupiedSlot)))
            {
                return startSlot;
            }
        }

        return -1;
    }

    public Component? GetComponentAtSlot(int slot)
    {
        return Components.FirstOrDefault(c => 
            c.IsMounted && 
            slot >= c.FirstOccupiedSlot && 
            slot <= c.LastOccupiedSlot);
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
        return Components.Find(c => c is T) as T;
    }

    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        return Components.OfType<T>();
    }
}
