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
    
    [Fact]
    public void CubeCoordinates_CalculateCorrectly()
    {
        // Arrange
        var hexEven = new HexCoordinates(2, 3); // Even column
        var hexOdd = new HexCoordinates(1, 2);  // Odd column

        // Assert
        hexEven.X.Should().Be(2);
        hexEven.Z.Should().Be(2); // Calculation: R - (Q + (Q % 2)) / 2 = 3 - (2 + 0) / 2
        hexEven.Y.Should().Be(-4); // Calculation: -X - Z = -2 - 2

        hexOdd.X.Should().Be(1);
        hexOdd.Z.Should().Be(1); // Calculation: R - (Q + (Q % 2)) / 2 = 2 - (1 + 1) / 2
        hexOdd.Y.Should().Be(-2); // Calculation: -X - Z = -1 - 1
    }

    [Theory]
    [InlineData(1, 1, 1, 2, 1)] // Adjacent hex
    [InlineData(1, 1, 2, 1, 1)] // Same row but shifted
    [InlineData(1, 1, 4, 4, 5)] // Larger distance
    [InlineData(1, 1, 1, 1, 0)] // Same hex
    [InlineData(2, 2, 4, 2, 2)] // Horizontal line on even row
    [InlineData(1, 1, 3, 3, 3)]
    [InlineData(2, 2, 2, 5, 3)]
    [InlineData(1, 1, 5, 5, 6)]
    [InlineData(5, 5, 1, 1, 6)]
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
        var center = new HexCoordinates(2, 2);

        // Act
        var neighbors = center.GetAdjacentCoordinates().ToList();

        // Assert
        neighbors.Should().HaveCount(6);
        neighbors.Should().Contain(new HexCoordinates(1, 2));   // East
        neighbors.Should().Contain(new HexCoordinates(1, 3));  // Northeast
        neighbors.Should().Contain(new HexCoordinates(2, 1));  // Northwest
        neighbors.Should().Contain(new HexCoordinates(2, 3));  // West
        neighbors.Should().Contain(new HexCoordinates(3, 2));  // Southwest
        neighbors.Should().Contain(new HexCoordinates(3, 3));   // Southeast
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
    [InlineData(2, 2, 1)]  // Range 1 from origin
    [InlineData(3, 3, 1)]  // Range 1 from origin
    [InlineData(2, 2, 2)]  // Range 2 from origin
    [InlineData(3, 3, 2)]  // Range 2 from origin
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
        hex1.H.Should().Be(0);
        hex2.H.Should().Be(75); // 100 * 0.75
        hex3.H.Should().Be(150); // 200 * 0.75
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
        hex1.V.Should().Be(0);
        hex2.V.Should().Be(HexCoordinates.HexHeight);
        hex3.V.Should().Be(HexCoordinates.HexHeight*0.5);  // Offset for odd Q
        hex4.V.Should().Be(HexCoordinates.HexHeight*1.5);  // Height + 0.5*Height offset for odd Q
    }
    
    [Fact]
    public void GetHexesAlongLine_ShouldReturnHexes_WhenClearPath()
    {
        // Arrange
        var start = new HexCoordinates(1, 1);
        var end = new HexCoordinates(3, 3);

        // Act
        var hexes = start.LineTo(end);

        // Assert
        hexes.Should().NotBeNull();
        hexes.Count.Should().Be(4);
        hexes.Should().ContainInOrder(new HexCoordinates(1, 1), new HexCoordinates(2,1), new HexCoordinates(2, 2), new HexCoordinates(3, 3));
    }

    [Fact]
    public void GetHexesAlongLine_ShouldHandleSameHex()
    {
        // Arrange
        var coordinates = new HexCoordinates(2, 2);
        
        // Act
        var hexes = coordinates.LineTo(coordinates);

        // Assert
        hexes.Should().NotBeNull();
        hexes.Count.Should().Be(1);
        hexes.Should().ContainSingle().Which.Should().Be(coordinates);
    }
}
