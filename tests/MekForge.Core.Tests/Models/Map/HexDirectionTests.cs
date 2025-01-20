using Sanet.MekForge.Core.Models.Map;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Map;

public class HexDirectionTests
{
    [Theory]
    [InlineData(HexDirection.Top, HexDirection.Bottom)]
    [InlineData(HexDirection.TopRight, HexDirection.BottomLeft)]
    [InlineData(HexDirection.BottomRight, HexDirection.TopLeft)]
    [InlineData(HexDirection.Bottom, HexDirection.Top)]
    [InlineData(HexDirection.BottomLeft, HexDirection.TopRight)]
    [InlineData(HexDirection.TopLeft, HexDirection.BottomRight)]
    public void GetOppositeDirection_ReturnsCorrectOppositeDirection(HexDirection input, HexDirection expected)
    {
        // Act
        var result = input.GetOppositeDirection();

        // Assert
        Assert.Equal(expected, result);
    }
}
