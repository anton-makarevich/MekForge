namespace Sanet.MekForge.Core.Models.Units.Components;

public class HeatSink : UnitComponent
{
    public HeatSink(int dissipation = 1, string name = "Heat Sink") : base(name, 1)
    {
        HeatDissipation = dissipation;
    }

    public int HeatDissipation { get; }

    public override void ApplyDamage()
    {
        IsDestroyed = true;
    }
}
