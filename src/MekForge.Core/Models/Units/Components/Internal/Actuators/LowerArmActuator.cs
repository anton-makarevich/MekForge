namespace Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

public class LowerArmActuator : Component
{
    private static readonly int[] LowerArmSlots = [2];
    public LowerArmActuator() : base("Lower Arm", LowerArmSlots)
    {
    }
}
