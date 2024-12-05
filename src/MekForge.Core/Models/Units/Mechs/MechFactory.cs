using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public static class MechFactory
{
    public static Mech CreateLocustLct1V()
    {
        var partsData = new Dictionary<PartLocation, UnitPartData>
        {
            [PartLocation.Head] = new(
                Name: "Head",
                MaxArmor: 9,
                MaxStructure: 3,
                Slots: 6,
                Components: new List<Component>()),

            [PartLocation.CenterTorso] = new(
                Name: "Center Torso",
                MaxArmor: 10,
                MaxStructure: 31,
                Slots: 12,
                Components: new List<Component> { new HeatSink() }),

            [PartLocation.LeftTorso] = new(
                Name: "Left Torso",
                MaxArmor: 8,
                MaxStructure: 21,
                Slots: 12,
                Components: new List<Component> { new MediumLaser() }),

            [PartLocation.RightTorso] = new(
                Name: "Right Torso",
                MaxArmor: 8,
                MaxStructure: 21,
                Slots: 12,
                Components: new List<Component> { new MediumLaser() }),

            [PartLocation.LeftArm] = new(
                Name: "Left Arm",
                MaxArmor: 6,
                MaxStructure: 17,
                Slots: 12,
                Components: new List<Component>()),

            [PartLocation.RightArm] = new(
                Name: "Right Arm",
                MaxArmor: 6,
                MaxStructure: 17,
                Slots: 12,
                Components: new List<Component>()),

            [PartLocation.LeftLeg] = new(
                Name: "Left Leg",
                MaxArmor: 8,
                MaxStructure: 25,
                Slots: 6,
                Components: new List<Component>()),

            [PartLocation.RightLeg] = new(
                Name: "Right Leg",
                MaxArmor: 8,
                MaxStructure: 25,
                Slots: 6,
                Components: new List<Component>())
        };

        return new Mech(
            chassis: "Locust",
            model: "LCT-1V",
            tonnage: 20,
            walkMp: 8,
            partsData: partsData);
    }
}
