using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class TurnOrderTests
{
    private readonly TurnOrder _sut;
    private readonly IPlayer _player1;
    private readonly IPlayer _player2;
    private readonly IPlayer _player3;
    private readonly UnitData _unitData = MechFactoryTests.CreateDummyMechData();
    private readonly MechFactory _mechFactory = new MechFactory(new ClassicBattletechRulesProvider());
    
    public TurnOrderTests()
    {
        _sut = new TurnOrder();
        _player1 = Substitute.For<IPlayer>();
        _player2 = Substitute.For<IPlayer>();
        _player3 = Substitute.For<IPlayer>();

        // Setup unit counts
        
        _player1.Units.Returns([_mechFactory.Create(_unitData), _mechFactory.Create(_unitData)]); // 2 units
        _player2.Units.Returns([_mechFactory.Create(_unitData), _mechFactory.Create(_unitData), _mechFactory.Create(_unitData)]); // 3 units
        _player3.Units.Returns([_mechFactory.Create(_unitData), _mechFactory.Create(_unitData), _mechFactory.Create(_unitData)]); // 3 units
    }

    [Fact]
    public void CalculateOrder_WithUnequalUnits_ShouldHandleDoubleMovements()
    {
        // Arrange - Player2 won, Player1 second, Player3 lost
        var initiativeOrder = new List<IPlayer> { _player2, _player1, _player3 };

        // Act
        _sut.CalculateOrder(initiativeOrder);

        // Assert - Following the example from requirements
        var steps = _sut.Steps;
        steps.Should().HaveCount(6);
        
        // 1. Player 3 moves one unit (2 remains)
        steps[0].Should().Be(new TurnStep(_player3, 1));
        
        // 2. Player 1 moves one unit (1 remains)
        steps[1].Should().Be(new TurnStep(_player1, 1));
        
        // 3. Player 2 moves one unit (2 remains)
        steps[2].Should().Be(new TurnStep(_player2, 1));
        
        // 4. Player 3 moves 2 units (has twice as many as Player 1)
        steps[3].Should().Be(new TurnStep(_player3, 2));
        
        // 5. Player 1 moves last unit
        steps[4].Should().Be(new TurnStep(_player1, 1));
        
        // 6. Player 2 moves 2 units
        steps[5].Should().Be(new TurnStep(_player2, 2));
    }

    [Fact]
    public void CalculateOrder_WithEqualUnits_ShouldMoveOneByOne()
    {
        // Arrange
        _player1.Units.Returns([_mechFactory.Create(_unitData), _mechFactory.Create(_unitData)]); // 2 units
        _player2.Units.Returns([_mechFactory.Create(_unitData), _mechFactory.Create(_unitData)]); // 2 units
        var initiativeOrder = new List<IPlayer> { _player2, _player1 };

        // Act
        _sut.CalculateOrder(initiativeOrder);

        // Assert
        var steps = _sut.Steps;
        steps.Should().HaveCount(4);
        
        // Loser moves first, one unit at a time
        steps[0].Should().Be(new TurnStep(_player1, 1));
        steps[1].Should().Be(new TurnStep(_player2, 1));
        steps[2].Should().Be(new TurnStep(_player1, 1));
        steps[3].Should().Be(new TurnStep(_player2, 1));
    }

    [Fact]
    public void GetNextStep_ShouldTrackProgress()
    {
        // Arrange
        var initiativeOrder = new List<IPlayer> { _player1, _player2 };
        _sut.CalculateOrder(initiativeOrder);

        // Act & Assert
        _sut.CurrentStep.Should().BeNull();
        
        var step1 = _sut.GetNextStep();
        step1.Should().NotBeNull();
        _sut.CurrentStep.Should().Be(step1);
        
        var step2 = _sut.GetNextStep();
        step2.Should().NotBeNull();
        _sut.CurrentStep.Should().Be(step2);
    }

    [Fact]
    public void Reset_ShouldClearCurrentStep()
    {
        // Arrange
        var initiativeOrder = new List<IPlayer> { _player1, _player2 };
        _sut.CalculateOrder(initiativeOrder);
        _sut.GetNextStep(); // Advance to first step

        // Act
        _sut.Reset();

        // Assert
        _sut.CurrentStep.Should().BeNull();
        var nextStep = _sut.GetNextStep();
        nextStep.Should().Be(_sut.Steps[0]);
    }
}
