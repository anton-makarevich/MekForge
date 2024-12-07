using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace MekForge.Core.Tests.Models.Units.Mechs;

public class MechFactoryTests
{
    private readonly string[] _locustMtfData;

    public MechFactoryTests()
    {
        _locustMtfData = File.ReadAllLines("Resources/Locust LCT-1V.mtf");
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_CreatesCorrectMech()
    {
        // Act
        var mech = MechFactory.CreateFromMtfData(_locustMtfData);

        // Assert
        Assert.Equal("Locust", mech.Chassis);
        Assert.Equal("LCT-1V", mech.Model);
        Assert.Equal(20, mech.Tonnage);
        Assert.Equal(8, mech.GetMovementPoints(MovementType.Walk));
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_HasCorrectArmor()
    {
        // Act
        var mech = MechFactory.CreateFromMtfData(_locustMtfData);

        // Assert
        Assert.Equal(4, mech.Parts.First(p => p.Location == PartLocation.LeftArm).CurrentArmor);
        Assert.Equal(4, mech.Parts.First(p => p.Location == PartLocation.RightArm).CurrentArmor);
        Assert.Equal(8, mech.Parts.First(p => p.Location == PartLocation.LeftTorso).CurrentArmor);
        Assert.Equal(8, mech.Parts.First(p => p.Location == PartLocation.RightTorso).CurrentArmor);
        Assert.Equal(10, mech.Parts.First(p => p.Location == PartLocation.CenterTorso).CurrentArmor);
        Assert.Equal(8, mech.Parts.First(p => p.Location == PartLocation.Head).CurrentArmor);
        Assert.Equal(8, mech.Parts.First(p => p.Location == PartLocation.LeftLeg).CurrentArmor);
        Assert.Equal(8, mech.Parts.First(p => p.Location == PartLocation.RightLeg).CurrentArmor);
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_HasCorrectWeapons()
    {
        // Act
        var mech = MechFactory.CreateFromMtfData(_locustMtfData);

        // Assert
        // Left Arm
        var leftArm = mech.Parts.First(p => p.Location == PartLocation.LeftArm);
        Assert.Contains(leftArm.GetComponents<Weapon>(), w => w.Name == "Machine Gun");

        // Right Arm
        var rightArm = mech.Parts.First(p => p.Location == PartLocation.RightArm);
        Assert.Contains(rightArm.GetComponents<Weapon>(), w => w.Name == "Machine Gun");

        // Center Torso
        var centerTorso = mech.Parts.First(p => p.Location == PartLocation.CenterTorso);
        Assert.Contains(centerTorso.GetComponents<Weapon>(), w => w.Name == "Medium Laser");
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_HasCorrectActuators()
    {
        // Act
        var mech = MechFactory.CreateFromMtfData(_locustMtfData);

        // Assert
        var leftArm = mech.Parts.First(p => p.Location == PartLocation.LeftArm);
        Assert.Contains(leftArm.GetComponents<Component>(), a => a.Name == "Shoulder");
        Assert.Contains(leftArm.GetComponents<Component>(), a => a.Name == "Upper Arm Actuator");

        var rightArm = mech.Parts.First(p => p.Location == PartLocation.RightArm);
        Assert.Contains(rightArm.GetComponents<Component>(), a => a.Name == "Shoulder");
        Assert.Contains(rightArm.GetComponents<Component>(), a => a.Name == "Upper Arm Actuator");
    }

    [Fact]
    public async Task CreateFromMtfFileAsync_LocustMtf_CreatesCorrectMech()
    {
        // Act
        var mech = await MechFactory.CreateFromMtfFileAsync("Resources/Locust LCT-1V.mtf");

        // Assert
        Assert.Equal("Locust", mech.Chassis);
        Assert.Equal("LCT-1V", mech.Model);
        Assert.Equal(20, mech.Tonnage);
        Assert.Equal(8, mech.GetMovementPoints(MovementType.Walk));
    }
}
