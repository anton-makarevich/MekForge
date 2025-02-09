using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public class Mech : Unit
{
    public int PossibleTorsoRotation { get; }
    
    public bool HasUsedTorsoTwist
    {
        get
        {
            if (Position == null) return false;
            var torsos = _parts.OfType<Torso>();
            return torsos.Any(t => t.Facing != Position.Value.Facing);
        }
    }

    public Mech(
        string chassis,
        string model, 
        int tonnage, 
        int walkMp,
        IEnumerable<UnitPart> parts,
        int possibleTorsoRotation = 1,
        Guid? id = null) 
        : base(chassis, model, tonnage, walkMp, parts, id)
    {
        PossibleTorsoRotation = possibleTorsoRotation;
        Status = UnitStatus.Active;
    }

    public bool CanRotateTorso()
    {
        return Position != null && PossibleTorsoRotation > 0 && !HasUsedTorsoTwist;
    }

    public bool TryRotateTorso(HexDirection newFacing)
    {
        if (!CanRotateTorso())
            return false;

        var currentUnitFacing = (int)Position!.Value.Facing;
        var newFacingInt = (int)newFacing;
        
        // Calculate steps in both directions (clockwise and counterclockwise)
        var clockwiseSteps = (newFacingInt - currentUnitFacing + 6) % 6;
        var counterClockwiseSteps = (currentUnitFacing - newFacingInt + 6) % 6;
        
        // Use the smaller number of steps
        var steps = Math.Min(clockwiseSteps, counterClockwiseSteps);
        
        // Check if rotation is within allowed range
        if (steps > PossibleTorsoRotation) return false;
        foreach (var torso in _parts.OfType<Torso>())
        {
            torso.Rotate(newFacing);
        }
        return true;
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

    public override HexPosition? Position
    {
        get => base.Position;
        protected set
        {
            base.Position = value;
            // Reset torso rotation when position changes
            foreach (var torso in _parts.OfType<Torso>())
            {
                torso.ResetRotation();
            }
        }
    }
}
