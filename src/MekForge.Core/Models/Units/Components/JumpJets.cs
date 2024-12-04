namespace Sanet.MekForge.Core.Models.Units.Components;

public abstract class JumpJets : UnitComponent
{
    protected JumpJets(string name, int slots, int jumpMP) : base(name, slots)
    {
        JumpMP = jumpMP;
    }

    public int JumpMP { get; }
}