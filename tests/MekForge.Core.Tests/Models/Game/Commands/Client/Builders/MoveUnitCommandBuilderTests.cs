using FluentAssertions;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client.Builders;

public class MoveUnitCommandBuilderTests
{
    private readonly MoveUnitCommandBuilder _builder;
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Guid _playerId;
    private readonly Unit _unit;
    private readonly HexCoordinates _coordinates;
    
    public MoveUnitCommandBuilderTests()
    {
        _playerId = Guid.NewGuid();
        _builder = new MoveUnitCommandBuilder(_gameId, _playerId);
        _unit = new MechFactory(new ClassicBattletechRulesProvider()).Create(MechFactoryTests.CreateDummyMechData());
        _coordinates = new HexCoordinates(1, 1);
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenNoDataSet()
    {
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenOnlyUnitSet()
    {
        // Arrange
        _builder.SetUnit(_unit);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenOnlyMovementTypeSet()
    {
        // Arrange
        _builder.SetMovementType(MovementType.Walk);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenOnlyDestinationSet()
    {
        // Arrange
        _builder.SetDestination(_coordinates);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenOnlyMovementPathSet()
    {
        // Arrange
        _builder.MovementPath([]);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenMissingMovementType()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetDestination(_coordinates);
        _builder.MovementPath([new PathSegment(new HexPosition(1,1,HexDirection.Bottom),
            new HexPosition(1,2,HexDirection.Bottom), 1)]);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenMissingDestination()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.MovementPath([new PathSegment(new HexPosition(1,1,HexDirection.Bottom),
            new HexPosition(1,2,HexDirection.Bottom), 1)]);  
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenMissingDirection()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDestination(_coordinates);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsTrue_WhenAllDataSet()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDestination(_coordinates);
        _builder.MovementPath([new PathSegment(
            new HexPosition(1,1,HexDirection.Bottom),
            new HexPosition(1,2,HexDirection.Bottom), 
            1)]);
        
        // Act & Assert
        _builder.CanBuild.Should().BeTrue();
    }
    
    [Fact]
    public void Build_ReturnsCommand()
    {
        // Arrange
        var startPos = new HexPosition(1, 1, HexDirection.Top);
        var endPos = new HexPosition(1, 2, HexDirection.Top);
        var pathSegment = new PathSegment(startPos, endPos, 1);
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDestination(_coordinates);
        _builder.MovementPath([pathSegment]);
        
        // Act 
        var command = _builder.Build();
        
        // Assert
        command.Should().NotBeNull();
        command.Direction.Should().Be((int)HexDirection.Top);
        command.Destination.Q.Should().Be(_coordinates.Q);
        command.Destination.R.Should().Be(_coordinates.R);
        command.GameOriginId.Should().Be(_gameId);
        command.MovementType.Should().Be(MovementType.Walk);
        command.PlayerId.Should().Be(_playerId);
        command.UnitId.Should().Be(_unit.Id);
        command.MovementPoints.Should().Be(1);
        // command.PathSegments.Should().HaveCount(1);
        // command.PathSegments[0].Cost.Should().Be(1);
        // command.PathSegments[0].From.Should().BeEquivalentTo(startPos.Coordinates.ToData());
        // command.PathSegments[0].To.Should().BeEquivalentTo(endPos.Coordinates.ToData());
    }
    
    [Fact]
    public void Build_ReturnsNull_WhenNoDataSet()
    {
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Build_ReturnsNull_WhenOnlyUnitSet()
    {
        // Arrange
        _builder.SetUnit(_unit);
        
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Build_ReturnsNull_WhenOnlyMovementTypeSet()
    {
        // Arrange
        _builder.SetMovementType(MovementType.Walk);
        
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Build_ReturnsNull_WhenOnlyTargetPositionSet()
    {
        // Arrange
        _builder.SetDestination(_coordinates);
        
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Build_ReturnsNull_WhenOnlyDirectionSet()
    {
        // Arrange
        _builder.MovementPath([new PathSegment(new HexPosition(1,1,HexDirection.Bottom),
        new HexPosition(1,2,HexDirection.Bottom), 2)]);
        
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Build_ReturnsCommand_WithCorrectData_WhenAllDataSet()
    {
        // Arrange
        var startPos = new HexPosition(1, 1, HexDirection.Top);
        var midPos = new HexPosition(1, 1, HexDirection.TopRight);
        var endPos = new HexPosition(1, 2, HexDirection.TopRight);
        var turnSegment = new PathSegment(startPos, midPos, 1);
        var moveSegment = new PathSegment(midPos, endPos, 1);
        
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDestination(_coordinates);
        _builder.MovementPath([turnSegment, moveSegment]);
        
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().NotBeNull();
        result!.GameOriginId.Should().Be(_gameId);
        result.PlayerId.Should().Be(_playerId);
        result.UnitId.Should().Be(_unit.Id);
        result.MovementType.Should().Be(MovementType.Walk);
        result.Destination.Q.Should().Be(_coordinates.Q);
        result.Destination.R.Should().Be(_coordinates.R);
        result.Direction.Should().Be((int)HexDirection.TopRight);
        result.MovementPoints.Should().Be(2);
        // result.PathSegments.Should().HaveCount(2);
        // result.PathSegments[0].Cost.Should().Be(1);
        // result.PathSegments[1].Cost.Should().Be(1);
    }
    
    [Fact]
    public void Reset_ClearsAllData()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDestination(_coordinates);
        _builder.MovementPath([new PathSegment(new HexPosition(1,1,HexDirection.Bottom),
        new HexPosition(1,2,HexDirection.Bottom),2)]);
        
        // Act
        _builder.Reset();
        
        // Assert
        _builder.CanBuild.Should().BeFalse();
    }
}
