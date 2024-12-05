namespace Sanet.MekForge.Core.Models.Units.Components;

public abstract class Component:IManufacturedItem
{
    protected Component(string name, int slots, string manufacturer = "Unknown")
    {
        Name = name;
        Slots = slots;
        Manufacturer = manufacturer;
    }

    public string Name { get; }
    public int Slots { get; }
    public bool IsDestroyed { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    public virtual void ApplyDamage()
    {
        IsDestroyed = true;
    }
    
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
    public string Manufacturer { get; }
}
