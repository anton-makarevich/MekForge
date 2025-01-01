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

public class MoveUnitCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public MoveUnitCommandTests()
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
        var command = new MoveUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player1.Id,
            UnitId = unit.Id,
            Destination = position.ToData()
        };
        
        _localizationService.GetString("Command_MoveUnit").Returns("formatted move command"); 

        // Act
        var result = command.Format(_localizationService, _game);
        
        // Assert
        result.Should().Be("formatted move command");
    }
    
    [Fact]
    public void Format_ShouldReturnEmpty_WhenUnitNotFound()
    {
        // Arrange
        var command = new MoveUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player1.Id,
            UnitId = Guid.NewGuid(), // Unknown unit ID
            Destination = new HexCoordinates(0, 0).ToData()
        };
        
        // Act
        var result = command.Format(_localizationService, _game);
        
        // Assert
        result.Should().BeEmpty();
    }
    
    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var unknownPlayerId = Guid.NewGuid();
        var command = new MoveUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = unknownPlayerId,
            UnitId = Guid.NewGuid(),
            Destination = new HexCoordinates(0, 0).ToData()
        };
        
        // Act
        var result = command.Format(_localizationService, _game);
        
        // Assert
        result.Should().BeEmpty();
    }
}