using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels.Wrappers;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.ViewModels.Wrappers;

public class WeaponSelectionViewModelTests
{
    private readonly Weapon _weapon;
    private readonly Mech _target;
    private Action<Weapon, bool>? _selectionChangedAction;
    private WeaponSelectionViewModel _sut = null!;
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

    public WeaponSelectionViewModelTests()
    {
        _weapon = new MediumLaser();
        
        // Create a test mech using MechFactory
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
        var mechFactory = new MechFactory(structureValueProvider);
        var mechData = MechFactoryTests.CreateDummyMechData();
        _target = mechFactory.Create(mechData);
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = false;
        const bool isEnabled = true;

        // Act
        CreateSut(isInRange, isSelected, isEnabled, _target);

        // Assert
        _sut.Weapon.ShouldBe(_weapon);
        _sut.IsInRange.ShouldBe(isInRange);
        _sut.IsSelected.ShouldBe(isSelected);
        _sut.IsEnabled.ShouldBeFalse(); //default HitProbability is zero
        _sut.Target.ShouldBe(_target);
        _sut.ModifiersBreakdown = CreateTestBreakdown(5);
        _sut.IsEnabled.ShouldBe(isEnabled);
    }

    [Fact]
    public void Name_ReturnsWeaponName()
    {
        // Arrange
        CreateSut();

        // Act & Assert
        _sut.Name.ShouldBe("Medium Laser");
    }

    [Fact]
    public void RangeInfo_ReturnsCorrectFormat()
    {
        // Arrange
        CreateSut();

        // Act & Assert
        _sut.RangeInfo.ShouldBe("0-9");
    }

    [Fact]
    public void Damage_ReturnsCorrectFormat()
    {
        // Arrange
        CreateSut();

        // Act & Assert
        _sut.Damage.ShouldBe("\u26a1: 5");
    }

    [Fact]
    public void Heat_ReturnsCorrectFormat()
    {
        // Arrange
        CreateSut();

        // Act & Assert
        _sut.Heat.ShouldBe("\uf06d: 3");
    }

    [Fact]
    public void IsSelected_WhenDisabled_CannotBeSetToTrue()
    {
        // Arrange
        CreateSut(isEnabled: false);

        // Act
        _sut.IsSelected = true;

        // Assert
        _sut.IsSelected.ShouldBeFalse();
        _selectionChangedAction.ShouldBeNull();
    }

    [Fact]
     public void IsSelected_WhenEnabled_CanBeChanged()
     {
         // Arrange
         CreateSut(isEnabled: true);
         var wasActionCalled = false;
         var expectedValue = true;
         _selectionChangedAction = (weapon, selected) =>
         {
             weapon.ShouldBe(_weapon);
             selected.ShouldBe(expectedValue);
             wasActionCalled = true;
         };
         _sut.ModifiersBreakdown = CreateTestBreakdown(5);
    
         // Act
         _sut.IsSelected = true;
    
         // Assert
         _sut.IsSelected.ShouldBeTrue();
         wasActionCalled.ShouldBeTrue();
    
         // Test deselection
         expectedValue = false;
         wasActionCalled = false;
         _sut.IsSelected = false;
    
         _sut.IsSelected.ShouldBeFalse();
         wasActionCalled.ShouldBeTrue();
     }

    [Fact]
    public void ModifiersBreakdown_CanBeSetAndRetrieved()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = false;
        const bool isEnabled = true;
        var testBreakdown = CreateTestBreakdown(8);
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction,
            _localizationService
            )
        {
            // Act
            ModifiersBreakdown = testBreakdown
        };

        // Assert
        _sut.ModifiersBreakdown.ShouldBe(testBreakdown);
    }
    
    [Fact]
    public void ModifiersBreakdown_NotifiesPropertyChanged()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = false;
        const bool isEnabled = true;
        var initialBreakdown = CreateTestBreakdown(8);
        var updatedBreakdown = CreateTestBreakdown(5);
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction,
            _localizationService)
        {
            ModifiersBreakdown = initialBreakdown
        };

        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(WeaponSelectionViewModel.HitProbability))
                propertyChangedRaised = true;
        };
        
        // Act
        _sut.ModifiersBreakdown = updatedBreakdown;
        
        // Assert
        propertyChangedRaised.ShouldBeTrue();
        _sut.ModifiersBreakdown.ShouldBe(updatedBreakdown);
    }
    
    [Fact]
    public void HitProbabilityText_FormatsCorrectly()
    {
        // Arrange
        CreateSut();
        
        // Act & Assert - Positive probability
        _sut.ModifiersBreakdown = CreateTestBreakdown(6);
        _sut.HitProbabilityText.ShouldBe("72%");
        
        // Act & Assert - Zero probability
        _sut.ModifiersBreakdown = CreateTestBreakdown(13,false);
        _sut.HitProbabilityText.ShouldBe("N/A");
    }
    
    [Fact]
    public void IsEnabled_ReturnsFalseWhenHitProbabilityIsZero()
    {
        // Arrange
        CreateSut(isEnabled: true);
        
        // Act - Set hit probability to zero
        _sut.ModifiersBreakdown = CreateTestBreakdown(13,false);
        
        // Assert - Should be disabled despite isEnabled being true
        _sut.IsEnabled.ShouldBeFalse();
        
        // Act - Set hit probability to positive value
        _sut.ModifiersBreakdown = CreateTestBreakdown(6);
        
        // Assert - Should be enabled
        _sut.IsEnabled.ShouldBeTrue();
    }
    
    [Fact]
    public void IsEnabled_ReturnsFalseWhenExplicitlyDisabled()
    {
        // Arrange
        CreateSut(isEnabled: false);
        
        // Act - Set hit probability to positive value
        _sut.ModifiersBreakdown = CreateTestBreakdown(6);
        
        // Assert - Should still be disabled because isEnabled is false
        _sut.IsEnabled.ShouldBeFalse();
    }
    
    [Fact]
    public void ModifiersDescription_FormatsCorrectly()
    {
        // Arrange
        CreateSut();
        var breakdown = CreateTestBreakdown(8);
        _localizationService.GetString("Attack_TargetNumber").Returns("Target Number");
        
        // Act
        _sut.ModifiersBreakdown = breakdown;
        
        // Assert
        var description = _sut.ModifiersDescription;
        description.ShouldContain("Target Number: 8");
        
        // The Format method of each modifier should be called
        _localizationService.Received().GetString("Attack_TargetNumber");
        
        // Each modifier's Format method would be called, but we can't verify that directly
        // in this test since we're using real objects, not mocks
    }
    
    [Fact]
    public void ModifiersDescription_HandlesNoLineOfSight()
    {
        // Arrange
        CreateSut();
        var breakdown = CreateTestBreakdown(8, hasLineOfSight: false);
        _localizationService.GetString("Attack_NoLineOfSight").Returns("No Line Of Sight");
        
        // Act
        _sut.ModifiersBreakdown = breakdown;
        
        // Assert
        _sut.ModifiersDescription.ShouldBe("No Line Of Sight");
        _localizationService.Received().GetString("Attack_NoLineOfSight");
    }
    
    [Fact]
    public void ModifiersDescription_HandlesNullBreakdown()
    {
        // Arrange
        CreateSut();
        
        // Act
        _sut.ModifiersBreakdown = null;
        
        // Assert
        _sut.ModifiersDescription.ShouldBe(string.Empty);
    }

    private void CreateSut(
        bool isInRange = true,
        bool isSelected = false,
        bool isEnabled = true,
        Unit? target = null)
    {
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            target,
            (w, s) => _selectionChangedAction?.Invoke(w, s),
            _localizationService);
    }

    private ToHitBreakdown CreateTestBreakdown(int total, bool hasLineOfSight = true)
    {
        // Create a breakdown that will result in the specified total
        var gunneryValue = 4;
        var otherModifiers = total - gunneryValue;

        return new ToHitBreakdown
        {
            GunneryBase = new GunneryAttackModifier { Value = gunneryValue },
            AttackerMovement = new AttackerMovementModifier { Value = 0, MovementType = MovementType.StandingStill },

            TargetMovement = new TargetMovementModifier { Value = 0, HexesMoved = 0 },
            RangeModifier = new RangeAttackModifier
                { Value = otherModifiers, Range = WeaponRange.Medium, Distance = 5, WeaponName = "Test" },
            OtherModifiers = [],
            TerrainModifiers = [],
            HasLineOfSight = hasLineOfSight
        };
    }
}
