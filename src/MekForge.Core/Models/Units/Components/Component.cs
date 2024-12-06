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
        _mountedSlots = null; // null means not mounted yet
    }

    public string Name { get; }
    public int[] MountedAtSlots { get; private set; }
    public bool IsDestroyed { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    public string Manufacturer { get; }
    public bool IsFixed { get; }

    // Slot positioning
    private int[]? _mountedSlots;
    public int[] OccupiedSlots => _mountedSlots ?? Array.Empty<int>();
    public bool IsMounted => _mountedSlots != null;
    public int SlotsCount => MountedAtSlots.Length;

    public void Mount(int[] slots)
    {
        _mountedSlots = slots;
        MountedAtSlots = slots;
    }

    public void UnMount()
    {
        if (IsFixed)
        {
            throw new ComponentException("Fixed components cannot be unmounted.");
        }
        _mountedSlots = null; // Reset for non-fixed components
        MountedAtSlots = new int[0];
    }

    public virtual void Hit()
    {
        IsDestroyed = true;
    }
    
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
}
