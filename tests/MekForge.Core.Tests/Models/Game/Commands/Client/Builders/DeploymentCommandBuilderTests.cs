using FluentAssertions;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client.Builders;

public class DeploymentCommandBuilderTests
{
    private readonly DeploymentCommandBuilder _builder;
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Guid _playerId = Guid.NewGuid();
    private readonly Unit _unit;
    private readonly HexCoordinates _coordinates;
    
    public DeploymentCommandBuilderTests()
    {
        _builder = new DeploymentCommandBuilder(_gameId, _playerId);
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
    public void CanBuild_ReturnsFalse_WhenOnlyPositionSet()
    {
        // Arrange
        _builder.SetPosition(_coordinates);
        
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
    public void CanBuild_ReturnsTrue_WhenAllDataSet()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetPosition(_coordinates);
        _builder.SetDirection(HexDirection.Top);
        
        // Act & Assert
        _builder.CanBuild.Should().BeTrue();
    }
    
    [Fact]
    public void Build_ReturnsNull_WhenCanBuildIsFalse()
    {
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
        _builder.SetPosition(_coordinates);
        _builder.SetDirection(HexDirection.Top);
        
        // Act
        var result = _builder.Build();
        
        // Assert
        result.Should().NotBeNull();
        result!.GameOriginId.Should().Be(_gameId);
        result.PlayerId.Should().Be(_playerId);
        result.UnitId.Should().Be(_unit.Id);
        result.Position.Q.Should().Be(_coordinates.Q);
        result.Position.R.Should().Be(_coordinates.R);
        result.Direction.Should().Be((int)HexDirection.Top);
    }
    
    [Fact]
    public void Reset_ClearsAllData()
    {
        // Arrange
        _builder.SetUnit(_unit);
        _builder.SetPosition(_coordinates);
        _builder.SetDirection(HexDirection.Top);
        
        // Act
        _builder.Reset();
        
        // Assert
        _builder.CanBuild.Should().BeFalse();
        _builder.Build().Should().BeNull();
    }
}
