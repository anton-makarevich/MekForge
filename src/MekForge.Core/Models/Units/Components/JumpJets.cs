namespace Sanet.MekForge.Core.Models.Units.Components;

public class JumpJets : Component
{
    public JumpJets() : this(1)
    {
    }

    public JumpJets(int jumpMp) : base("Jump Jets", new[] { 0 })
    {
        JumpMp = jumpMp;
    }

    public int JumpMp { get; }

    public override void Hit()
    {
        base.Hit();
    }
}