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
    public void GetTiedPlayers_WhenNoResults_ShouldReturnEmptyList()
    {
        // Act & Assert
        _sut.GetTiedPlayers().Should().BeEmpty();
    }

    [Fact]
    public void HasTies_WhenNoResults_ShouldReturnFalse()
    {
        // Act & Assert
        _sut.HasTies().Should().BeFalse();
    }

    [Fact]
    public void HasTies_WhenSingleResult_ShouldReturnFalse()
    {
        // Arrange
        _sut.AddResult(_player1, 7);

        // Act & Assert
        _sut.HasTies().Should().BeFalse();
    }

    [Fact]
    public void GetOrderedPlayers_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act & Assert
        _sut.GetOrderedPlayers().Should().BeEmpty();
    }

    [Fact]
    public void GetOrderedPlayers_ShouldMaintainOrderAfterMultipleAdditions()
    {
        // Arrange & Act
        _sut.AddResult(_player1, 5); // Will end up last
        _sut.AddResult(_player2, 9); // Will be first
        _sut.AddResult(_player3, 7); // Will be second

        // Assert
        var orderedPlayers = _sut.GetOrderedPlayers();
        orderedPlayers.Should().ContainInOrder(_player2, _player3, _player1);
    }
}
