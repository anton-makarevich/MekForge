using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Players;

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
        orderedPlayers.ShouldBe([_player2, _player3, _player1]);
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
        orderedPlayers.ShouldBe([_player3, _player1, _player4, _player2]);
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
        _sut.GetOrderedPlayers().ShouldBeEmpty();
        _sut.HasPlayer(_player1).ShouldBeFalse();
        _sut.HasPlayer(_player2).ShouldBeFalse();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(8)]
    public void HasPlayer_WhenPlayerAdded_ShouldReturnTrue(int roll)
    {
        // Arrange
        _sut.AddResult(_player1, roll);

        // Act & Assert
        _sut.HasPlayer(_player1).ShouldBeTrue();
    }

    [Fact]
    public void HasPlayer_WhenPlayerNotAdded_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 5);

        // Act & Assert
        _sut.HasPlayer(_player2).ShouldBeFalse();
    }

    [Fact]
    public void HasTies_WhenNoTies_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 5);
        _sut.AddResult(_player2, 8);
        _sut.AddResult(_player3, 6);

        // Act & Assert
        _sut.HasTies().ShouldBeFalse();
    }

    [Fact]
    public void HasTies_WhenTiesExist_ShouldReturnTrue()
    {
        // Arrange
        _sut.AddResult(_player1, 7);
        _sut.AddResult(_player2, 7);
        _sut.AddResult(_player3, 6);

        // Act & Assert
        _sut.HasTies().ShouldBeTrue();
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
        _sut.HasTies().ShouldBeFalse();
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
        tiedPlayers.ShouldBeEmpty();
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
        tiedPlayers.Count.ShouldBe(2);
        tiedPlayers.ShouldContain(_player1);
        tiedPlayers.ShouldContain(_player2);
        tiedPlayers.ShouldNotContain(_player3);
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
        tiedPlayers.Count.ShouldBe(2);
        tiedPlayers.ShouldContain(_player1);
        tiedPlayers.ShouldContain(_player2);
    }

    [Fact]
    public void GetTiedPlayers_WhenMultipleTies_ShouldReturnAllTiedPlayers()
    {
        // Arrange
        _sut.AddResult(_player1, 4); // Tied with player 4
        _sut.AddResult(_player2, 6);
        _sut.AddResult(_player3, 8);
        _sut.AddResult(_player4, 4); // Tied with player 1

        // Act
        var tiedPlayers = _sut.GetTiedPlayers();

        // Assert
        tiedPlayers.Count.ShouldBe(2);
        tiedPlayers.ShouldContain(_player1);
        tiedPlayers.ShouldContain(_player4);
    }

    [Fact]
    public void GetOrderedPlayers_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act & Assert
        _sut.GetOrderedPlayers().ShouldBeEmpty();
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
        orderedPlayers.ShouldBe([_player1, _player3, _player2]);
    }

    [Fact]
    public void HasPlayerRolledInCurrentRound_WhenPlayerHasNotRolled_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 7);
        _sut.StartNewRoll(); // Move to round 2

        // Act & Assert
        _sut.HasPlayerRolledInCurrentRound(_player1).ShouldBeFalse();
    }

    [Fact]
    public void HasPlayerRolledInCurrentRound_WhenPlayerHasRolledInCurrentRound_ShouldReturnTrue()
    {
        // Arrange
        _sut.AddResult(_player1, 7);

        // Act & Assert
        _sut.HasPlayerRolledInCurrentRound(_player1).ShouldBeTrue();
    }

    [Fact]
    public void HasPlayerRolledInCurrentRound_WhenPlayerHasRolledInPreviousRound_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 7); // Round 1
        _sut.StartNewRoll();        // Move to round 2
        _sut.AddResult(_player2, 8); // Round 2

        // Act & Assert
        _sut.HasPlayerRolledInCurrentRound(_player1).ShouldBeFalse();
        _sut.HasPlayerRolledInCurrentRound(_player2).ShouldBeTrue();
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
        _sut.HasPlayerRolledInCurrentRound(_player1).ShouldBeTrue();
        _sut.HasPlayerRolledInCurrentRound(_player2).ShouldBeFalse();
    }
}