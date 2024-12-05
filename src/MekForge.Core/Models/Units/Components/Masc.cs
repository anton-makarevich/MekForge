namespace Sanet.MekForge.Core.Models.Units.Components;

public class Masc : Component
{
    public Masc(string name, int slots) : base(name, slots)
    {
        IsActive = false; // MASC starts inactive
    }

    public override void ApplyDamage()
    {
        IsDestroyed = true;
        Deactivate();
    }
}