using System.Collections.Generic;
using System.Linq;
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
        IReadOnlyDictionary<PartLocation, UnitPartData> partsData) 
        : base(chassis, model, tonnage, walkMp, partsData)
    {
        Status = MechStatus.Active;
    }

    public MechStatus Status { get; private set; }

    protected override PartLocation? GetTransferLocation(PartLocation location) => location switch
    {
        PartLocation.LeftArm => PartLocation.LeftTorso,
        PartLocation.RightArm => PartLocation.RightTorso,
        PartLocation.LeftLeg => PartLocation.LeftTorso,
        PartLocation.RightLeg => PartLocation.RightTorso,
        PartLocation.Head => PartLocation.CenterTorso,
        PartLocation.LeftTorso => PartLocation.CenterTorso,
        PartLocation.RightTorso => PartLocation.CenterTorso,
        _ => null
    };

    // Heat management
    public int HeatDissipation => GetAllComponents<HeatSink>().Sum(hs => hs.HeatDissipation);
    
    public override void ApplyHeat(int heat)
    {
        CurrentHeat = System.Math.Max(0, CurrentHeat + heat - HeatDissipation);
        ApplyHeatEffects();
    }

    private void ApplyHeatEffects()
    {
        // Apply effects based on current heat level
        if (CurrentHeat >= 30)
        {
            // Automatic shutdown
            Status = MechStatus.Shutdown;
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

    // Status management
    public void Startup()
    {
        if (Status == MechStatus.Shutdown || Status == MechStatus.PoweredDown)
        {
            Status = MechStatus.Active;
        }
    }

    public void Shutdown()
    {
        if (Status == MechStatus.Active)
        {
            Status = MechStatus.Shutdown;
        }
    }

    public void SetProne()
    {
        Status |= MechStatus.Prone;
    }

    public void StandUp()
    {
        Status &= ~MechStatus.Prone;
    }
}
