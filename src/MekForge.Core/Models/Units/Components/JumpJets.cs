namespace Sanet.MekForge.Core.Models.Units.Components;

public class JumpJets : Component
{
    public JumpJets() : base("Jump Jets", new[] { 0 })
    {
    }

    public int JumpMp { get; }
}