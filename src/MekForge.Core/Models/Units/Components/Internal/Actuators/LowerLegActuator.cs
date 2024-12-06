namespace Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

public class LowerLegActuator : Component
{
    private static readonly int[] LowerLegSlots = { 2 };
    public LowerLegActuator() : base("Lower Leg", LowerLegSlots)
    {
    }
}
