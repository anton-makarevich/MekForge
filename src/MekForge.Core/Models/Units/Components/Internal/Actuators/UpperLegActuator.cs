namespace Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

public class UpperLegActuator : Component
{
    private static readonly int[] UpperLegSlots = { 1 };
    public UpperLegActuator() : base("Upper Leg", UpperLegSlots)
    {
    }
}
