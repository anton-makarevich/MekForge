namespace Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

public class LowerArm : Component
{
    private static readonly int[] LowerArmSlots = { 2 };
    public LowerArm() : base("Lower Arm", LowerArmSlots)
    {
    }
}
