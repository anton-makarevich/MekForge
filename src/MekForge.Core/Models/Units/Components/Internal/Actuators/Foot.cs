namespace Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

public class FootActuator : Component
{
    private static readonly int[] FootSlots = { 3 };
    public FootActuator() : base("Foot Actuator", FootSlots)
    {
    }
}
