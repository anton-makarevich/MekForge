namespace Sanet.MekForge.Core.Models.Units.Components;

public class HeatSink : Component
{
    public HeatSink() : base("Heat Sink", new[] { 0 })
    {
        HeatDissipation = 1;
    }

    public int HeatDissipation { get; }
}
