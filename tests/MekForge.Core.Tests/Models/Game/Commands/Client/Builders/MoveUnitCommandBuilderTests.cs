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
    public void CanBuild_ReturnsFalse_WhenOnlyDirectionSet()
    {
        // Arrange
        _builder.SetDirection(HexDirection.Top);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenMissingMovementType()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetDestination(_coordinates);
        _builder.SetDirection(HexDirection.Top);
        
        // Act & Assert
        _builder.CanBuild.Should().BeFalse();
    }
    
    [Fact]
    public void CanBuild_ReturnsFalse_WhenMissingDestination()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDirection(HexDirection.Top);
        
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
        _builder.SetDirection(HexDirection.Top);
        
        // Act & Assert
        _builder.CanBuild.Should().BeTrue();
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
        _builder.SetDirection(HexDirection.Top);
        
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Build_ReturnsCommand_WithCorrectData_WhenAllDataSet()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDestination(_coordinates);
        _builder.SetDirection(HexDirection.Top);
        
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
        result.Direction.Should().Be((int)HexDirection.Top);
    }
    
    [Fact]
    public void Reset_ClearsAllData()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetMovementType(MovementType.Walk);
        _builder.SetDestination(_coordinates);
        _builder.SetDirection(HexDirection.Top);
        
        // Act
        _builder.Reset();
        
        // Assert
        _builder.CanBuild.Should().BeFalse();
    }
}