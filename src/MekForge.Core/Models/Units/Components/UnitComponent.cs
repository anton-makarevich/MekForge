namespace Sanet.MekForge.Core.Models.Units.Components;

public abstract class UnitComponent
{
    protected UnitComponent(string name, int slots)
    {
        Name = name;
        Slots = slots;
    }

    public string Name { get; }
    public int Slots { get; }
    public bool IsDestroyed { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    public abstract void ApplyDamage();
    
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
}
