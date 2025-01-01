namespace Sanet.MekForge.Core.Services.Localization;

public class FakeLocalizationService: ILocalizationService
{
    public string GetString(string key)
    {
        return key switch
        {
            "Command_JoinGame" => "{0} has joined game with {1} units.",
            "Command_MoveUnit" => "{0} moved {1} to {2}.",
            "Command_DeployUnit" => "{0} deployed {1} to {2} facing {3}.",
            "Command_RollDice" => "{0} rolls",
            "Command_DiceRolled" => "{0} rolled {1}.",
            "Command_UpdatePlayerStatus" => "{0}'s status is {1}.",
            "Command_ChangePhase" => "Game changed phase to {0}.",
            "Command_ChangeActivePlayer" => "Game changed active player to {0}.",
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }
}