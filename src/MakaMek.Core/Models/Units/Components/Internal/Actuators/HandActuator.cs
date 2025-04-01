namespace Sanet.MakaMek.Core.Models.Units.Components.Internal.Actuators;

public class HandActuator : Component
{
    private static readonly int[] HandSlots = [3];
    public HandActuator() : base("Hand Actuator", HandSlots)
    {
    }
}
