using Shouldly;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Tests.Models.Map;

public class HexCoordinatesTests
{
    [Fact]
    public void Constructor_SetsQAndR()
    {
        // Arrange & Act
        var coords = new HexCoordinates(2, 3);

        // Assert
        coords.Q.ShouldBe(2);
        coords.R.ShouldBe(3);
    }

    [Fact]
    public void S_CalculatesCorrectly()
    {
        // Arrange
        var coords = new HexCoordinates(2, 3);

        // Act
        var s = coords.S;

        // Assert
        s.ShouldBe(-5); // -Q - R = -2 - 3 = -5
    }
    
    [Fact]
    public void CubeCoordinates_CalculateCorrectly()
    {
        // Arrange
        var hexEven = new HexCoordinates(2, 3); // Even column
        var hexOdd = new HexCoordinates(1, 2);  // Odd column

        // Assert
        hexEven.X.ShouldBe(2);
        hexEven.Z.ShouldBe(2); // Calculation: R - (Q + (Q % 2)) / 2 = 3 - (2 + 0) / 2
        hexEven.Y.ShouldBe(-4); // Calculation: -X - Z = -2 - 2

        hexOdd.X.ShouldBe(1);
        hexOdd.Z.ShouldBe(1); // Calculation: R - (Q + (Q % 2)) / 2 = 2 - (1 + 1) / 2
        hexOdd.Y.ShouldBe(-2); // Calculation: -X - Z = -1 - 1
    }
    
    [Theory]
    [InlineData(5, 4, 5, 3, HexDirection.Top)]
    [InlineData(5, 4, 6, 4, HexDirection.BottomRight)]
    [InlineData(5, 4, 4, 4, HexDirection.BottomLeft)]
    [InlineData(4, 4, 3, 4, HexDirection.TopLeft)]
    public void GetDirectionToNeighbour_ReturnsExpectedDirection(int centerQ, int centerR, int neighbourQ, int neighbourR, HexDirection expectedDirection)
    {
        // Arrange
        var center = new HexCoordinates(centerQ, centerR);
        var neighbour = new HexCoordinates(neighbourQ, neighbourR);

        // Act
        var direction = center.GetDirectionToNeighbour(neighbour);

        // Assert
        direction.ShouldBe(expectedDirection);
    }
    
    [Fact]
    public void GetDirectionToNeighbour_ThrowsWhenNotAdjacent()
    {
        // Arrange
        var center = new HexCoordinates(5, 4);
        var neighbour = new HexCoordinates(7, 4); // Not adjacent

        // Act & Assert
        Action act = () => center.GetDirectionToNeighbour(neighbour);
        Should.Throw<WrongHexException>(act)
            .Message.ShouldBe("Neighbour is not adjacent to center.");
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
        distance.ShouldBe(expectedDistance);
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
        distance1To2.ShouldBe(distance2To1);
    }

    [Fact]
    public void GetAdjacentCoordinates_ReturnsAllSixNeighbors()
    {
        // Arrange
        var center = new HexCoordinates(2, 2);

        // Act
        var neighbors = center.GetAdjacentCoordinates().ToList();

        // Assert
        neighbors.Count.ShouldBe(6);
        neighbors.ShouldContain(new HexCoordinates(1, 2));   // East
        neighbors.ShouldContain(new HexCoordinates(1, 3));  // Northeast
        neighbors.ShouldContain(new HexCoordinates(2, 1));  // Northwest
        neighbors.ShouldContain(new HexCoordinates(2, 3));  // West
        neighbors.ShouldContain(new HexCoordinates(3, 2));  // Southwest
        neighbors.ShouldContain(new HexCoordinates(3, 3));   // Southeast
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
            center.DistanceTo(neighbor).ShouldBe(1);
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
            center.DistanceTo(hex).ShouldBeLessThanOrEqualTo(range);
        }

        // Verify that all hexes at exactly range distance are included
        var hexesAtRange = hexesInRange.Where(h => center.DistanceTo(h) == range);
        hexesAtRange.Count().ShouldBe(6 * range); // Each range adds 6 more hexes
    }

    [Fact]
    public void X_CalculatesCorrectPixelPosition()
    {
        // Arrange & Act
        var hex1 = new HexCoordinates(0, 0);
        var hex2 = new HexCoordinates(1, 0);
        var hex3 = new HexCoordinates(2, 0);

        // Assert
        hex1.H.ShouldBe(0);
        hex2.H.ShouldBe(75); // 100 * 0.75
        hex3.H.ShouldBe(150); // 200 * 0.75
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
        hex1.V.ShouldBe(0);
        hex2.V.ShouldBe(HexCoordinates.HexHeight);
        hex3.V.ShouldBe( -HexCoordinates.HexHeight*0.5);  // Offset for odd Q
        hex4.V.ShouldBe(HexCoordinates.HexHeight*0.5);  // Height - 0.5*Height offset for odd Q
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
        hexes.ShouldNotBeNull();
        hexes.Count.ShouldBe(4);
        hexes.ShouldBe(new[] { new HexCoordinates(1, 1), new HexCoordinates(2,1), new HexCoordinates(2, 2), new HexCoordinates(3, 3) });
    }

    [Fact]
    public void GetHexesAlongLine_ShouldHandleSameHex()
    {
        // Arrange
        var coordinates = new HexCoordinates(2, 2);
        
        // Act
        var hexes = coordinates.LineTo(coordinates);

        // Assert
        hexes.ShouldNotBeNull();
        hexes.Count.ShouldBe(1);
        hexes.ShouldBe(new[] { coordinates });
    }
    
    [Fact]
    public void ToData_ReturnsCorrectDataObject()
    {
        // Arrange
        var hexCoordinates = new HexCoordinates(3, 4);
         
        // Act
        var data = hexCoordinates.ToData();
         
        // Assert
        data.Q.ShouldBe(3);
        data.R.ShouldBe(4);
    }

    [Fact]
    public void ToString_ReturnsHexCoordinatesAsText()
    {
        // Arrange
        var hexCoordinates = new HexCoordinates(3, 4);
        
        // Act
        var text = hexCoordinates.ToString();
        
        // Assert
        text.ShouldBe("0304");
    }

    [Fact]
    public void GetHexesInFiringArc_ForwardArcRange2_ReturnsCorrectHexes()
    {
        // Arrange
        var unitPosition = new HexCoordinates(5, 5);
        var expectedHexes = new[]
        {
            // Range 1 (3 hexes)
            new HexCoordinates(3, 4),
            new HexCoordinates(5, 4),
            new HexCoordinates(6, 4),
            
            // Range 2 (5 hexes)
            new HexCoordinates(3, 4),
            new HexCoordinates(4, 3),
            new HexCoordinates(5, 3),
            new HexCoordinates(6, 3),
            new HexCoordinates(7, 4)
        };

        // Act
        var hexesInArc = unitPosition.GetHexesInFiringArc(HexDirection.Top, FiringArc.Forward, 2).ToList();

        // Assert
        hexesInArc.Count.ShouldBe(8);
        foreach (var expected in expectedHexes)
        {
            hexesInArc.ShouldContain(hex => hex.Q == expected.Q && hex.R == expected.R,
                $"Missing hex at Q:{expected.Q}, R:{expected.R}");
        }
    }

    [Fact]
    public void GetHexesInFiringArc_RearArcRange2_ReturnsCorrectHexes()
    {
        // Arrange
        var unitPosition = new HexCoordinates(5, 5);
        var expectedHexes = new[]
        {
            // Range 1 (1 hex)
            new HexCoordinates(5, 6),
            
            // Range 2 (3 hexes)
            new HexCoordinates(4, 6),
            new HexCoordinates(5, 7),
            new HexCoordinates(6, 6)
        };

        // Act
        var hexesInArc = unitPosition.GetHexesInFiringArc(HexDirection.Top, FiringArc.Rear, 2).ToList();

        // Assert
        hexesInArc.Count.ShouldBe(4);
        foreach (var expected in expectedHexes)
        {
            hexesInArc.ShouldContain(hex => hex.Q == expected.Q && hex.R == expected.R,
                $"Missing hex at Q:{expected.Q}, R:{expected.R}");
        }
    }
}
