using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class DeployUnitCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public DeployUnitCommandTests()
    {
        _game.Players.Returns([_player1]);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var unitData = MechFactoryTests.CreateDummyMechData();
        var unit = new MechFactory(new ClassicBattletechRulesProvider()).Create(unitData);
        _player1.AddUnit(unit);

        var position = new HexCoordinates(4, 5);
        var command = new DeployUnitCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            UnitId = unit.Id,
            Position = position.ToData(),
            Direction = (int)HexDirection.TopRight
        };

        _localizationService.GetString("Command_DeployUnit").Returns("formatted deploy command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted deploy command");
        _localizationService.Received(1).GetString("Command_DeployUnit");
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenUnitNotFound()
    {
        // Arrange
        var command = new DeployUnitCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            UnitId = Guid.NewGuid(), // Unknown unit ID
            Position = new HexCoordinates(0, 0).ToData(),
            Direction = 0
        };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().BeEmpty();
        _localizationService.DidNotReceive().GetString(Arg.Any<string>());
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var unknownPlayerId = Guid.NewGuid();
        var command = new DeployUnitCommand
        {
            GameOriginId = _gameId,
            PlayerId = unknownPlayerId,
            UnitId = Guid.NewGuid(),
            Position = new HexCoordinates(0, 0).ToData(),
            Direction = 0
        };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().BeEmpty();
        _localizationService.DidNotReceive().GetString(Arg.Any<string>());
    }
}