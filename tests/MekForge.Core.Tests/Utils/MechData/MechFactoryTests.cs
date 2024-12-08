using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Utils.MechData;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Utils.MechData;

public class MechFactoryTests
{
    private readonly MechFactory _mechFactory;

    public MechFactoryTests()
    {
        var structureValueProvider = Substitute.For<IStructureValueProvider>();
        structureValueProvider.GetStructureValues(20).Returns(new Dictionary<PartLocation, int>
        {
            { PartLocation.Head, 8 },
            { PartLocation.CenterTorso, 10 },
            { PartLocation.LeftTorso, 8 },
            { PartLocation.RightTorso, 8 },
            { PartLocation.LeftArm, 4 },
            { PartLocation.RightArm, 4 },
            { PartLocation.LeftLeg, 8 },
            { PartLocation.RightLeg, 8 }
        });

        _mechFactory = new MechFactory(CreateDummyMechData(), structureValueProvider);
    }

    private Core.Utils.MechData.MechData CreateDummyMechData()
    {
        return new Core.Utils.MechData.MechData
        {
            Chassis = "Locust",
            Model = "LCT-1V",
            Mass = 20,
            WalkMp = 8,
            ArmorValues = new Dictionary<PartLocation, ArmorLocation>
            {
                { PartLocation.Head, new ArmorLocation { FrontArmor = 8 } },
                { PartLocation.CenterTorso, new ArmorLocation { FrontArmor = 10, RearArmor = 5 } },
                { PartLocation.LeftTorso, new ArmorLocation { FrontArmor = 8, RearArmor = 4 } },
                { PartLocation.RightTorso, new ArmorLocation { FrontArmor = 8, RearArmor = 4 } },
                { PartLocation.LeftArm, new ArmorLocation { FrontArmor = 4 } },
                { PartLocation.RightArm, new ArmorLocation { FrontArmor = 4 } },
                { PartLocation.LeftLeg, new ArmorLocation { FrontArmor = 8 } },
                { PartLocation.RightLeg, new ArmorLocation { FrontArmor = 8 } }
            },
            LocationEquipment = new Dictionary<PartLocation, List<string>>
            {
                { PartLocation.LeftArm, ["Machine Gun"] },
                { PartLocation.RightArm, ["Upper Arm Actuator", "Medium Laser"] }
            }
        };
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_CreatesCorrectMech()
    {
        // Act
        var mech = _mechFactory.Create();

        // Assert
        mech.Chassis.Should().Be("Locust");
        mech.Model.Should().Be("LCT-1V");
        mech.Name.Should().Be("Locust LCT-1V");
        mech.Tonnage.Should().Be(20);
        mech.Class.Should().Be(WeightClass.Light);
        mech.GetMovementPoints(MovementType.Walk).Should().Be(8);
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_HasCorrectArmor()
    {
        // Act
        var mech = _mechFactory.Create();

        // Assert
        mech.Parts.First(p => p.Location == PartLocation.LeftArm).CurrentArmor.Should().Be(4);
        mech.Parts.First(p => p.Location == PartLocation.RightArm).CurrentArmor.Should().Be(4);
        mech.Parts.First(p => p.Location == PartLocation.LeftTorso).CurrentArmor.Should().Be(8);
        mech.Parts.First(p => p.Location == PartLocation.RightTorso).CurrentArmor.Should().Be(8);
        mech.Parts.First(p => p.Location == PartLocation.CenterTorso).CurrentArmor.Should().Be(10);
        mech.Parts.First(p => p.Location == PartLocation.Head).CurrentArmor.Should().Be(8);
        mech.Parts.First(p => p.Location == PartLocation.LeftLeg).CurrentArmor.Should().Be(8);
        mech.Parts.First(p => p.Location == PartLocation.RightLeg).CurrentArmor.Should().Be(8);
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_HasCorrectWeapons()
    {
        // Act
        var mech = _mechFactory.Create();

        // Assert
        // Left Arm
        var leftArm = mech.Parts.First(p => p.Location == PartLocation.LeftArm);
        leftArm.GetComponents<Weapon>().Should().Contain(w => w.Name == "Machine Gun");

        // Right Arm
        var rightArm = mech.Parts.First(p => p.Location == PartLocation.RightArm);
        rightArm.GetComponents<Weapon>().Should().Contain(w => w.Name == "Medium Laser");
    }

    [Fact]
    public void CreateFromMtfData_LocustMtf_HasCorrectActuators()
    {

        // Act
        var mech = _mechFactory.Create();

        // Assert
        var leftArm = mech.Parts.First(p => p.Location == PartLocation.LeftArm);
        leftArm.GetComponents<Component>().Should().Contain(a => a.Name == "Shoulder");

        var rightArm = mech.Parts.First(p => p.Location == PartLocation.RightArm);
        rightArm.GetComponents<Component>().Should().Contain(a => a.Name == "Shoulder");
        rightArm.GetComponents<Component>().Should().Contain(a => a.Name == "Upper Arm Actuator");
    }
}
