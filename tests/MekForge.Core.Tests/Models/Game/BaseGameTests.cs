using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class BaseGameTests() : BaseGame(BattleMap.GenerateMap(5, 5, new SingleTerrainGenerator(5,5, new ClearTerrain())),
        new ClassicBattletechRulesProvider(),
        Substitute.For<ICommandPublisher>(),
        Substitute.For<IToHitCalculator>())
{
    [Fact]
    public void AddPlayer_ShouldAddPlayer_WhenJoinGameCommandIsReceived()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };

        // Act
        OnPlayerJoined(joinCommand);

        // Assert
        Players.Count.ShouldBe(1);
    }

    [Fact]
    public void New_ShouldHaveCorrectTurnAndPhase()
    {
        Turn.ShouldBe(1);
        TurnPhase.ShouldBe(PhaseNames.Start);
    }

    [Fact]
    public void OnWeaponConfiguration_DoesNothing_WhenPlayerNotFound()
    {
        // Arrange
        var command = new WeaponConfigurationCommand
        {
            GameOriginId = Id,
            PlayerId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            Configuration = new WeaponConfiguration
            {
                Type = WeaponConfigurationType.TorsoRotation,
                Value = (int)HexDirection.Bottom
            }
        };

        // Act
        OnWeaponConfiguration(command);

        // Assert
        // No exception should be thrown
    }

    [Fact]
    public void OnWeaponConfiguration_DoesNothing_WhenUnitNotFound()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };
        OnPlayerJoined(joinCommand);
        var player = Players.First();
        var command = new WeaponConfigurationCommand
        {
            GameOriginId = Id,
            PlayerId = player.Id,
            UnitId = Guid.NewGuid(),
            Configuration = new WeaponConfiguration
            {
                Type = WeaponConfigurationType.TorsoRotation,
                Value = (int)HexDirection.Bottom
            }
        };

        // Act
        OnWeaponConfiguration(command);

        // Assert
        // No exception should be thrown
    }

    [Fact]
    public void OnWeaponConfiguration_RotatesTorso_WhenConfigurationIsTorsoRotation()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [MechFactoryTests.CreateDummyMechData()],
            Tint = "#FF0000"
        };
        OnPlayerJoined(joinCommand);
        var player = Players.First();
        var mech = player.Units.First() as Mech;
        mech?.Deploy(new HexPosition(new HexCoordinates(3, 3), HexDirection.BottomLeft));

        var command = new WeaponConfigurationCommand
        {
            GameOriginId = Id,
            PlayerId = player.Id,
            UnitId = mech!.Id,
            Configuration = new WeaponConfiguration
            {
                Type = WeaponConfigurationType.TorsoRotation,
                Value = (int)HexDirection.Bottom
            }
        };

        // Act
        OnWeaponConfiguration(command);

        // Assert
        mech.TorsoDirection.ShouldBe(HexDirection.Bottom);
    }

    public override void HandleCommand(GameCommand command)
    {
        throw new NotImplementedException();
    }
}