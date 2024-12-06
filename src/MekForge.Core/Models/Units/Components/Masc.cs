namespace Sanet.MekForge.Core.Models.Units.Components;

public class Masc : Component
{
    public Masc(string name, int slots) : base(name, Enumerable.Range(0, slots).ToArray())
    {
        Deactivate(); // MASC starts deactivated
    }

    public override void Hit()
    {
        base.Hit();
        Deactivate();
    }
}