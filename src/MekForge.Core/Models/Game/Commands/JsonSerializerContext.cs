using System.Text.Json.Serialization;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;

namespace Sanet.MekForge.Core.Models.Game.Commands;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(GameCommand))]
[JsonSerializable(typeof(GameCommand[]))]
[JsonSerializable(typeof(ClientCommand))]
[JsonSerializable(typeof(DeployUnitCommand))]
[JsonSerializable(typeof(JoinGameCommand))]
[JsonSerializable(typeof(MoveUnitCommand))]
[JsonSerializable(typeof(RollDiceCommand))]
[JsonSerializable(typeof(UpdatePlayerStatusCommand))]
[JsonSerializable(typeof(ChangeActivePlayerCommand))]
[JsonSerializable(typeof(ChangePhaseCommand))]
[JsonSerializable(typeof(DiceRolledCommand))]
public partial class GameCommandContext : JsonSerializerContext
{
}
