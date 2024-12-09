using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Utils.MechData.Community;

namespace Sanet.MekForge.Core.Tests.Utils.MechData.Community;

public class MtfDataProviderTests
{
    private readonly string[] _locustMtfData = File.ReadAllLines("Resources/Mechs/Locust LCT-1V.mtf");

    [Fact]
    public void Parse_LocustMtf_ReturnsCorrectBasicData()
    {
        // Arrange
        var parser = new MtfDataProvider();

        // Act
        var mechData = parser.LoadMechFromTextData(_locustMtfData);

        // Assert
        mechData.Chassis.Should().Be("Locust");
        mechData.Model.Should().Be("LCT-1V");
        mechData.Mass.Should().Be(20);
        mechData.WalkMp.Should().Be(8);
    }

    [Fact]
    public void Parse_LocustMtf_ReturnsCorrectArmorValues()
    {
        // Arrange
        var parser = new MtfDataProvider();

        // Act
        var mechData = parser.LoadMechFromTextData(_locustMtfData);

        // Assert
        mechData.ArmorValues[PartLocation.LeftArm].FrontArmor.Should().Be(4);
        mechData.ArmorValues[PartLocation.RightArm].FrontArmor.Should().Be(4);
        mechData.ArmorValues[PartLocation.LeftTorso].FrontArmor.Should().Be(8);
        mechData.ArmorValues[PartLocation.RightTorso].FrontArmor.Should().Be(8);
        mechData.ArmorValues[PartLocation.CenterTorso].FrontArmor.Should().Be(10);
        mechData.ArmorValues[PartLocation.Head].FrontArmor.Should().Be(8);
        mechData.ArmorValues[PartLocation.LeftLeg].FrontArmor.Should().Be(8);
        mechData.ArmorValues[PartLocation.RightLeg].FrontArmor.Should().Be(8);
    }

    [Fact]
    public void Parse_LocustMtf_ReturnsCorrectEquipment()
    {
        // Arrange
        var parser = new MtfDataProvider();

        // Act
        var mechData = parser.LoadMechFromTextData(_locustMtfData);

        // Assert
        // Left Arm
        var leftArmEquipment = mechData.LocationEquipment[PartLocation.LeftArm];
        leftArmEquipment.Should().Contain("Shoulder");
        leftArmEquipment.Should().Contain("Upper Arm Actuator");
        leftArmEquipment.Should().Contain("Machine Gun");

        // Right Arm
        var rightArmEquipment = mechData.LocationEquipment[PartLocation.RightArm];
        rightArmEquipment.Should().Contain("Shoulder");
        rightArmEquipment.Should().Contain("Upper Arm Actuator");
        rightArmEquipment.Should().Contain("Machine Gun");

        // Center Torso
        var centerTorsoEquipment = mechData.LocationEquipment[PartLocation.CenterTorso];
        centerTorsoEquipment.Should().Contain("Medium Laser");
        centerTorsoEquipment.Should().Contain("Fusion Engine");
        
    }
}
