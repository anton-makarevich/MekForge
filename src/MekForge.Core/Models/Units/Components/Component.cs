using Sanet.MekForge.Core.Exceptions;

namespace Sanet.MekForge.Core.Models.Units.Components;

public abstract class Component : IManufacturedItem
{
    protected Component(string name, int[] slots, string manufacturer = "Unknown")
    {
        Name = name;
        MountedAtSlots = slots;
        IsFixed = slots.Length > 0;
        Manufacturer = manufacturer;
    }

    public string Name { get; }
    public int[] MountedAtSlots { get; private set; }
    public bool IsDestroyed { get; private set; }
    public bool IsActive { get; protected set; } = true;
    public string Manufacturer { get; }
    public bool IsFixed { get; }
    public int BattleValue { get; protected set; }

    // Slot positioning
    public int[] OccupiedSlots => MountedAtSlots;
    public bool IsMounted => MountedAtSlots.Length > 0;
    public bool HasMountingSlots => MountedAtSlots.Length > 0;
    public int SlotsCount => MountedAtSlots.Length;

    public void Mount(int[] slots)
    {
        if (IsMounted) return;
        
        MountedAtSlots = slots;
    }

    public void UnMount()
    {
        if (IsFixed)
        {
            throw new ComponentException("Fixed components cannot be unmounted.");
        }
        if (!IsMounted) return;
        
        MountedAtSlots = [];
    }

    public virtual void Hit()
    {
        IsDestroyed = true;
    }
    
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
}
