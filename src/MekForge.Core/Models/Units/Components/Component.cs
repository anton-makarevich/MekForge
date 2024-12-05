namespace Sanet.MekForge.Core.Models.Units.Components;

public abstract class Component : IManufacturedItem
{
    protected Component(string name, int[] slots, string manufacturer = "Unknown")
    {
        Name = name;
        RequiredSlots = slots;
        Manufacturer = manufacturer;
        _mountedSlots = null; // null means not mounted yet
    }

    public string Name { get; }
    public int[] RequiredSlots { get; }
    public bool IsDestroyed { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    public string Manufacturer { get; }

    // Slot positioning
    private int[]? _mountedSlots;
    public int[] OccupiedSlots => _mountedSlots ?? Array.Empty<int>();
    public bool IsMounted => _mountedSlots != null;
    public int SlotsCount => RequiredSlots.Length;

    public void Mount()
    {
        _mountedSlots = RequiredSlots;
    }

    public virtual void Hit()
    {
        IsDestroyed = true;
    }
    
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
}
