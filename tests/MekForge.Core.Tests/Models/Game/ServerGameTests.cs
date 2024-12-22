using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class ServerGameTests
{
    private readonly ServerGame _serverGame;
    private readonly ICommandPublisher _commandPublisher;
    public ServerGameTests()
    {
        var battleMap = BattleMap.GenerateMap(5, 5,
            new SingleTerrainGenerator(5,5, new ClearTerrain()));
        
        _commandPublisher = Substitute.For<ICommandPublisher>();
        var rulesProvider = Substitute.For<IRulesProvider>();
        rulesProvider.GetStructureValues(20).Returns(new Dictionary<PartLocation, int>
        {
            { PartLocation.Head, 8 },
            { PartLocation.CenterTorso, 10 },
            { PartLocation.LeftTorso, 8 },
            { PartLocation.RightTorso, 8 },
            { PartLocation.LeftArm, 4 },
            { PartLocation.RightArm, 4 },
            { PartLocation.LeftLeg, 8 },
            { PartLocation.RightLeg, 8 }
        });
        _serverGame = new ServerGame(battleMap, rulesProvider, _commandPublisher);
    }

    [Fact]
    public void HandleCommand_ShouldAddPlayer_WhenJoinGameCommandIsReceived()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = new List<UnitData>()
        };

        // Act
        _serverGame.HandleCommand(joinCommand);

        // Assert
        _serverGame.Players.Should().HaveCount(1);
    }

    [Fact]
    public void HandleCommand_ShouldNotProcessOwnCommands_WhenGameOriginIdMatches()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units = new List<UnitData>(),
            GameOriginId = _serverGame.GameId // Set to this game's ID
        };

        // Act
        _serverGame.HandleCommand(command);

        // Assert
        // Verify that no players were added since the command was from this game instance
        _serverGame.Players.Should().BeEmpty();
    }
    
    [Fact]
    public void HandleCommand_ShouldProcessPlayerStatusCommand_WhenReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _serverGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = playerId,
            GameOriginId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units=[]
        });

        var statusCommand = new UpdatePlayerStatusCommand
        {
            PlayerId = playerId,
            GameOriginId = Guid.NewGuid(),
            PlayerStatus = PlayerStatus.Playing
        };

        // Act
        _serverGame.HandleCommand(statusCommand);

        // Assert
        var updatedPlayer = _serverGame.Players.FirstOrDefault(p => p.Id == playerId);
        updatedPlayer.Should().NotBeNull();
        updatedPlayer.Status.Should().Be(PlayerStatus.Playing);
    }

    [Fact]
    public void UpdatePhase_ShouldPublishPhaseChangedEvent_WhenCalled()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _serverGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = playerId,
            GameOriginId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units=[]
        });
        _serverGame.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerId = playerId,
            GameOriginId = Guid.NewGuid(),
            PlayerStatus = PlayerStatus.Playing
        });

        // Assert
        _serverGame.TurnPhase.Should().Be(Phase.Deployment);
        _commandPublisher.Received(1).PublishCommand(Arg.Is<ChangePhaseCommand>(cmd => 
            cmd.Phase == Phase.Deployment &&
            cmd.GameOriginId == _serverGame.GameId
        ));
    }
    
    [Fact]
    public void StartDeploymentPhase_ShouldRandomizeOrderAndSetActivePlayer_WhenAllPlayersReady()
    {
        // Arrange
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();
    
        _serverGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = playerId1,
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = new List<UnitData>()
        });

        _serverGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = playerId2,
            PlayerName = "Player2",
            GameOriginId = Guid.NewGuid(),
            Units = new List<UnitData>()
        });

        _serverGame.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerId = playerId1,
            GameOriginId = Guid.NewGuid(),
            PlayerStatus = PlayerStatus.Playing
        });

        _serverGame.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerId = playerId2,
            GameOriginId = Guid.NewGuid(),
            PlayerStatus = PlayerStatus.Playing
        });
        
        // Assert
        _serverGame.ActivePlayer.Should().NotBeNull();
        var expectedIds = new List<Guid> { playerId1, playerId2 };
        expectedIds.Should().Contain(_serverGame.ActivePlayer.Id);
    }
    
    [Fact]
    public void DeployUnit_ShouldDeployUnitAndSetNextPlayer_WhenCalled()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var unitData = MechFactoryIntegrationTests.LoadMechFromMtfFile("Resources/Mechs/LCT-1V.mtf");
        unitData.Id = unitId;
    
        _serverGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = playerId,
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [unitData]
        });
    
        _serverGame.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerId = playerId,
            GameOriginId = Guid.NewGuid(),
            PlayerStatus = PlayerStatus.Playing
        });
    
        // Act
        _serverGame.HandleCommand(new DeployUnitCommand
        {
            PlayerId = playerId,
            UnitId = unitId,
            GameOriginId = Guid.NewGuid(),
            Position = new HexCoordinateData(2, 3) ,
            Direction = 0
        });
    
        // Assert
        _serverGame.ActivePlayer.Should().BeNull();
    }
}