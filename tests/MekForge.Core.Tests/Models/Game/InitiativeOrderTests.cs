using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class InitiativeOrderTests
{
    private readonly InitiativeOrder _sut = new();
    private readonly IPlayer _player1 = Substitute.For<IPlayer>();
    private readonly IPlayer _player2 = Substitute.For<IPlayer>();
    private readonly IPlayer _player3 = Substitute.For<IPlayer>();
    private readonly IPlayer _player4 = Substitute.For<IPlayer>();

    [Fact]
    public void AddResult_ShouldSortResultsDescending()
    {
        // Arrange & Act
        _sut.AddResult(_player1, 5);
        _sut.AddResult(_player2, 8);
        _sut.AddResult(_player3, 6);

        // Assert
        var orderedPlayers = _sut.GetOrderedPlayers();
        orderedPlayers.Should().ContainInOrder(_player2, _player3, _player1);
    }

    [Fact]
    public void AddResult_WhenRerolling_ShouldMaintainFirstRollPrecedence()
    {
        // Arrange
        _sut.AddResult(_player1, 7);  // First roll
        _sut.AddResult(_player2, 4);  // First roll
        _sut.AddResult(_player3, 10); // First roll - Winner
        _sut.AddResult(_player4, 7);  // First roll

        _sut.StartNewRoll(); // Start second roll

        // Act
        _sut.AddResult(_player1, 11); // Second roll - Higher than winner but doesn't matter
        _sut.AddResult(_player4, 3);  // Second roll

        // Assert
        var orderedPlayers = _sut.GetOrderedPlayers();
        orderedPlayers.Should().ContainInOrder(_player3, _player1, _player4, _player2);
    }

    [Fact]
    public void Clear_ShouldRemoveAllResults()
    {
        // Arrange
        _sut.AddResult(_player1, 5);
        _sut.AddResult(_player2, 8);

        // Act
        _sut.Clear();

        // Assert
        _sut.GetOrderedPlayers().Should().BeEmpty();
        _sut.HasPlayer(_player1).Should().BeFalse();
        _sut.HasPlayer(_player2).Should().BeFalse();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(8)]
    public void HasPlayer_WhenPlayerAdded_ShouldReturnTrue(int roll)
    {
        // Arrange
        _sut.AddResult(_player1, roll);

        // Act & Assert
        _sut.HasPlayer(_player1).Should().BeTrue();
    }

    [Fact]
    public void HasPlayer_WhenPlayerNotAdded_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 5);

        // Act & Assert
        _sut.HasPlayer(_player2).Should().BeFalse();
    }

    [Fact]
    public void HasTies_WhenNoTies_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 5);
        _sut.AddResult(_player2, 8);
        _sut.AddResult(_player3, 6);

        // Act & Assert
        _sut.HasTies().Should().BeFalse();
    }

    [Fact]
    public void HasTies_WhenTiesExist_ShouldReturnTrue()
    {
        // Arrange
        _sut.AddResult(_player1, 7);
        _sut.AddResult(_player2, 7);
        _sut.AddResult(_player3, 6);

        // Act & Assert
        _sut.HasTies().Should().BeTrue();
    }

    [Fact]
    public void HasTies_WhenTiesResolvedInSecondRoll_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 7);
        _sut.AddResult(_player2, 7);
        
        _sut.StartNewRoll();
        _sut.AddResult(_player1, 8);
        _sut.AddResult(_player2, 6);

        // Act & Assert
        _sut.HasTies().Should().BeFalse();
    }

    [Fact]
    public void GetTiedPlayers_WhenNoTies_ShouldReturnEmptyList()
    {
        // Arrange
        _sut.AddResult(_player1, 5);
        _sut.AddResult(_player2, 8);
        _sut.AddResult(_player3, 6);

        // Act
        var tiedPlayers = _sut.GetTiedPlayers();

        // Assert
        tiedPlayers.Should().BeEmpty();
    }

    [Fact]
    public void GetTiedPlayers_WhenTiesExist_ShouldReturnTiedPlayers()
    {
        // Arrange
        _sut.AddResult(_player1, 7);
        _sut.AddResult(_player2, 7);
        _sut.AddResult(_player3, 6);

        // Act
        var tiedPlayers = _sut.GetTiedPlayers();

        // Assert
        tiedPlayers.Should().HaveCount(2);
        tiedPlayers.Should().Contain(_player1);
        tiedPlayers.Should().Contain(_player2);
        tiedPlayers.Should().NotContain(_player3);
    }

    [Fact]
    public void GetTiedPlayers_WhenMultipleRollsAndTies_ShouldOnlyConsiderCurrentRoll()
    {
        // Arrange
        _sut.AddResult(_player1, 7);
        _sut.AddResult(_player2, 7);
        _sut.AddResult(_player3, 7);
        
        _sut.StartNewRoll();
        _sut.AddResult(_player1, 6);
        _sut.AddResult(_player2, 6);

        // Act
        var tiedPlayers = _sut.GetTiedPlayers();

        // Assert
        tiedPlayers.Should().HaveCount(2);
        tiedPlayers.Should().Contain(_player1);
        tiedPlayers.Should().Contain(_player2);
    }

    [Fact]
    public void GetOrderedPlayers_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act & Assert
        _sut.GetOrderedPlayers().Should().BeEmpty();
    }

    [Fact]
    public void GetOrderedPlayers_ShouldMaintainOrderAfterMultipleRolls()
    {
        // Arrange
        _sut.AddResult(_player1, 7);  // Ties for first
        _sut.AddResult(_player2, 4);  // Last
        _sut.AddResult(_player3, 7);  // Ties for first
        
        _sut.StartNewRoll();
        _sut.AddResult(_player1, 8);  // Wins tie
        _sut.AddResult(_player3, 6);  // Loses tie

        // Assert
        var orderedPlayers = _sut.GetOrderedPlayers();
        orderedPlayers.Should().ContainInOrder(_player1, _player3, _player2);
    }

    [Fact]
    public void HasPlayerRolledInCurrentRound_WhenPlayerHasNotRolled_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 7);
        _sut.StartNewRoll(); // Move to round 2

        // Act & Assert
        _sut.HasPlayerRolledInCurrentRound(_player1).Should().BeFalse();
    }

    [Fact]
    public void HasPlayerRolledInCurrentRound_WhenPlayerHasRolledInCurrentRound_ShouldReturnTrue()
    {
        // Arrange
        _sut.AddResult(_player1, 7);

        // Act & Assert
        _sut.HasPlayerRolledInCurrentRound(_player1).Should().BeTrue();
    }

    [Fact]
    public void HasPlayerRolledInCurrentRound_WhenPlayerHasRolledInPreviousRound_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 7); // Round 1
        _sut.StartNewRoll();        // Move to round 2
        _sut.AddResult(_player2, 8); // Round 2

        // Act & Assert
        _sut.HasPlayerRolledInCurrentRound(_player1).Should().BeFalse();
        _sut.HasPlayerRolledInCurrentRound(_player2).Should().BeTrue();
    }

    [Fact]
    public void HasPlayerRolledInCurrentRound_WhenPlayerHasRolledInMultipleRounds_ShouldCheckCurrentRound()
    {
        // Arrange
        _sut.AddResult(_player1, 7); // Round 1
        _sut.AddResult(_player2, 7); // Round 1
        
        _sut.StartNewRoll();        // Move to round 2
        _sut.AddResult(_player1, 8); // Round 2
        
        // Act & Assert
        _sut.HasPlayerRolledInCurrentRound(_player1).Should().BeTrue();
        _sut.HasPlayerRolledInCurrentRound(_player2).Should().BeFalse();
    }
}