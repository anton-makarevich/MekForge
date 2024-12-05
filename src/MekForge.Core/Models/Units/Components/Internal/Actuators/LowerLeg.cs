namespace Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

public class LowerLeg : Component
{
    private static readonly int[] LowerLegSlots = { 2 };
    public LowerLeg() : base("Lower Leg", LowerLegSlots)
    {
    }
}
