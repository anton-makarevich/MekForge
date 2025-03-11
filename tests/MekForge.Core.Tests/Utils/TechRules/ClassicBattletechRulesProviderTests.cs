using Sanet.MekForge.Core.Models.Game.Combat;
using Shouldly;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Tests.Utils.TechRules
{
    public class ClassicBattletechRulesProviderTests
    {
        private readonly IRulesProvider _provider;

        public ClassicBattletechRulesProviderTests()
        {
            _provider = new ClassicBattletechRulesProvider();
        }

        [Theory]
        [InlineData(20, 3, 6, 5, 5, 3, 3, 4, 4)]
        [InlineData(25, 3, 8, 6, 6, 4, 4, 6, 6)]
        [InlineData(30, 3, 10, 7, 7, 5, 5, 7, 7)]
        [InlineData(35, 3, 11, 8, 8, 6, 6, 8, 8)]
        [InlineData(40, 3, 12, 10, 10, 6, 6, 10, 10)]
        [InlineData(45, 3, 14, 11, 11, 7, 7, 11, 11)]
        [InlineData(50, 3, 16, 12, 12, 8, 8, 12, 12)]
        [InlineData(55, 3, 18, 13, 13, 9, 9, 13, 13)]
        [InlineData(60, 3, 20, 14, 14, 10, 10, 14, 14)]
        [InlineData(65, 3, 21, 15, 15, 10, 10, 15, 15)]
        [InlineData(70, 3, 22, 15, 15, 11, 11, 15, 15)]
        [InlineData(75, 3, 23, 16, 16, 12, 12, 16, 16)]
        [InlineData(80, 3, 25, 17, 17, 13, 13, 17, 17)]
        [InlineData(85, 3, 27, 18, 18, 14, 14, 18, 18)]
        [InlineData(90, 3, 29, 19, 19, 15, 15, 19, 19)]
        [InlineData(95, 3, 30, 20, 20, 16, 16, 20, 20)]
        [InlineData(100, 3, 31, 21, 21, 17, 17, 21, 21)]
        public void GetStructureValues_ValidTonnage_ReturnsExpectedValues(int tonnage, int head, int centerTorso, int leftTorso, int rightTorso, int leftArm, int rightArm, int leftLeg, int rightLeg)
        {
            // Act
            var result = _provider.GetStructureValues(tonnage);

            // Assert
            result[PartLocation.Head].ShouldBe(head);
            result[PartLocation.CenterTorso].ShouldBe(centerTorso);
            result[PartLocation.LeftTorso].ShouldBe(leftTorso);
            result[PartLocation.RightTorso].ShouldBe(rightTorso);
            result[PartLocation.LeftArm].ShouldBe(leftArm);
            result[PartLocation.RightArm].ShouldBe(rightArm);
            result[PartLocation.LeftLeg].ShouldBe(leftLeg);
            result[PartLocation.RightLeg].ShouldBe(rightLeg);
        }

        [Fact]
        public void GetStructureValues_InvalidTonnage_ThrowsException()
        {
            // Act & Assert
            Should.Throw<ArgumentOutOfRangeException>(()=>_provider.GetStructureValues(150));
        }

        [Theory]
        [InlineData(AmmoType.MachineGun, 200)]
        [InlineData(AmmoType.AC2, 45)]
        [InlineData(AmmoType.AC5, 20)]
        [InlineData(AmmoType.AC10, 10)]
        [InlineData(AmmoType.AC20, 5)]
        [InlineData(AmmoType.LRM5, 24)]
        [InlineData(AmmoType.LRM10, 12)]
        [InlineData(AmmoType.LRM15, 8)]
        [InlineData(AmmoType.LRM20, 6)]
        [InlineData(AmmoType.SRM2, 50)]
        [InlineData(AmmoType.SRM4, 25)]
        [InlineData(AmmoType.SRM6, 15)]
        public void GetAmmoRounds_ReturnsExpectedValues(AmmoType ammoType, int expectedRounds)
        {
            _provider.GetAmmoRounds(ammoType).ShouldBe(expectedRounds);
        }

        [Theory]
        [InlineData(MovementType.StandingStill, 0)]
        [InlineData(MovementType.Walk, 1)]
        [InlineData(MovementType.Run, 2)]
        [InlineData(MovementType.Jump, 3)]
        [InlineData(MovementType.Prone, 2)]
        public void GetAttackerMovementModifier_ReturnsExpectedValues(MovementType movementType, int expectedModifier)
        {
            _provider.GetAttackerMovementModifier(movementType).ShouldBe(expectedModifier);
        }

        [Theory]
        [InlineData(0, 0)]  // 0-2 hexes: no modifier
        [InlineData(2, 0)]
        [InlineData(3, 1)]  // 3-4 hexes: +1
        [InlineData(4, 1)]
        [InlineData(5, 2)]  // 5-6 hexes: +2
        [InlineData(6, 2)]
        [InlineData(7, 3)]  // 7-9 hexes: +3
        [InlineData(9, 3)]
        [InlineData(10, 4)] // 10-17 hexes: +4
        [InlineData(17, 4)]
        [InlineData(18, 5)] // 18-24 hexes: +5
        [InlineData(24, 5)]
        [InlineData(25, 6)] // 25+ hexes: +6
        [InlineData(30, 6)]
        public void GetTargetMovementModifier_ReturnsExpectedValues(int hexesMoved, int expectedModifier)
        {
            _provider.GetTargetMovementModifier(hexesMoved).ShouldBe(expectedModifier);
        }

        [Theory]
        [InlineData(WeaponRange.Minimum,6,6, 1)]
        [InlineData(WeaponRange.Minimum,6,5, 2)]
        [InlineData(WeaponRange.Short,1,1,0)]
        [InlineData(WeaponRange.Medium,1,1, 2)]
        [InlineData(WeaponRange.Long,1,1, 4)]
        [InlineData(WeaponRange.OutOfRange,1,1, ToHitBreakdown.ImpossibleRoll)]
        public void GetRangeModifier_ReturnsExpectedValues(WeaponRange range, int rangeValue, int distance, int expectedModifier)
        {
            _provider.GetRangeModifier(range,rangeValue,distance).ShouldBe(expectedModifier);
        }

        [Theory]
        [InlineData(0, 0)]   // 0-7 heat: no modifier
        [InlineData(7, 0)]
        [InlineData(8, 1)]   // 8-12 heat: +1
        [InlineData(12, 1)]
        [InlineData(13, 2)]  // 13-16 heat: +2
        [InlineData(16, 2)]
        [InlineData(17, 3)]  // 17-23 heat: +3
        [InlineData(23, 3)]
        [InlineData(24, 4)]  // 24+ heat: +4
        [InlineData(25, 4)]  // >24 heat: +4
        public void GetHeatModifier_ReturnsExpectedValues(int currentHeat, int expectedModifier)
        {
            _provider.GetHeatModifier(currentHeat).ShouldBe(expectedModifier);
        }

        [Theory]
        [InlineData("LightWoods", 1)]
        [InlineData("HeavyWoods", 2)]
        [InlineData("unknown_terrain", 0)]
        public void GetTerrainToHitModifier_ReturnsExpectedValues(string terrainId, int expectedModifier)
        {
            _provider.GetTerrainToHitModifier(terrainId).ShouldBe(expectedModifier);
        }

        [Fact]
        public void GetAttackerMovementModifier_InvalidMovementType_ThrowsArgumentException()
        {
            var invalidType = (MovementType)999;
            Should.Throw<ArgumentException>(() => _provider.GetAttackerMovementModifier(invalidType));
        }

        [Fact]
        public void GetRangeModifier_InvalidRange_ThrowsArgumentException()
        {
            var invalidRange = (WeaponRange)999;
            Should.Throw<ArgumentException>(() => _provider.GetRangeModifier(invalidRange,999,999));
        }

        [Theory]
        [InlineData(true, 1)]   // Front arc: +1 modifier
        [InlineData(false, 2)]  // Other arc: +2 modifier
        public void GetSecondaryTargetModifier_ReturnsExpectedValues(bool isFrontArc, int expectedModifier)
        {
            _provider.GetSecondaryTargetModifier(isFrontArc).ShouldBe(expectedModifier);
        }

        #region Hit Location Tests

        [Theory]
        [InlineData(2, PartLocation.CenterTorso)]  // Critical hit
        [InlineData(3, PartLocation.RightArm)]
        [InlineData(4, PartLocation.RightArm)]
        [InlineData(5, PartLocation.RightLeg)]
        [InlineData(6, PartLocation.RightTorso)]
        [InlineData(7, PartLocation.CenterTorso)]
        [InlineData(8, PartLocation.LeftTorso)]
        [InlineData(9, PartLocation.LeftLeg)]
        [InlineData(10, PartLocation.LeftArm)]
        [InlineData(11, PartLocation.LeftArm)]
        [InlineData(12, PartLocation.Head)]
        public void GetHitLocation_FrontAttack_ReturnsCorrectLocation(int diceResult, PartLocation expectedLocation)
        {
            // Act
            var result = _provider.GetHitLocation(diceResult, FiringArc.Forward);

            // Assert
            result.ShouldBe(expectedLocation);
        }

        [Theory]
        [InlineData(2, PartLocation.CenterTorso)]  // Critical hit
        [InlineData(3, PartLocation.RightArm)]
        [InlineData(4, PartLocation.RightArm)]
        [InlineData(5, PartLocation.RightLeg)]
        [InlineData(6, PartLocation.RightTorso)]
        [InlineData(7, PartLocation.CenterTorso)]
        [InlineData(8, PartLocation.LeftTorso)]
        [InlineData(9, PartLocation.LeftLeg)]
        [InlineData(10, PartLocation.LeftArm)]
        [InlineData(11, PartLocation.LeftArm)]
        [InlineData(12, PartLocation.Head)]
        public void GetHitLocation_RearAttack_ReturnsCorrectLocation(int diceResult, PartLocation expectedLocation)
        {
            // Act
            var result = _provider.GetHitLocation(diceResult, FiringArc.Rear);

            // Assert
            result.ShouldBe(expectedLocation);
        }

        [Theory]
        [InlineData(2, PartLocation.LeftTorso)]  // Critical hit
        [InlineData(3, PartLocation.LeftLeg)]
        [InlineData(4, PartLocation.LeftArm)]
        [InlineData(5, PartLocation.LeftArm)]
        [InlineData(6, PartLocation.LeftLeg)]
        [InlineData(7, PartLocation.LeftTorso)]
        [InlineData(8, PartLocation.CenterTorso)]
        [InlineData(9, PartLocation.RightTorso)]
        [InlineData(10, PartLocation.RightArm)]
        [InlineData(11, PartLocation.RightLeg)]
        [InlineData(12, PartLocation.Head)]
        public void GetHitLocation_LeftAttack_ReturnsCorrectLocation(int diceResult, PartLocation expectedLocation)
        {
            // Act
            var result = _provider.GetHitLocation(diceResult, FiringArc.Left);

            // Assert
            result.ShouldBe(expectedLocation);
        }

        [Theory]
        [InlineData(2, PartLocation.RightTorso)]  // Critical hit
        [InlineData(3, PartLocation.RightLeg)]
        [InlineData(4, PartLocation.RightArm)]
        [InlineData(5, PartLocation.RightArm)]
        [InlineData(6, PartLocation.RightLeg)]
        [InlineData(7, PartLocation.RightTorso)]
        [InlineData(8, PartLocation.CenterTorso)]
        [InlineData(9, PartLocation.LeftTorso)]
        [InlineData(10, PartLocation.LeftArm)]
        [InlineData(11, PartLocation.LeftLeg)]
        [InlineData(12, PartLocation.Head)]
        public void GetHitLocation_RightAttack_ReturnsCorrectLocation(int diceResult, PartLocation expectedLocation)
        {
            // Act
            var result = _provider.GetHitLocation(diceResult, FiringArc.Right);

            // Assert
            result.ShouldBe(expectedLocation);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(13)]
        [InlineData(20)]
        public void GetHitLocation_InvalidDiceResult_ThrowsArgumentOutOfRangeException(int invalidDiceResult)
        {
            // Act & Assert
            Should.Throw<ArgumentOutOfRangeException>(() => _provider.GetHitLocation(invalidDiceResult, FiringArc.Forward));
            Should.Throw<ArgumentOutOfRangeException>(() => _provider.GetHitLocation(invalidDiceResult, FiringArc.Left));
            Should.Throw<ArgumentOutOfRangeException>(() => _provider.GetHitLocation(invalidDiceResult, FiringArc.Right));
            Should.Throw<ArgumentOutOfRangeException>(() => _provider.GetHitLocation(invalidDiceResult, FiringArc.Rear));
        }

        [Fact]
        public void GetHitLocation_InvalidAttackDirection_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Should.Throw<ArgumentOutOfRangeException>(() => _provider.GetHitLocation(7, (FiringArc)999));
        }

        #endregion
    }
}
