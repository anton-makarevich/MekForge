using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands;

public class CommandFormattingTests
{
    private readonly IGame _game;
    private readonly ILocalizationService _localizationService;
    private readonly Player _player1;
    private readonly Player _player2;
    private readonly Guid _gameId = Guid.NewGuid();

    public CommandFormattingTests()
    {
        _localizationService = Substitute.For<ILocalizationService>();
        _game = Substitute.For<IGame>();
        
        _player1 = new Player(Guid.NewGuid(), "Player 1");
        _player2 = new Player(Guid.NewGuid(), "Player 2");
        
        _game.Players.Returns(new[] { _player1, _player2 });
    }

    [Fact]
    public void DeployUnitCommand_ShouldFormatCorrectly()
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

        var expectedFacingHex = position.Neighbor(HexDirection.TopRight);
        _localizationService.GetString("Command_DeployUnit").Returns("formatted deploy command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted deploy command");
        _localizationService.Received(1).GetString("Command_DeployUnit");
    }

    [Fact]
    public void JoinGameCommand_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            PlayerName = _player1.Name,
            Units = new List<UnitData>()
        };

        _localizationService.GetString("Command_JoinGame").Returns("formatted join command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted join command");
        _localizationService.Received(1).GetString("Command_JoinGame");
    }

    [Fact]
    public void UpdatePlayerStatusCommand_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new UpdatePlayerStatusCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            PlayerStatus = PlayerStatus.Playing
        };

        _localizationService.GetString("Command_UpdatePlayerStatus").Returns("formatted status command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted status command");
        _localizationService.Received(1).GetString("Command_UpdatePlayerStatus");
    }

    [Fact]
    public void ChangePhaseCommand_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new ChangePhaseCommand
        {
            GameOriginId = _gameId,
            Phase = PhaseNames.Deployment
        };

        _localizationService.GetString("Command_ChangePhase").Returns("formatted phase command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted phase command");
        _localizationService.Received(1).GetString("Command_ChangePhase");
    }

    [Fact]
    public void ChangeActivePlayerCommand_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new ChangeActivePlayerCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id
        };

        _localizationService.GetString("Command_ChangeActivePlayer").Returns("formatted active player command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted active player command");
        _localizationService.Received(1).GetString("Command_ChangeActivePlayer");
    }

    [Fact]
    public void DeployUnitCommand_ShouldReturnEmpty_WhenPlayerNotFound()
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

    [Fact]
    public void DeployUnitCommand_ShouldReturnEmpty_WhenUnitNotFound()
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
}
