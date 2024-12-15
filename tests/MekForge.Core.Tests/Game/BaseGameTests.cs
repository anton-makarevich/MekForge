using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Game;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Game;

public class BaseGameTests() : BaseGame(new BattleState(
        new BattleMap(5, 5)),
        Substitute.For<IRulesProvider>(),
        Substitute.For<ICommandPublisher>())
{
    [Fact]
    public void AddPlayer_ShouldAddPlayer_WhenJoinGameCommandIsReceived()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units = new List<UnitData>()
        };

        // Act
        AddPlayer(joinCommand);

        // Assert
        Players.Should().HaveCount(1);
    }

    // Additional tests for common functionalities can be added here
    public override void HandleCommand(GameCommand command)
    {
        throw new NotImplementedException();
    }
}