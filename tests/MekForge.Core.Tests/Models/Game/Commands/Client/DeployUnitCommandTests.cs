using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class DeployUnitCommandTests : GameCommandTestBase<DeployUnitCommand>
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");
    private readonly Unit _unit;
    private readonly HexCoordinates _position = new(4, 5);

    public DeployUnitCommandTests()
    {
        _game.Players.Returns([_player1]);
        var unitData = MechFactoryTests.CreateDummyMechData();
        _unit = new MechFactory(new ClassicBattletechRulesProvider()).Create(unitData);
        _player1.AddUnit(_unit);
    }

    protected override DeployUnitCommand CreateCommand()
    {
        return new DeployUnitCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            UnitId = _unit.Id,
            Position = _position.ToData(),
            Direction = (int)HexDirection.TopRight
        };
    }

    protected override void AssertCommandSpecificProperties(DeployUnitCommand original, DeployUnitCommand? cloned)
    {
        base.AssertCommandSpecificProperties(original, cloned);
        cloned!.PlayerId.Should().Be(original.PlayerId);
        cloned.UnitId.Should().Be(original.UnitId);
        cloned.Position.Should().BeEquivalentTo(original.Position);
        cloned.Direction.Should().Be(original.Direction);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();
        _localizationService.GetString("Command_DeployUnit").Returns("formatted deploy command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted deploy command");
        _localizationService.Received(1).GetString("Command_DeployUnit");
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