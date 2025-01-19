using FluentAssertions;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Data;

namespace Sanet.MekForge.Core.Tests.Models.Map;

public class PathSegmentTests
{
    [Fact]
    public void Constructor_WithPositionsAndCost_SetsProperties()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(2, 2), HexDirection.Bottom);
        const int cost = 3;

        // Act
        var segment = new PathSegment(from, to, cost);

        // Assert
        segment.From.Should().Be(from);
        segment.To.Should().Be(to);
        segment.Cost.Should().Be(cost);
    }

    [Fact]
    public void Constructor_WithData_SetsProperties()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(2, 2), HexDirection.Bottom);
        const int cost = 3;
        var data = new PathSegmentData
        {
            From = from.ToData(),
            To = to.ToData(),
            Cost = cost
        };

        // Act
        var segment = new PathSegment(data);

        // Assert
        segment.From.Should().Be(from);
        segment.To.Should().Be(to);
        segment.Cost.Should().Be(cost);
    }

    [Fact]
    public void ToData_ReturnsCorrectData()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(2, 2), HexDirection.Bottom);
        const int cost = 3;
        var segment = new PathSegment(from, to, cost);

        // Act
        var data = segment.ToData();

        // Assert
        data.From.Should().Be(from.ToData());
        data.To.Should().Be(to.ToData());
        data.Cost.Should().Be(cost);
    }

    [Fact]
    public void Record_WithSameValues_AreEqual()
    {
        // Arrange
        var from = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var to = new HexPosition(new HexCoordinates(2, 2), HexDirection.Bottom);
        const int cost = 3;

        // Act
        var segment1 = new PathSegment(from, to, cost);
        var segment2 = new PathSegment(from, to, cost);

        // Assert
        segment1.Should().Be(segment2);
    }
}