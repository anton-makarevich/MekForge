using Shouldly;
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
        mechData.Chassis.ShouldBe("Locust");
        mechData.Model.ShouldBe("LCT-1V");
        mechData.Mass.ShouldBe(20);
        mechData.WalkMp.ShouldBe(8);
        mechData.EngineRating.ShouldBe(160);
        mechData.EngineType.ShouldBe("Fusion");
    }

    [Fact]
    public void Parse_LocustMtf_ReturnsCorrectArmorValues()
    {
        // Arrange
        var parser = new MtfDataProvider();

        // Act
        var mechData = parser.LoadMechFromTextData(_locustMtfData);

        // Assert
        mechData.ArmorValues[PartLocation.LeftArm].FrontArmor.ShouldBe(4);
        mechData.ArmorValues[PartLocation.RightArm].FrontArmor.ShouldBe(4);
        mechData.ArmorValues[PartLocation.LeftTorso].FrontArmor.ShouldBe(8);
        mechData.ArmorValues[PartLocation.RightTorso].FrontArmor.ShouldBe(8);
        mechData.ArmorValues[PartLocation.CenterTorso].FrontArmor.ShouldBe(10);
        mechData.ArmorValues[PartLocation.Head].FrontArmor.ShouldBe(8);
        mechData.ArmorValues[PartLocation.LeftLeg].FrontArmor.ShouldBe(8);
        mechData.ArmorValues[PartLocation.RightLeg].FrontArmor.ShouldBe(8);
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
        leftArmEquipment.ShouldContain(MekForgeComponent.Shoulder);
        leftArmEquipment.ShouldContain(MekForgeComponent.UpperArmActuator);
        leftArmEquipment.ShouldContain(MekForgeComponent.MachineGun);

        // Right Arm
        var rightArmEquipment = mechData.LocationEquipment[PartLocation.RightArm];
        rightArmEquipment.ShouldContain(MekForgeComponent.Shoulder);
        rightArmEquipment.ShouldContain(MekForgeComponent.UpperArmActuator);
        rightArmEquipment.ShouldContain(MekForgeComponent.MachineGun);

        // Center Torso
        var centerTorsoEquipment = mechData.LocationEquipment[PartLocation.CenterTorso];
        centerTorsoEquipment.ShouldContain(MekForgeComponent.Engine);
        centerTorsoEquipment.ShouldContain(MekForgeComponent.Gyro);
        centerTorsoEquipment.ShouldContain(MekForgeComponent.MediumLaser);
    }
}
