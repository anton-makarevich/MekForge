using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Engines;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Data;

public class MechFactoryTests
{
    private readonly MechFactory _mechFactory;
    private readonly UnitData _unitData;

    public MechFactoryTests()
    {
        var structureValueProvider = Substitute.For<IRulesProvider>();
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
        _unitData = CreateDummyMechData();
        _mechFactory = new MechFactory(structureValueProvider);
    }

    public static UnitData CreateDummyMechData(Tuple<PartLocation, List<MekForgeComponent>>? locationEquipment = null)
    {
        var data = new UnitData
        {
            Chassis = "Locust",
            Model = "LCT-1V",
            Mass = 20,
            WalkMp = 8,
            EngineRating = 275,
            EngineType = "Fusion",
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
            LocationEquipment = new Dictionary<PartLocation, List<MekForgeComponent>>
            {
                { PartLocation.LeftArm, [MekForgeComponent.MachineGun] },
                { PartLocation.CenterTorso, [MekForgeComponent.Engine] },
                { PartLocation.RightArm, [MekForgeComponent.UpperArmActuator, MekForgeComponent.MediumLaser] }
            },
            Quirks = new Dictionary<string, string>(),
            AdditionalAttributes = new Dictionary<string, string>()
        };
        if (locationEquipment != null)
        {
            data.LocationEquipment[locationEquipment.Item1] = locationEquipment.Item2;
        }
        return data;
    }

    [Fact]
    public void CreateFromMtfData_CreatesCorrectMech()
    {
        // Act
        var mech = _mechFactory.Create(_unitData);

        // Assert
        mech.Chassis.Should().Be("Locust");
        mech.Model.Should().Be("LCT-1V");
        mech.Name.Should().Be("Locust LCT-1V");
        mech.Tonnage.Should().Be(20);
        mech.Class.Should().Be(WeightClass.Light);
        mech.GetMovementPoints(MovementType.Walk).Should().Be(8);
    }

    [Fact]
    public void CreateFromMtfData_HasCorrectArmor()
    {
        // Act
        var mech = _mechFactory.Create(_unitData);

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
    public void CreateFromMtfData_HasCorrectWeapons()
    {
        // Act
        var mech = _mechFactory.Create(_unitData);

        // Assert
        // Left Arm
        var leftArm = mech.Parts.First(p => p.Location == PartLocation.LeftArm);
        leftArm.GetComponents<Weapon>().Should().Contain(w => w.Name == "Machine Gun");
        
        // Right Arm
        var rightArm = mech.Parts.First(p => p.Location == PartLocation.RightArm);
        rightArm.GetComponents<Weapon>().Should().Contain(w => w.Name == "Medium Laser");
        
        // Center Torso
        var centerTorso = mech.Parts.First(p => p.Location == PartLocation.CenterTorso);
        var engines = centerTorso.GetComponents<Engine>().ToList();
        engines.Should().ContainSingle();
        engines[0].Rating.Should().Be(275);
        engines[0].Type.Should().Be(EngineType.Fusion);

    }

    [Fact]
    public void CreateFromMtfData_HasCorrectActuators()
    {

        // Act
        var mech = _mechFactory.Create(_unitData);

        // Assert
        var leftArm = mech.Parts.First(p => p.Location == PartLocation.LeftArm);
        leftArm.GetComponents<Component>().Should().Contain(a => a.Name == "Shoulder");

        var rightArm = mech.Parts.First(p => p.Location == PartLocation.RightArm);
        rightArm.GetComponents<Component>().Should().Contain(a => a.Name == "Shoulder");
        rightArm.GetComponents<Component>().Should().Contain(a => a.Name == "Upper Arm Actuator");
    }
    
    [Fact]
    public void CreateFromMtfData_CorrectlyAddsOneComponentThatOccupiesSeveralSlots()
    {
        // Arrange
        var locationEquipment = Tuple.Create(PartLocation.LeftTorso, new List<MekForgeComponent> 
        { 
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5 
        });
        var mechData = CreateDummyMechData(locationEquipment);

        // Act
        var mech = _mechFactory.Create(mechData);

        // Assert
        var leftTorso = mech.Parts.First(p => p.Location == PartLocation.LeftTorso);
        var weapon = leftTorso.GetComponents<AC5>();
        weapon.Count().Should().Be(1); 
    }
    [Fact]
    public void CreateFromMtfData_CorrectlyAddsTwoComponentsThatOccupySeveralSlots()
    {
        // Arrange
        var locationEquipment = Tuple.Create(PartLocation.LeftTorso, new List<MekForgeComponent> 
        { 
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5,
            MekForgeComponent.AC5 
        });
        var mechData = CreateDummyMechData(locationEquipment);

        // Act
        var mech = _mechFactory.Create(mechData);

        // Assert
        var leftTorso = mech.Parts.First(p => p.Location == PartLocation.LeftTorso);
        var weapon = leftTorso.GetComponents<AC5>();
        weapon.Count().Should().Be(2); 
    }
}
