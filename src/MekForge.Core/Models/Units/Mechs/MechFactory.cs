// using Sanet.MekForge.Core.Models.Units.Components.Engines;
// using Sanet.MekForge.Core.Models.Units.Components.Internal;
// using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
// using Sanet.MekForge.Core.Models.Units.Components.Weapons;
// using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
// using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
//
// namespace Sanet.MekForge.Core.Models.Units.Mechs;
//
// public static class MechFactory
// {
//     public static Mech CreateLocustLCT1V()
//     {
//         var head = new Head(PartLocation.Head, 8, 3);
//
//         var centerTorso = new CenterTorso(10, 2, 6);
//         centerTorso.TryAddComponent(new Engine("Fusion Engine 160", 160));
//         centerTorso.TryAddComponent(new MediumLaser());
//         centerTorso.TryAddComponent(new Ammo(AmmoType.MachineGun));
//
//         var leftTorso = new SideTorso(PartLocation.LeftTorso, 8, 2, 5);
//         var rightTorso = new SideTorso(PartLocation.RightTorso, 8, 2, 5);
//
//         var leftArm = new Arm(PartLocation.LeftArm, 4, 3);
//         leftArm.TryAddComponent(new UpperArmActuator());
//         leftArm.TryAddComponent(new MachineGun());
//
//         var rightArm = new Arm(PartLocation.RightArm, 4, 3);
//         rightArm.TryAddComponent(new UpperArmActuator());
//         rightArm.TryAddComponent(new MachineGun());
//
//         var leftLeg = new Leg(PartLocation.LeftLeg, 8, 4);
//         var rightLeg = new Leg(PartLocation.RightLeg, 8, 4);
//
//         var parts = new Dictionary<PartLocation, UnitPart>
//         {
//             [PartLocation.Head] = head,
//             [PartLocation.CenterTorso] = centerTorso,
//             [PartLocation.LeftTorso] = leftTorso,
//             [PartLocation.RightTorso] = rightTorso,
//             [PartLocation.LeftArm] = leftArm,
//             [PartLocation.RightArm] = rightArm,
//             [PartLocation.LeftLeg] = leftLeg,
//             [PartLocation.RightLeg] = rightLeg
//         };
//
//         return new Mech("Locust LCT-1V", 20, 8, parts);
//     }
// }
