using FluentAssertions;
using Sanet.MekForge.Core.Models;

namespace Sanet.MekForge.Core.Tests.Models;

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
        var distance1To2 = hex1.DistanceTo(hex2);
        var distance2To1 = hex2.DistanceTo(hex1);

        // Assert
        distance1To2.Should().Be(distance2To1);
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

    [Theory]
    [InlineData(0, 0, 1)]  // Range 1 from origin
    [InlineData(0, 0, 2)]  // Range 2 from origin
    [InlineData(1, -1, 1)] // Range 1 from non-origin
    public void GetCoordinatesInRange_ReturnsCorrectHexes(int centerQ, int centerR, int range)
    {
        // Arrange
        var center = new HexCoordinates(centerQ, centerR);

        // Act
        var hexesInRange = center.GetCoordinatesInRange(range).ToList();

        // Assert
        foreach (var hex in hexesInRange)
        {
            center.DistanceTo(hex).Should().BeLessThanOrEqualTo(range);
        }

        // Verify that all hexes at exactly range distance are included
        var hexesAtRange = hexesInRange.Where(h => center.DistanceTo(h) == range);
        hexesAtRange.Count().Should().Be(6 * range); // Each range adds 6 more hexes
    }

    [Fact]
    public void X_CalculatesCorrectPixelPosition()
    {
        // Arrange & Act
        var hex1 = new HexCoordinates(0, 0);
        var hex2 = new HexCoordinates(1, 0);
        var hex3 = new HexCoordinates(2, 0);

        // Assert
        hex1.X.Should().Be(0);
        hex2.X.Should().Be(75); // 100 * 0.75
        hex3.X.Should().Be(150); // 200 * 0.75
    }

    [Fact]
    public void Y_CalculatesCorrectPixelPosition()
    {
        // Arrange & Act
        var hex1 = new HexCoordinates(0, 0); // Even Q
        var hex2 = new HexCoordinates(0, 1); // Even Q
        var hex3 = new HexCoordinates(1, 0); // Odd Q
        var hex4 = new HexCoordinates(1, 1); // Odd Q

        // Assert
        hex1.Y.Should().Be(0);
        hex2.Y.Should().Be(HexCoordinates.HexHeight);
        hex3.Y.Should().Be(HexCoordinates.HexHeight*0.5);  // Offset for odd Q
        hex4.Y.Should().Be(HexCoordinates.HexHeight*1.5);  // Height + 0.5*Height offset for odd Q
    }
}
