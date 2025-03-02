namespace Sanet.MekForge.Core.Services.Localization;

public class FakeLocalizationService: ILocalizationService
{
    public string GetString(string key)
    {
        return key switch
        {
            "Command_JoinGame" => "{0} has joined game with {1} units.",
            "Command_MoveUnit" => "{0} moved {1} to {2} facing {3} using {4}.",
            "Command_DeployUnit" => "{0} deployed {1} to {2} facing {3}.",
            "Command_RollDice" => "{0} rolls",
            "Command_DiceRolled" => "{0} rolled {1}.",
            "Command_UpdatePlayerStatus" => "{0}'s status is {1}.",
            "Command_ChangePhase" => "Game changed phase to {0}.",
            "Command_ChangeActivePlayer" => "{0}'s turn.",
            "Command_ChangeActivePlayerUnits" => "{0}'s turn to play {1} units.",
            "Command_WeaponConfiguration_TorsoRotation" => "{0}'s {1} rotates torso to face {2}",
            "Command_WeaponConfiguration_ArmsFlip" => "{0}'s {1} flips arms {2}",
            "Command_WeaponAttackDeclaration_NoAttacks" => "{0}'s {1} declares no attacks",
            "Command_WeaponAttackDeclaration_Header" => "{0}'s {1} declares attacks:",
            "Command_WeaponAttackDeclaration_WeaponLine" => "- {0} targeting {1}'s {2}",
            "Direction_Forward" => "forward",
            "Direction_Backward" => "backward",
            
            // Attack modifiers
            "Modifier_GunnerySkill" => "Gunnery Skill: +{0}",
            "Modifier_AttackerMovement" => "Attacker Movement ({0}): +{1}",
            "Modifier_TargetMovement" => "Target Movement ({0} hexes): +{1}",
            "Modifier_Range" => "{0} at {1} hexes ({2} range): +{3}",
            "Modifier_Heat" => "Heat Level ({0}): +{1}",
            "Modifier_Terrain" => "{0} at {1}: +{2}",
            
            // Attack information
            "Attack_NoLineOfSight" => "No LOS",
            "Attack_TargetNumber" => "Target ToHit Number",
            "Attack_OutOfRange" => "Target out of range",
            "Attack_NoModifiersCalculated" => "Attack modifiers not calculated",
            "Attack_Targeting" => "Already targeting {0}",
            
            // Secondary target modifiers
            "Attack_SecondaryTargetFrontArc" => "Secondary target (front arc): +{0}",
            "Attack_SecondaryTargetOtherArc" => "Secondary target (other arc): +{0}",
            
            _ => key
        };
    }
}