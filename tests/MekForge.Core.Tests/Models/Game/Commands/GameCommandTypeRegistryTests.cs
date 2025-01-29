using System.Text.Json;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands;

public class GameCommandTypeRegistryTests
{
    [Fact]
    public void GetRegisteredTypes_ShouldReturnAllCommandTypes()
    {
        // Act
        var types = GameCommandTypeRegistry.GetRegisteredTypes();

        // Assert
        types.Count.ShouldBeGreaterThan(0);
        types.Values.ShouldContain(typeof(JoinGameCommand));
        types.Values.ShouldContain(typeof(MoveUnitCommand));
        types.Values.ShouldContain(typeof(DeployUnitCommand));
        // ... add other command types as needed
    }

    [Fact]
    public void Serialize_ShouldIncludeTypeDiscriminator()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerName = "PLayer",
            Units = [],
            Tint = "#FF0000",
            PlayerId = Guid.NewGuid()
        };

        // Act
        var json = GameCommandTypeRegistry.Serialize(command);

        // Assert
        json.ShouldContain("\"$type\":\"join\"");
    }

    [Fact]
    public void Deserialize_ShouldRestoreCorrectType()
    {
        // Arrange
        var originalCommand = new JoinGameCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerName = "PLayer",
            Units = [],
            Tint = "#FF0000",
            PlayerId = Guid.NewGuid()
        };
        var json = GameCommandTypeRegistry.Serialize(originalCommand);

        // Act
        var deserializedCommand = GameCommandTypeRegistry.Deserialize(json);

        // Assert
        deserializedCommand.ShouldNotBeNull();
        deserializedCommand.ShouldBeOfType<JoinGameCommand>();
        deserializedCommand.GameOriginId.ShouldBe(originalCommand.GameOriginId);
    }

    [Fact]
    public void Deserialize_WithMissingTypeProperty_ShouldThrow()
    {
        // Arrange
        var json = "{\"gameOriginId\":\"00000000-0000-0000-0000-000000000000\"}";

        // Act & Assert
        Should.Throw<JsonException>(() => GameCommandTypeRegistry.Deserialize(json))
            .Message.ShouldBe("Missing $type property");
    }

    [Fact]
    public void Deserialize_WithUnknownType_ShouldThrow()
    {
        // Arrange
        var json = "{\"$type\":\"unknown_type\",\"gameOriginId\":\"00000000-0000-0000-0000-000000000000\"}";

        // Act & Assert
        Should.Throw<JsonException>(() => GameCommandTypeRegistry.Deserialize(json))
            .Message.ShouldBe("Unknown command type: unknown_type");
    }

    [Fact]
    public void Serialize_WithUnregisteredType_ShouldThrow()
    {
        // Arrange
        var command = new UnregisteredCommand { GameOriginId = Guid.NewGuid() };

        // Act & Assert
        Should.Throw<JsonException>(() => GameCommandTypeRegistry.Serialize(command))
            .Message.ShouldBe($"Unknown command type: {nameof(UnregisteredCommand)}");
    }

    [Fact]
    public void SerializeAndDeserialize_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalCommand = new MoveUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            MovementType = MovementType.StandingStill,
            MovementPath = [],
            PlayerId = Guid.NewGuid(),
        };

        // Act
        var json = GameCommandTypeRegistry.Serialize(originalCommand);
        var deserializedCommand = GameCommandTypeRegistry.Deserialize(json) as MoveUnitCommand;

        // Assert
        deserializedCommand.ShouldNotBeNull();
        deserializedCommand.GameOriginId.ShouldBe(originalCommand.GameOriginId);
        // Assert other properties are preserved
    }

    private record UnregisteredCommand : GameCommand
    {
        public override string Format(ILocalizationService localizationService, IGame game) => string.Empty;
    }
}
