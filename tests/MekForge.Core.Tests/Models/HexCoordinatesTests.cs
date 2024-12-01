using FluentAssertions;
using Sanet.MekForge.Core.Models;

namespace MekForge.Core.Tests.Models;

public class HexCoordinatesTests
{
    [Fact]
    public void Constructor_SetsQAndR()
    {
        // Arrange & Act
        var coords = new HexCoordinates(2, 3);

        // Assert
        coords.Q.Should().Be(2);
        coords.R.Should().Be(3);
    }

    [Fact]
    public void S_CalculatesCorrectly()
    {
        // Arrange
        var coords = new HexCoordinates(2, 3);

        // Act
        var s = coords.S;

        // Assert
        s.Should().Be(-5); // -Q - R = -2 - 3 = -5
    }

    [Fact]
    public void GetAdjacentCoordinates_ReturnsAllSixDirections()
    {
        // Arrange
        var coords = new HexCoordinates(0, 0);

        // Act
        var adjacent = coords.GetAdjacentCoordinates().ToList();

        // Assert
        adjacent.Should().HaveCount(6);
        adjacent.Should().Contain(new HexCoordinates(1, 0));   // East
        adjacent.Should().Contain(new HexCoordinates(1, -1));  // Northeast
        adjacent.Should().Contain(new HexCoordinates(0, -1));  // Northwest
        adjacent.Should().Contain(new HexCoordinates(-1, 0));  // West
        adjacent.Should().Contain(new HexCoordinates(-1, 1));  // Southwest
        adjacent.Should().Contain(new HexCoordinates(0, 1));   // Southeast
    }

    [Theory]
    [InlineData(0, 0, 1, 0, 1)]  // Adjacent east
    [InlineData(0, 0, 2, 0, 2)]  // Two hexes east
    [InlineData(0, 0, 1, -1, 1)] // Adjacent northeast
    [InlineData(0, 0, -2, 2, 2)] // Two hexes southwest
    [InlineData(3, -1, -2, 2, 5)] // Longer distance
    public void DistanceTo_CalculatesCorrectly(int fromQ, int fromR, int toQ, int toR, int expectedDistance)
    {
        // Arrange
        var from = new HexCoordinates(fromQ, fromR);
        var to = new HexCoordinates(toQ, toR);

        // Act
        var distance = from.DistanceTo(to);

        // Assert
        distance.Should().Be(expectedDistance);
    }
}
