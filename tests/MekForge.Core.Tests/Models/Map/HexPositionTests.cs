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

    [Theory]
    [InlineData(HexDirection.Top, HexDirection.TopRight, 1)] // One step clockwise
    [InlineData(HexDirection.Top, HexDirection.BottomRight, 2)] // Two steps clockwise
    [InlineData(HexDirection.Top, HexDirection.Bottom, 3)] // Three steps either way
    public void GetTurningSteps_ReturnsCorrectNumberOfSteps(HexDirection from, HexDirection to, int expectedSteps)
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), from);

        // Act
        var steps = position.GetTurningSteps(to).ToList();

        // Assert
        steps.Count.Should().Be(expectedSteps);
        steps.All(p => p.Coordinates == position.Coordinates).Should().BeTrue(); // All steps are in same hex
    }

    [Fact]
    public void GetTurningSteps_NoTurnNeeded_ReturnsEmptySequence()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);

        // Act
        var steps = position.GetTurningSteps(HexDirection.Top).ToList();

        // Assert
        steps.Should().BeEmpty();
    }

    [Fact]
    public void GetTurningSteps_FromTopToRight_ReturnsCorrectSequence()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);

        // Act
        var steps = position.GetTurningSteps(HexDirection.BottomRight).ToList();

        // Assert
        steps.Count.Should().Be(2);
        steps[0].Facing.Should().Be(HexDirection.TopRight);
        steps[1].Facing.Should().Be(HexDirection.BottomRight);
    }

    [Fact]
    public void GetTurningSteps_FromTopToLeft_ReturnsShorterCounterclockwisePath()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);

        // Act
        var steps = position.GetTurningSteps(HexDirection.BottomLeft).ToList();

        // Assert
        steps.Count.Should().Be(2); // Should take 2 steps counterclockwise instead of 4 clockwise
        steps[0].Facing.Should().Be(HexDirection.TopLeft);
        steps[1].Facing.Should().Be(HexDirection.BottomLeft);
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
