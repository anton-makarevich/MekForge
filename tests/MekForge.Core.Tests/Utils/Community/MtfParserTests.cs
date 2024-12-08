using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Utils.Community;

namespace Sanet.MekForge.Core.Tests.Utils.Community;

public class MtfParserTests
{
    private readonly string[] _locustMtfData = File.ReadAllLines("Resources/Locust LCT-1V.mtf");

    [Fact]
    public void Parse_LocustMtf_ReturnsCorrectBasicData()
    {
        // Arrange
        var parser = new MtfParser();

        // Act
        var mechData = parser.Parse(_locustMtfData);

        // Assert
        Assert.Equal("Locust", mechData.Chassis);
        Assert.Equal("LCT-1V", mechData.Model);
        Assert.Equal(20, mechData.Mass);
        Assert.Equal(8, mechData.WalkMp);
    }

    [Fact]
    public void Parse_LocustMtf_ReturnsCorrectArmorValues()
    {
        // Arrange
        var parser = new MtfParser();

        // Act
        var mechData = parser.Parse(_locustMtfData);

        // Assert
        Assert.Equal(4, mechData.ArmorValues[PartLocation.LeftArm].FrontArmor);
        Assert.Equal(4, mechData.ArmorValues[PartLocation.RightArm].FrontArmor);
        Assert.Equal(8, mechData.ArmorValues[PartLocation.LeftTorso].FrontArmor);
        Assert.Equal(8, mechData.ArmorValues[PartLocation.RightTorso].FrontArmor);
        Assert.Equal(10, mechData.ArmorValues[PartLocation.CenterTorso].FrontArmor);
        Assert.Equal(8, mechData.ArmorValues[PartLocation.Head].FrontArmor);
        Assert.Equal(8, mechData.ArmorValues[PartLocation.LeftLeg].FrontArmor);
        Assert.Equal(8, mechData.ArmorValues[PartLocation.RightLeg].FrontArmor);
    }

    [Fact]
    public void Parse_LocustMtf_ReturnsCorrectEquipment()
    {
        // Arrange
        var parser = new MtfParser();

        // Act
        var mechData = parser.Parse(_locustMtfData);

        // Assert
        // Left Arm
        var leftArmEquipment = mechData.LocationEquipment[PartLocation.LeftArm];
        Assert.Contains("Shoulder", leftArmEquipment);
        Assert.Contains("Upper Arm Actuator", leftArmEquipment);
        Assert.Contains("Machine Gun", leftArmEquipment);

        // Right Arm
        var rightArmEquipment = mechData.LocationEquipment[PartLocation.RightArm];
        Assert.Contains("Shoulder", rightArmEquipment);
        Assert.Contains("Upper Arm Actuator", rightArmEquipment);
        Assert.Contains("Machine Gun", rightArmEquipment);

        // Center Torso
        var centerTorsoEquipment = mechData.LocationEquipment[PartLocation.CenterTorso];
        Assert.Contains("Medium Laser", centerTorsoEquipment);
        Assert.Contains("Fusion Engine", centerTorsoEquipment);
    }
}
