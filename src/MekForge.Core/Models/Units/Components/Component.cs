using Sanet.MekForge.Core.Exceptions;

namespace Sanet.MekForge.Core.Models.Units.Components;

public abstract class Component : IManufacturedItem
{
    protected Component(string name, int[] slots, int size = 1, string manufacturer = "Unknown")
    {
        Name = name;
        MountedAtSlots = slots;
        IsFixed = slots.Length > 0;
        Size = IsFixed
        ? slots.Length
        : size;
        Manufacturer = manufacturer;
    }

    public string Name { get; }
    public int[] MountedAtSlots { get; private set; }
    public bool IsDestroyed { get; private set; }
    public bool IsActive { get; protected set; } = true;
    
    public int Size { get; }
    public string Manufacturer { get; }
    public bool IsFixed { get; }
    public int BattleValue { get; protected set; }

    // Slot positioning
    public bool IsMounted => MountedAtSlots.Length > 0;

    public void Mount(int[] slots)
    {
        if (IsMounted) return;
        if (slots.Length != Size)
        {
            throw new ComponentException($"Component {Name} requires {Size} slots.");
        }
        
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
