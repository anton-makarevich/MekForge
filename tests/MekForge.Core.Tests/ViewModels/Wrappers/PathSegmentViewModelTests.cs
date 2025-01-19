using FluentAssertions;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.ViewModels.Wrappers;

namespace Sanet.MekForge.Core.Tests.ViewModels.Wrappers;

public class PathSegmentViewModelTests
{
    [Fact]
    public void IsTurn_ReturnsFalse_WhenPositionsAreDifferent()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(1, 0), HexDirection.Top);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.IsTurn.Should().BeFalse();
    }

    [Fact]
    public void IsTurn_ReturnsTrue_WhenSamePositionDifferentFacing()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(0, 0), HexDirection.TopRight);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.IsTurn.Should().BeTrue();
    }

    [Fact]
    public void ViewModel_ReturnsCorrectValues()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(2, 1), HexDirection.Top);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.From.Should().Be(from);
        sut.To.Should().Be(to);
        sut.FromX.Should().Be(from.Coordinates.H);
        sut.FromY.Should().Be(from.Coordinates.V );
    }

    [Fact]
    public void EndPoints_CalculatedCorrectly_ForStraightMovement()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(2, 1), HexDirection.Top);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.From.Should().Be(from);
        sut.To.Should().Be(to);
        sut.EndX.Should().Be(sut.StartX + 75); // One hex to the right
        sut.EndY.Should().BeApproximately(sut.StartY + 43.3,0.1); // Half hex to the bottom
    }

    [Fact]
    public void EndPoints_CalculatedCorrectly_ForTurn()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(0, 0), HexDirection.TopRight);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);
        const double turnLength = 40;

        // Expected values for 60-degree turn (NorthRight = 60 degrees)
        var expectedX = sut.StartX + turnLength * Math.Sin(Math.PI / 3); // sin(60°)
        var expectedY = sut.StartY - turnLength * Math.Cos(Math.PI / 3); // -cos(60°)

        // Act & Assert
        sut.EndX.Should().BeApproximately(expectedX, 0.001);
        sut.EndY.Should().BeApproximately(expectedY, 0.001);
    }

    [Fact]
    public void ArrowDirectionVector_PointsInFacingDirection_ForTurn()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(0, 0), HexDirection.TopRight);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Expected values for NorthRight direction (60 degrees)
        var expectedX = Math.Sin(Math.PI / 3); // sin(60°)
        var expectedY = -Math.Cos(Math.PI / 3); // -cos(60°)

        // Act
        var direction = sut.ArrowDirectionVector;

        // Assert
        direction.X.Should().BeApproximately((float)expectedX, 0.001f);
        direction.Y.Should().BeApproximately((float)expectedY, 0.001f);
    }

    [Fact]
    public void ArrowDirectionVector_PointsInMovementDirection_ForStraightMovement()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(0, 1), HexDirection.Top);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Expected values for North direction (0 degrees)
        var expectedX = Math.Sin(0); // sin(0°) = 0
        var expectedY = -Math.Cos(0); // -cos(0°) = -1

        // Act
        var direction = sut.ArrowDirectionVector;

        // Assert
        direction.X.Should().BeApproximately((float)expectedX, 0.001f);
        direction.Y.Should().BeApproximately((float)expectedY, 0.001f);
    }

    [Fact]
    public void TurnAngleSweep_Returns60_ForClockwiseTurn()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(0, 0), HexDirection.TopRight);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.TurnAngleSweep.Should().Be(60);
    }

    [Fact]
    public void TurnAngleSweep_ReturnsMinus60_ForCounterclockwiseTurn()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.TopRight);
        var to = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.TurnAngleSweep.Should().Be(-60);
    }

    [Fact]
    public void TurnAngleSweep_Returns0_ForStraightMovement()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(1, 0), HexDirection.Top);
        var segment = new PathSegment(from, to, 1);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.TurnAngleSweep.Should().Be(0);
    }

    [Fact]
    public void Cost_ReturnsSegmentCost()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(1, 0), HexDirection.Top);
        var segment = new PathSegment(from, to, 2);
        var sut = new PathSegmentViewModel(segment);

        // Act & Assert
        sut.Cost.Should().Be(2);
    }
}
