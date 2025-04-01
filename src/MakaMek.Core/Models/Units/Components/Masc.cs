namespace Sanet.MakaMek.Core.Models.Units.Components;

public class Masc : Component
{
    public Masc(string name) : base(name, [])
    {
        Deactivate(); // MASC starts deactivated
    }

    public override void Hit()
    {
        base.Hit();
        Deactivate();
    }
}