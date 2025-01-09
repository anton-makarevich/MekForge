using FluentAssertions;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Tests.Models.Map;

public class HexPositionTests
{
    [Fact]
    public void Constructor_WithCoordinatesAndFacing_SetsProperties()
    {
        // Arrange
        var coordinates = new HexCoordinates(2, 3);
        var facing = HexDirection.Bottom;

        // Act
        var position = new HexPosition(coordinates, facing);

        // Assert
        position.Coordinates.Should().Be(coordinates);
        position.Facing.Should().Be(facing);
    }

    [Fact]
    public void Constructor_WithQRAndFacing_SetsProperties()
    {
        // Arrange
        const int q = 2;
        const int r = 3;
        var facing = HexDirection.Bottom;

        // Act
        var position = new HexPosition(q, r, facing);

        // Assert
        position.Coordinates.Should().Be(new HexCoordinates(q, r));
        position.Facing.Should().Be(facing);
    }

    [Theory]
    [InlineData(HexDirection.Top, HexDirection.Top, 0)] // No turn
    [InlineData(HexDirection.Top, HexDirection.TopRight, 1)] // Turn 1 step clockwise
    [InlineData(HexDirection.Top, HexDirection.BottomRight, 2)] // Turn 2 steps clockwise
    [InlineData(HexDirection.Top, HexDirection.Bottom, 3)] // Turn 3 steps (either direction)
    [InlineData(HexDirection.Top, HexDirection.BottomLeft, 2)] // Turn 2 steps counterclockwise
    [InlineData(HexDirection.Top, HexDirection.TopLeft, 1)] // Turn 1 step counterclockwise
    [InlineData(HexDirection.Bottom, HexDirection.Top, 3)] // Turn 3 steps (either direction)
    [InlineData(HexDirection.Bottom, HexDirection.BottomRight, 1)] // Turn 1 step clockwise
    [InlineData(HexDirection.Bottom, HexDirection.TopRight, 2)] // Turn 2 steps clockwise
    public void GetTurningCost_ReturnsCorrectCost(HexDirection from, HexDirection to, int expectedCost)
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), from);

        // Act
        var cost = position.GetTurningCost(to);

        // Assert
        cost.Should().Be(expectedCost);
    }

    [Fact]
    public void GetTurningCost_FromTopToBottomLeft_ReturnsShorterPath()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);

        // Act
        var cost = position.GetTurningCost(HexDirection.BottomLeft);

        // Assert
        cost.Should().Be(2); // Should take counterclockwise path (2 steps) instead of clockwise (4 steps)
    }

    [Fact]
    public void GetTurningCost_FromBottomToTopRight_ReturnsShorterPath()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);

        // Act
        var cost = position.GetTurningCost(HexDirection.TopRight);

        // Assert
        cost.Should().Be(2); // Should take clockwise path (2 steps) instead of counterclockwise (4 steps)
    }

    [Fact]
    public void Record_WithSameValues_AreEqual()
    {
        // Arrange
        var position1 = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var position2 = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);

        // Assert
        position1.Should().Be(position2);
    }

    [Fact]
    public void Record_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var position1 = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var position2 = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var position3 = new HexPosition(new HexCoordinates(2, 1), HexDirection.Top);

        // Assert
        position1.Should().NotBe(position2); // Different facing
        position1.Should().NotBe(position3); // Different coordinates
    }
}
