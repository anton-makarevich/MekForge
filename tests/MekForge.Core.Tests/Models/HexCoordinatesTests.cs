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

    [Theory]
    [InlineData(0, 0, 0, 0, 0)]  // Same hex
    [InlineData(0, 0, 1, 0, 1)]  // Adjacent hex (East)
    [InlineData(0, 0, 1, -1, 1)] // Adjacent hex (Northeast)
    [InlineData(0, 0, 0, -1, 1)] // Adjacent hex (Northwest)
    [InlineData(0, 0, -1, 0, 1)] // Adjacent hex (West)
    [InlineData(0, 0, -1, 1, 1)] // Adjacent hex (Southwest)
    [InlineData(0, 0, 0, 1, 1)]  // Adjacent hex (Southeast)
    [InlineData(0, 0, 2, 0, 2)]  // Two hexes away (straight line)
    [InlineData(0, 0, 2, -2, 2)] // Two hexes away (diagonal)
    [InlineData(0, 0, 3, -1, 3)] // Three hexes away
    [InlineData(-2, 1, 2, -1, 4)] // Random longer distance
    [InlineData(0, 0, 1, 0, 1)]  // Adjacent east
    [InlineData(0, 0, 2, 0, 2)]  // Two hexes east
    [InlineData(0, 0, 1, -1, 1)] // Adjacent northeast
    [InlineData(0, 0, -2, 2, 2)] // Two hexes southwest
    [InlineData(3, -1, -2, 2, 5)] // Longer distance
    public void DistanceTo_ReturnsCorrectDistance(int q1, int r1, int q2, int r2, int expectedDistance)
    {
        // Arrange
        var hex1 = new HexCoordinates(q1, r1);
        var hex2 = new HexCoordinates(q2, r2);

        // Act
        var distance = hex1.DistanceTo(hex2);

        // Assert
        distance.Should().Be(expectedDistance);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        // Arrange
        var hex1 = new HexCoordinates(2, -1);
        var hex2 = new HexCoordinates(-1, 2);

        // Act
        var distance1to2 = hex1.DistanceTo(hex2);
        var distance2to1 = hex2.DistanceTo(hex1);

        // Assert
        distance1to2.Should().Be(distance2to1);
    }

    [Fact]
    public void GetAdjacentCoordinates_ReturnsAllSixNeighbors()
    {
        // Arrange
        var center = new HexCoordinates(0, 0);

        // Act
        var neighbors = center.GetAdjacentCoordinates().ToList();

        // Assert
        neighbors.Should().HaveCount(6);
        neighbors.Should().Contain(new HexCoordinates(1, 0));   // East
        neighbors.Should().Contain(new HexCoordinates(1, -1));  // Northeast
        neighbors.Should().Contain(new HexCoordinates(0, -1));  // Northwest
        neighbors.Should().Contain(new HexCoordinates(-1, 0));  // West
        neighbors.Should().Contain(new HexCoordinates(-1, 1));  // Southwest
        neighbors.Should().Contain(new HexCoordinates(0, 1));   // Southeast
    }

    [Fact]
    public void GetAdjacentCoordinates_AllNeighborsAreDistanceOne()
    {
        // Arrange
        var center = new HexCoordinates(2, 3); // Using non-zero coordinates

        // Act
        var neighbors = center.GetAdjacentCoordinates();

        // Assert
        foreach (var neighbor in neighbors)
        {
            center.DistanceTo(neighbor).Should().Be(1);
        }
    }
}
