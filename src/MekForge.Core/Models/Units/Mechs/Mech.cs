using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public class Mech : Unit
{
    public Mech(
        string chassis,
        string model, 
        int tonnage, 
        int walkMp,
        IEnumerable<UnitPart> parts,
        Guid? id = null) 
        : base(chassis, model, tonnage, walkMp, parts,id)
    {
        Status = UnitStatus.Active;
    }

    protected override PartLocation? GetTransferLocation(PartLocation location) => location switch
    {
        PartLocation.LeftArm => PartLocation.LeftTorso,
        PartLocation.RightArm => PartLocation.RightTorso,
        PartLocation.LeftLeg => PartLocation.LeftTorso,
        PartLocation.RightLeg => PartLocation.RightTorso,
        PartLocation.LeftTorso => PartLocation.CenterTorso,
        PartLocation.RightTorso => PartLocation.CenterTorso,
        _ => null
    };

    // Heat management
    public int HeatDissipation => GetAllComponents<HeatSink>().Sum(hs => hs.HeatDissipation);
    
    public override void ApplyHeat(int heat)
    {
        CurrentHeat = Math.Max(0, CurrentHeat + heat - HeatDissipation);
        ApplyHeatEffects();
    }

    public override void ApplyDamage(int damage, UnitPart targetPart)
    {
        base.ApplyDamage(damage, targetPart);
        var head = _parts.Find(p => p.Location == PartLocation.Head);
        if (head is { IsDestroyed: true })
        {
            Status = UnitStatus.Destroyed;
            return;
        }
        var centerTorso = _parts.Find(p => p.Location == PartLocation.CenterTorso);
        if (centerTorso is { IsDestroyed: true })
        {
            Status = UnitStatus.Destroyed;
        }
    }

    private void ApplyHeatEffects()
    {
        // Apply effects based on current heat level
        if (CurrentHeat >= 30)
        {
            // Automatic shutdown
            Status = UnitStatus.Shutdown;
        }
        else if (CurrentHeat >= 25)
        {
            // Chance to shutdown, ammo explosion, etc.
            // To be implemented
        }
    }

    public override int CalculateBattleValue()
    {
        var bv = Tonnage * 100; // Base value
        bv += GetAllComponents<Weapon>().Sum(w => w.BattleValue);
        return bv;
    }

    public override bool CanMoveBackward(MovementType type) => type == MovementType.Walk;

    public void SetProne()
    {
        Status |= UnitStatus.Prone;
    }

    public void StandUp()
    {
        Status &= ~UnitStatus.Prone;
    }
}
