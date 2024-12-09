using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Utils.TechRules;

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
            result[PartLocation.Head].Should().Be(head);
            result[PartLocation.CenterTorso].Should().Be(centerTorso);
            result[PartLocation.LeftTorso].Should().Be(leftTorso);
            result[PartLocation.RightTorso].Should().Be(rightTorso);
            result[PartLocation.LeftArm].Should().Be(leftArm);
            result[PartLocation.RightArm].Should().Be(rightArm);
            result[PartLocation.LeftLeg].Should().Be(leftLeg);
            result[PartLocation.RightLeg].Should().Be(rightLeg);
        }

        [Fact]
        public void GetStructureValues_InvalidTonnage_ThrowsException()
        {
            // Act
            Action act = () => _provider.GetStructureValues(150);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(AmmoType.None, 0)]
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
        public void GetAmmoRounds_ReturnsCorrectValues(AmmoType ammoType, int expectedRounds)
        {
            // Act
            var result = _provider.GetAmmoRounds(ammoType);

            // Assert
            result.Should().Be(expectedRounds);
        }
    }
}
