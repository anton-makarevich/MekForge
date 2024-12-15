using FluentAssertions;
using JetBrains.Annotations;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Utils;

namespace Sanet.MekForge.Core.Tests.Utils;

[TestSubject(typeof(ToDataExtensions))]
public class ToDataExtensionsTest
{

    [Fact]
    public void ToData_ReturnsCorrectDataObject()
    {
         // Arrange
         var hexCoordinates = new HexCoordinates(3, 4);
         
         // Act
         var data = hexCoordinates.ToData();
         
         // Assert
         data.Q.Should().Be(3);
         data.R.Should().Be(4);
    }
}