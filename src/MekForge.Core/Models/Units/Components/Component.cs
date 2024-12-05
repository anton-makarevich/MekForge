namespace Sanet.MekForge.Core.Models.Units.Components;

public abstract class Component:IManufacturedItem
{
    protected Component(string name, int slots, string manufacturer = "Unknown")
    {
        Name = name;
        Slots = slots;
        Manufacturer = manufacturer;
        FirstOccupiedSlot = -1; // -1 means not mounted yet
    }

    public string Name { get; }
    public int Slots { get; }
    public bool IsDestroyed { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    // Slot positioning
    public int FirstOccupiedSlot { get; private set; }
    public int LastOccupiedSlot => FirstOccupiedSlot >= 0 ? FirstOccupiedSlot + Slots - 1 : -1;
    public bool IsMounted => FirstOccupiedSlot >= 0;

    public void Mount(int startingSlot)
    {
        FirstOccupiedSlot = startingSlot;
    }

    public virtual void ApplyDamage()
    {
        IsDestroyed = true;
    }
    
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
    public string Manufacturer { get; }
}
