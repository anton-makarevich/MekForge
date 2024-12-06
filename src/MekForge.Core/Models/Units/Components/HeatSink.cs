namespace Sanet.MekForge.Core.Models.Units.Components;

public class HeatSink : Component
{
    public HeatSink() : this(1, "Heat Sink")
    {
    }

    public HeatSink(int dissipation, string name) : base(name, new[] { 0 })
    {
        HeatDissipation = dissipation;
    }

    public int HeatDissipation { get; }

    public override void Hit()
    {
        base.Hit();
    }
}
