using Sanet.MekForge.Core.Models.Units.Components.Engines;
using Sanet.MekForge.Core.Models.Units.Components.Internal;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public static class MechFactory
{
    public static Mech CreateLocustLCT1V()
    {
        var partsData = new Dictionary<PartLocation, UnitPartData>
        {
            [PartLocation.Head] = new(
                Name: "Head",
                MaxArmor: 9,
                MaxStructure: 3,
                Slots: 12,
                Components: [
                    new LifeSupport(),
                    new Sensors(),
                    new Cockpit()
                ]),

            [PartLocation.CenterTorso] = new(
                Name: "Center Torso",
                MaxArmor: 31,
                MaxStructure: 10,
                Slots: 12,
                Components: [
                    new Engine("Fusion Engine 160", new[] { 0, 1, 2, 7, 8, 9 }, 160),
                    new Gyro(),
                    new MediumLaser(),
                    new Ammo(AmmoType.MachineGun)
                ]),

            [PartLocation.LeftTorso] = new(
                Name: "Left Torso",
                MaxArmor: 21,
                MaxStructure: 7,
                Slots: 12,
                Components: []),

            [PartLocation.RightTorso] = new(
                Name: "Right Torso",
                MaxArmor: 21,
                MaxStructure: 7,
                Slots: 12,
                Components: []),

            [PartLocation.LeftArm] = new(
                Name: "Left Arm",
                MaxArmor: 16,
                MaxStructure: 6,
                Slots: 12,
                Components: [
                    new Shoulder(),
                    new UpperArmActuator(),
                    new LowerArmActuator(),
                    new HandActuator(),
                    new MachineGun()
                ]),

            [PartLocation.RightArm] = new(
                Name: "Right Arm",
                MaxArmor: 16,
                MaxStructure: 6,
                Slots: 12,
                Components: [
                    new Shoulder(),
                    new UpperArmActuator(),
                    new LowerArmActuator(),
                    new HandActuator(),
                    new MachineGun()
                ]),

            [PartLocation.LeftLeg] = new(
                Name: "Left Leg",
                MaxArmor: 21,
                MaxStructure: 7,
                Slots: 12,
                Components: [
                    new Hip(),
                    new UpperLegActuator(),
                    new LowerLegActuator(),
                    new FootActuator()
                ]),

            [PartLocation.RightLeg] = new(
                Name: "Right Leg",
                MaxArmor: 21,
                MaxStructure: 7,
                Slots: 12,
                Components: [
                    new Hip(),
                    new UpperLegActuator(),
                    new LowerLegActuator(),
                    new FootActuator()
                ])
        };

        return new Mech("Locust", "LCT-1V", 20, 8, partsData);
    }
}
