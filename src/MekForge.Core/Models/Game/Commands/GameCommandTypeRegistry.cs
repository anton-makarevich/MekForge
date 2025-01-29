using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;

namespace Sanet.MekForge.Core.Models.Game.Commands;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(GameCommand), TypeInfoPropertyName = "GameCommand")]
[JsonSerializable(typeof(DeployUnitCommand), TypeInfoPropertyName = "DeployUnitCommand")]
[JsonSerializable(typeof(JoinGameCommand), TypeInfoPropertyName = "JoinGameCommand")]
[JsonSerializable(typeof(MoveUnitCommand), TypeInfoPropertyName = "MoveUnitCommand")]
[JsonSerializable(typeof(RollDiceCommand), TypeInfoPropertyName = "RollDiceCommand")]
[JsonSerializable(typeof(UpdatePlayerStatusCommand), TypeInfoPropertyName = "UpdatePlayerStatusCommand")]
[JsonSerializable(typeof(ChangeActivePlayerCommand), TypeInfoPropertyName = "ChangeActivePlayerCommand")]
[JsonSerializable(typeof(ChangePhaseCommand), TypeInfoPropertyName = "ChangePhaseCommand")]
[JsonSerializable(typeof(DiceRolledCommand), TypeInfoPropertyName = "DiceRolledCommand")]
public partial class GameCommandJsonContext : JsonSerializerContext { }

public static class GameCommandTypeRegistry
{
    private static readonly Dictionary<string, (Type Type, JsonTypeInfo TypeInfo)> TypeMap;

    static GameCommandTypeRegistry()
    {
        // Manual registration of all command types with their JsonTypeInfo
        TypeMap = new Dictionary<string, (Type Type, JsonTypeInfo TypeInfo)>
        {
            ["deploy"] = (typeof(DeployUnitCommand), GameCommandJsonContext.Default.DeployUnitCommand),
            ["join"] = (typeof(JoinGameCommand), GameCommandJsonContext.Default.JoinGameCommand),
            ["move"] = (typeof(MoveUnitCommand), GameCommandJsonContext.Default.MoveUnitCommand),
            ["roll"] = (typeof(RollDiceCommand), GameCommandJsonContext.Default.RollDiceCommand),
            ["status"] = (typeof(UpdatePlayerStatusCommand), GameCommandJsonContext.Default.UpdatePlayerStatusCommand),
            ["change_player"] = (typeof(ChangeActivePlayerCommand), GameCommandJsonContext.Default.ChangeActivePlayerCommand),
            ["change_phase"] = (typeof(ChangePhaseCommand), GameCommandJsonContext.Default.ChangePhaseCommand),
            ["dice_rolled"] = (typeof(DiceRolledCommand), GameCommandJsonContext.Default.DiceRolledCommand)
        };
    }

    public static string Serialize(GameCommand command)
    {
        var type = command.GetType();
        var discriminator = TypeMap.FirstOrDefault(x => x.Value.Type == type).Key 
            ?? throw new JsonException($"Unknown command type: {type.Name}");
        var typeInfo = TypeMap[discriminator].TypeInfo;
        
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        
        writer.WriteStartObject();
        writer.WriteString("$type", discriminator);
        
        var jsonElement = JsonSerializer.SerializeToElement(command, typeInfo);
        foreach (var property in jsonElement.EnumerateObject())
        {
            property.WriteTo(writer);
        }
        
        writer.WriteEndObject();
        writer.Flush();
        
        return System.Text.Encoding.UTF8.GetString(ms.ToArray());
    }

    public static GameCommand Deserialize(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("$type", out var typeProperty))
        {
            throw new JsonException("Missing $type property");
        }

        var typeDiscriminator = typeProperty.GetString();
        if (string.IsNullOrEmpty(typeDiscriminator) || !TypeMap.TryGetValue(typeDiscriminator, out var typeInfo))
        {
            throw new JsonException($"Unknown command type: {typeDiscriminator}");
        }

        return (GameCommand)root.Deserialize(typeInfo.TypeInfo)!;
    }

    public static IReadOnlyDictionary<string, Type> GetRegisteredTypes() => 
        TypeMap.ToDictionary(x => x.Key, x => x.Value.Type);
}