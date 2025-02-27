using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Utils.TechRules;

public class ClassicBattletechRulesProvider : IRulesProvider
{
    public Dictionary<PartLocation, int> GetStructureValues(int tonnage)
    {
        var structureValues = new Dictionary<PartLocation, int>();

        // Structure values based on tonnage
        switch (tonnage)
        {
            case 20:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 6;
                structureValues[PartLocation.LeftTorso] = 5;
                structureValues[PartLocation.RightTorso] = 5;
                structureValues[PartLocation.LeftArm] = 3;
                structureValues[PartLocation.RightArm] = 3;
                structureValues[PartLocation.LeftLeg] = 4;
                structureValues[PartLocation.RightLeg] = 4;
                break;
            case 25:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 8;
                structureValues[PartLocation.LeftTorso] = 6;
                structureValues[PartLocation.RightTorso] = 6;
                structureValues[PartLocation.LeftArm] = 4;
                structureValues[PartLocation.RightArm] = 4;
                structureValues[PartLocation.LeftLeg] = 6;
                structureValues[PartLocation.RightLeg] = 6;
                break;
            case 30:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 10;
                structureValues[PartLocation.LeftTorso] = 7;
                structureValues[PartLocation.RightTorso] = 7;
                structureValues[PartLocation.LeftArm] = 5;
                structureValues[PartLocation.RightArm] = 5;
                structureValues[PartLocation.LeftLeg] = 7;
                structureValues[PartLocation.RightLeg] = 7;
                break;
            case 35:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 11;
                structureValues[PartLocation.LeftTorso] = 8;
                structureValues[PartLocation.RightTorso] = 8;
                structureValues[PartLocation.LeftArm] = 6;
                structureValues[PartLocation.RightArm] = 6;
                structureValues[PartLocation.LeftLeg] = 8;
                structureValues[PartLocation.RightLeg] = 8;
                break;
            case 40:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 12;
                structureValues[PartLocation.LeftTorso] = 10;
                structureValues[PartLocation.RightTorso] = 10;
                structureValues[PartLocation.LeftArm] = 6;
                structureValues[PartLocation.RightArm] = 6;
                structureValues[PartLocation.LeftLeg] = 10;
                structureValues[PartLocation.RightLeg] = 10;
                break;
            case 45:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 14;
                structureValues[PartLocation.LeftTorso] = 11;
                structureValues[PartLocation.RightTorso] = 11;
                structureValues[PartLocation.LeftArm] = 7;
                structureValues[PartLocation.RightArm] = 7;
                structureValues[PartLocation.LeftLeg] = 11;
                structureValues[PartLocation.RightLeg] = 11;
                break;
            case 50:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 16;
                structureValues[PartLocation.LeftTorso] = 12;
                structureValues[PartLocation.RightTorso] = 12;
                structureValues[PartLocation.LeftArm] = 8;
                structureValues[PartLocation.RightArm] = 8;
                structureValues[PartLocation.LeftLeg] = 12;
                structureValues[PartLocation.RightLeg] = 12;
                break;
            case 55:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 18;
                structureValues[PartLocation.LeftTorso] = 13;
                structureValues[PartLocation.RightTorso] = 13;
                structureValues[PartLocation.LeftArm] = 9;
                structureValues[PartLocation.RightArm] = 9;
                structureValues[PartLocation.LeftLeg] = 13;
                structureValues[PartLocation.RightLeg] = 13;
                break;
            case 60:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 20;
                structureValues[PartLocation.LeftTorso] = 14;
                structureValues[PartLocation.RightTorso] = 14;
                structureValues[PartLocation.LeftArm] = 10;
                structureValues[PartLocation.RightArm] = 10;
                structureValues[PartLocation.LeftLeg] = 14;
                structureValues[PartLocation.RightLeg] = 14;
                break;
            case 65:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 21;
                structureValues[PartLocation.LeftTorso] = 15;
                structureValues[PartLocation.RightTorso] = 15;
                structureValues[PartLocation.LeftArm] = 10;
                structureValues[PartLocation.RightArm] = 10;
                structureValues[PartLocation.LeftLeg] = 15;
                structureValues[PartLocation.RightLeg] = 15;
                break;
            case 70:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 22;
                structureValues[PartLocation.LeftTorso] = 15;
                structureValues[PartLocation.RightTorso] = 15;
                structureValues[PartLocation.LeftArm] = 11;
                structureValues[PartLocation.RightArm] = 11;
                structureValues[PartLocation.LeftLeg] = 15;
                structureValues[PartLocation.RightLeg] = 15;
                break;
            case 75:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 23;
                structureValues[PartLocation.LeftTorso] = 16;
                structureValues[PartLocation.RightTorso] = 16;
                structureValues[PartLocation.LeftArm] = 12;
                structureValues[PartLocation.RightArm] = 12;
                structureValues[PartLocation.LeftLeg] = 16;
                structureValues[PartLocation.RightLeg] = 16;
                break;
            case 80:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 25;
                structureValues[PartLocation.LeftTorso] = 17;
                structureValues[PartLocation.RightTorso] = 17;
                structureValues[PartLocation.LeftArm] = 13;
                structureValues[PartLocation.RightArm] = 13;
                structureValues[PartLocation.LeftLeg] = 17;
                structureValues[PartLocation.RightLeg] = 17;
                break;
            case 85:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 27;
                structureValues[PartLocation.LeftTorso] = 18;
                structureValues[PartLocation.RightTorso] = 18;
                structureValues[PartLocation.LeftArm] = 14;
                structureValues[PartLocation.RightArm] = 14;
                structureValues[PartLocation.LeftLeg] = 18;
                structureValues[PartLocation.RightLeg] = 18;
                break;
            case 90:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 29;
                structureValues[PartLocation.LeftTorso] = 19;
                structureValues[PartLocation.RightTorso] = 19;
                structureValues[PartLocation.LeftArm] = 15;
                structureValues[PartLocation.RightArm] = 15;
                structureValues[PartLocation.LeftLeg] = 19;
                structureValues[PartLocation.RightLeg] = 19;
                break;
            case 95:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 30;
                structureValues[PartLocation.LeftTorso] = 20;
                structureValues[PartLocation.RightTorso] = 20;
                structureValues[PartLocation.LeftArm] = 16;
                structureValues[PartLocation.RightArm] = 16;
                structureValues[PartLocation.LeftLeg] = 20;
                structureValues[PartLocation.RightLeg] = 20;
                break;
            case 100:
                structureValues[PartLocation.Head] = 3;
                structureValues[PartLocation.CenterTorso] = 31;
                structureValues[PartLocation.LeftTorso] = 21;
                structureValues[PartLocation.RightTorso] = 21;
                structureValues[PartLocation.LeftArm] = 17;
                structureValues[PartLocation.RightArm] = 17;
                structureValues[PartLocation.LeftLeg] = 21;
                structureValues[PartLocation.RightLeg] = 21;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tonnage), "Invalid tonnage");
        }

        return structureValues;
    }

    public int GetAmmoRounds(AmmoType ammoType)
    {
        return ammoType switch
        {
            AmmoType.None => 0,
            AmmoType.MachineGun => 200, 
            AmmoType.AC2 => 45, 
            AmmoType.AC5 => 20,
            AmmoType.AC10 => 10,
            AmmoType.AC20 => 5, 
            AmmoType.LRM5 => 24, 
            AmmoType.LRM10 => 12, 
            AmmoType.LRM15 => 8, 
            AmmoType.LRM20 => 6, 
            AmmoType.SRM2 => 50, 
            AmmoType.SRM4 => 25, 
            AmmoType.SRM6 => 15, 
            _ => throw new ArgumentOutOfRangeException(nameof(ammoType), "Invalid AmmoType"),
        };
    }

    public int GetAttackerMovementModifier(MovementType movementType)
    {
        return movementType switch
        {
            MovementType.StandingStill => 0,
            MovementType.Walk => 1,
            MovementType.Run => 2,
            MovementType.Jump => 3,
            MovementType.Prone => 2,
            _ => throw new ArgumentException($"Unknown movement type: {movementType}")
        };
    }

    public int GetTargetMovementModifier(int hexesMoved)
    {
        return hexesMoved switch
        {
            <= 2 => 0,    // 0-2 hexes: no modifier
            <= 4 => 1,    // 3-4 hexes: +1
            <= 6 => 2,    // 5-6 hexes: +2
            <= 9 => 3,    // 7-9 hexes: +3
            <= 17 => 4,   // 10-17 hexes: +4
            <= 24 => 5,   // 18-24 hexes: +5
            _ => 6        // 25+ hexes: +6
        };
    }

    public int GetRangeModifier(WeaponRange rangeType, int rangeValue, int distance)
    {
        return rangeType switch
        {
            WeaponRange.Minimum => rangeValue-distance+1,
            WeaponRange.Short => 0,
            WeaponRange.Medium => 2,
            WeaponRange.Long => 4,
            WeaponRange.OutOfRange => ToHitBreakdown.ImpossibleRoll,
            _ => throw new ArgumentException($"Unknown weapon range: {rangeType}")
        };
    }

    public int GetHeatModifier(int currentHeat)
    {
        return currentHeat switch
        {
            <= 7 => 0,    // 0-7 heat: no modifier
            <= 12 => 1,   // 8-12 heat: +1
            <= 16 => 2,   // 13-16 heat: +2
            <= 23 => 3,   // 17-23 heat: +3
            _ => 4        // 24+ heat: +4 (assuming max penalty)
        };
    }

    public int GetTerrainToHitModifier(string terrainId)
    {
        return terrainId switch
        {
            "LightWoods" => 1,
            "HeavyWoods" => 2,
            _ => 0 // Default no modifier
        };
    }

    public int GetSecondaryTargetModifier(bool isFrontArc)
    {
        return isFrontArc ? 1 : 2; // +1 for front arc, +2 for other arcs
    }
}