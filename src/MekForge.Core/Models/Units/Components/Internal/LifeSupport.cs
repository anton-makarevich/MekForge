namespace Sanet.MekForge.Core.Models.Units.Components.Internal;

public class LifeSupport : Component
{
    // Life Support takes slots 1 and 6 in head
    private static readonly int[] LifeSupportSlots = { 0, 5 };

    public LifeSupport() : base("Life Support", new[] { 0, 5 })
    {
    }
}
