namespace Sanet.MekForge.Core.Models.Units.Components;

public class JumpJets : Component
{
    public JumpJets(int jumpMp =1) : base("Jump Jets", 1)
    {
        JumpMp = jumpMp;
    }

    public int JumpMp { get; }
    public override void ApplyDamage()
    {
        IsDestroyed = true;
    }
}