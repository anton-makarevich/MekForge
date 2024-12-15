using FluentAssertions;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Data.Community;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Tests.Data.Community;

public class MtfDataProviderTests
{
    private readonly string[] _locustMtfData = File.ReadAllLines("Resources/Mechs/LCT-1V.mtf");

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
        mechData.EngineRating.Should().Be(160);
        mechData.EngineType.Should().Be("Fusion");
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
        leftArmEquipment.Should().Contain(MekForgeComponent.Shoulder);
        leftArmEquipment.Should().Contain(MekForgeComponent.UpperArmActuator);
        leftArmEquipment.Should().Contain(MekForgeComponent.MachineGun);

        // Right Arm
        var rightArmEquipment = mechData.LocationEquipment[PartLocation.RightArm];
        rightArmEquipment.Should().Contain(MekForgeComponent.Shoulder);
        rightArmEquipment.Should().Contain(MekForgeComponent.UpperArmActuator);
        rightArmEquipment.Should().Contain(MekForgeComponent.MachineGun);

        // Center Torso
        var centerTorsoEquipment = mechData.LocationEquipment[PartLocation.CenterTorso];
        centerTorsoEquipment.Should().Contain(MekForgeComponent.Engine);
        centerTorsoEquipment.Should().Contain(MekForgeComponent.Gyro);
        centerTorsoEquipment.Should().Contain(MekForgeComponent.MediumLaser);
    }
}
