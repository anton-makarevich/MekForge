namespace Sanet.MekForge.Core.Models.Units.Components;

public class Masc : Component
{
    public Masc(string name, int[] slots) : base(name, slots)
    {
        Deactivate(); // MASC starts deactivated
    }
}