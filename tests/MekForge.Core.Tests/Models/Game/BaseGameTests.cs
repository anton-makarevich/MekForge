using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class BaseGameTests() : BaseGame(BattleMap.GenerateMap(5, 5, new SingleTerrainGenerator(5,5, new ClearTerrain())),
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

    // Additional tests for common functionalities can be added here
    public override void HandleCommand(GameCommand command)
    {
        throw new NotImplementedException();
    }
}