using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public class GameCommandJsonConverterFactory : JsonConverterFactory
{
    private static readonly Dictionary<string, Type> TypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Client commands
        ["client"] = typeof(Client.ClientCommand),
        ["deploy"] = typeof(Client.DeployUnitCommand),
        ["join"] = typeof(Client.JoinGameCommand),
        ["move"] = typeof(Client.MoveUnitCommand),
        ["roll"] = typeof(Client.RollDiceCommand),
        ["status"] = typeof(Client.UpdatePlayerStatusCommand),
        
        // Server commands
        ["change_player"] = typeof(Server.ChangeActivePlayerCommand),
        ["change_phase"] = typeof(Server.ChangePhaseCommand),
        ["dice_rolled"] = typeof(Server.DiceRolledCommand)
    };

    public override bool CanConvert(Type typeToConvert) =>
        typeof(GameCommand).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        new GameCommandJsonConverter();

    private class GameCommandJsonConverter : JsonConverter<GameCommand>
    {
        public override GameCommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("$type", out var typeProperty))
            {
                throw new JsonException("Missing $type property");
            }

            var typeDiscriminator = typeProperty.GetString();
            if (string.IsNullOrEmpty(typeDiscriminator) || !TypeMap.TryGetValue(typeDiscriminator, out var type))
            {
                throw new JsonException($"Unknown command type: {typeDiscriminator}");
            }

            return (GameCommand)JsonSerializer.Deserialize(root.GetRawText(), type, options)!;
        }

        public override void Write(Utf8JsonWriter writer, GameCommand value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            var typeDiscriminator = TypeMap.FirstOrDefault(x => x.Value == type).Key;
            if (typeDiscriminator == null)
            {
                throw new JsonException($"Unknown command type: {type.Name}");
            }

            writer.WriteStartObject();
            writer.WriteString("$type", typeDiscriminator);
            
            foreach (var property in type.GetProperties())
            {
                var propertyValue = property.GetValue(value);
                if (propertyValue != null)
                {
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name);
                    JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
