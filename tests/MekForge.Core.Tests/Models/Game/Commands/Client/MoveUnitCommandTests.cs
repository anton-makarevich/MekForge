using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Tests.Data.Community;
using Sanet.MekForge.Core.Utils;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class MoveUnitCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");
    private readonly Unit _unit;
    private readonly HexCoordinates _position = new(4, 5);

    public MoveUnitCommandTests()
    {
        _game.Players.Returns([_player1]);
        var unitData = MechFactoryTests.CreateDummyMechData();
        _unit = new MechFactory(new ClassicBattletechRulesProvider()).Create(unitData);
        _player1.AddUnit(_unit);
    }

    private MoveUnitCommand CreateCommand()
    {
        var startPos = new HexPosition(3, 5, HexDirection.Top);
        var endPos = new HexPosition(4, 5, HexDirection.Bottom);
        var pathSegment = new PathSegment(startPos, endPos, 1);

        return new MoveUnitCommand
        {
            MovementType = MovementType.Walk,
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            UnitId = _unit.Id,
            MovementPath = [pathSegment.ToData()]
        };
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();
        _unit.Deploy(new HexPosition(1, 1, HexDirection.Top));
        _localizationService.GetString("Command_MoveUnit").Returns("formatted move command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBe("formatted move command");
        _localizationService.Received(1).GetString("Command_MoveUnit");
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = CreateCommand() with { PlayerId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenUnitNotFound()
    {
        // Arrange
        var command = CreateCommand() with { UnitId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }
}