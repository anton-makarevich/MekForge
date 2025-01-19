using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class MoveUnitCommandTests : GameCommandTestBase<MoveUnitCommand>
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

    protected override MoveUnitCommand CreateCommand()
    {
        return new MoveUnitCommand
        {
            Direction =  1,
            MovementType = MovementType.Walk,
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            UnitId = _unit.Id,
            Destination = _position.ToData(),
            MovementPoints = 5,
            // PathSegments = [
            //     (new HexCoordinates(3, 5).ToData(), new HexCoordinates(4, 5).ToData(), 1)
            // ]
        };
    }

    protected override void AssertCommandSpecificProperties(MoveUnitCommand original, MoveUnitCommand? cloned)
    {
        base.AssertCommandSpecificProperties(original, cloned);
        cloned!.PlayerId.Should().Be(original.PlayerId);
        cloned.UnitId.Should().Be(original.UnitId);
        cloned.Destination.Should().BeEquivalentTo(original.Destination);
        //cloned.PathSegments.Should().BeEquivalentTo(original.PathSegments);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();
        _localizationService.GetString("Command_MoveUnit").Returns("formatted move command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted move command");
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
        result.Should().BeEmpty();
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenUnitNotFound()
    {
        // Arrange
        var command = CreateCommand() with { UnitId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().BeEmpty();
    }
}