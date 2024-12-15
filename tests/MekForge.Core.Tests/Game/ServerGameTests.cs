using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Game;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Game;

public class ServerGameTests
{
    private readonly ServerGame _serverGame;

    public ServerGameTests()
    {
        var battleState = new BattleState(new BattleMap(5, 5));
        var commandPublisher = Substitute.For<ICommandPublisher>();
        var rulesProvider = Substitute.For<IRulesProvider>();
        _serverGame = new ServerGame(battleState, rulesProvider, commandPublisher);
    }

    [Fact]
    public void HandleCommand_ShouldAddPlayer_WhenJoinGameCommandIsReceived()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units = new List<UnitData>()
        };

        // Act
        _serverGame.HandleCommand(joinCommand);

        // Assert
        _serverGame.Players.Should().HaveCount(1);
    }
}