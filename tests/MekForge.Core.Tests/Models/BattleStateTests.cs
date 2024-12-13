using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Utils.Generators;

namespace Sanet.MekForge.Core.Tests.Models;

public class BattleStateTests
{
    private readonly BattleState _battleState;

    public BattleStateTests()
    {
        var generator = Substitute.For<ITerrainGenerator>();
        generator.Generate(Arg.Any<HexCoordinates>())
            .Returns(c => {
                var hex = new Hex(c.Arg<HexCoordinates>());
                hex.AddTerrain(new ClearTerrain());
                return hex;
            });
        var map = BattleMap.GenerateMap(10, 10,generator);
        _battleState = new BattleState(map);
    }

    [Fact]
    public void TryDeployUnit_ShouldDeployUnit_WhenValid()
    {
        // Arrange
        var unit = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
        var coordinates = new HexCoordinates(1, 1);

        // Act
        var result = _battleState.TryDeployUnit(unit, coordinates);

        // Assert
        result.Should().BeTrue();
        unit.IsDeployed.Should().BeTrue();
    }

    [Fact]
    public void TryMoveUnit_ShouldMoveUnit_WhenValid()
    {
        // Arrange
        var unit = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
        var coordinates = new HexCoordinates(1, 1);
        _battleState.TryDeployUnit(unit, coordinates);

        // Act
        var newCoordinates = new HexCoordinates(2, 2);
        var result = _battleState.TryMoveUnit(unit, newCoordinates);

        // Assert
        result.Should().BeTrue();
        unit.Position.Should().Be(newCoordinates);
    }

    [Fact]
    public void HasLineOfSight_ShouldReturnTrue_WhenUnitIsInTheLastHex()
    {
        // Arrange
        var attacker = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
        var target = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
        var coordinates1 = new HexCoordinates(1, 1);
        var coordinates2 = new HexCoordinates(3, 3);
        _battleState.TryDeployUnit(attacker, coordinates1);
        _battleState.TryDeployUnit(target, coordinates2);

        // Act
        var result = _battleState.HasLineOfSight(attacker, target);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FindPath_ShouldReturnPath_WhenValid()
    {
        // Arrange
        var unit = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
        var startCoordinates = new HexCoordinates(1, 1);
        var targetCoordinates = new HexCoordinates(2, 2);
        var isDeployed = _battleState.TryDeployUnit(unit, startCoordinates);
        isDeployed.Should().BeTrue();

        // Act
        var path = _battleState.FindPath(unit, targetCoordinates);

        // Assert
        path.Should().NotBeNull();
        path.Should().Contain(targetCoordinates);
    }
}