using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Sanet.MekForge.Core.Models.Units.Mechs;
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
        _sut.HitProbability = 10;
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
        _sut.Damage.ShouldBe("Damage: 5");
    }

    [Fact]
    public void Heat_ReturnsCorrectFormat()
    {
        // Arrange
        CreateSut();

        // Act & Assert
        _sut.Heat.ShouldBe("Heat: 3");
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
         _sut.HitProbability = 70;
    
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
    public void HitProbability_CanBeSetAndRetrieved()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = false;
        const bool isEnabled = true;
        const double expectedProbability = 75.0;
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction)
        {
            // Act
            HitProbability = expectedProbability
        };

        // Assert
        _sut.HitProbability.ShouldBe(expectedProbability);
    }
    
    [Fact]
    public void HitProbability_NotifiesPropertyChanged()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = false;
        const bool isEnabled = true;
        const double initialProbability = 50.0;
        const double updatedProbability = 75.0;
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction)
        {
            HitProbability = initialProbability
        };

        var propertyChangedRaised = false;
        _sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(WeaponSelectionViewModel.HitProbability))
                propertyChangedRaised = true;
        };
        
        // Act
        _sut.HitProbability = updatedProbability;
        
        // Assert
        propertyChangedRaised.ShouldBeTrue();
        _sut.HitProbability.ShouldBe(updatedProbability);
    }
    
    [Fact]
    public void HitProbabilityText_FormatsCorrectly()
    {
        // Arrange
        CreateSut();
        
        // Act & Assert - Positive probability
        _sut.HitProbability = 75.5;
        _sut.HitProbabilityText.ShouldBe("76%");
        
        // Act & Assert - Zero probability
        _sut.HitProbability = 0;
        _sut.HitProbabilityText.ShouldBe("N/A");
        
        // Act & Assert - Negative probability (should not happen in practice)
        _sut.HitProbability = -1;
        _sut.HitProbabilityText.ShouldBe("N/A");
    }
    
    [Fact]
    public void IsEnabled_ReturnsFalseWhenHitProbabilityIsZero()
    {
        // Arrange
        CreateSut(isEnabled: true);
        
        // Act - Set hit probability to zero
        _sut.HitProbability = 0;
        
        // Assert - Should be disabled despite isEnabled being true
        _sut.IsEnabled.ShouldBeFalse();
        
        // Act - Set hit probability to positive value
        _sut.HitProbability = 50;
        
        // Assert - Should be enabled
        _sut.IsEnabled.ShouldBeTrue();
    }
    
    [Fact]
    public void IsEnabled_ReturnsFalseWhenExplicitlyDisabled()
    {
        // Arrange
        CreateSut(isEnabled: false);
        
        // Act - Set hit probability to positive value
        _sut.HitProbability = 50;
        
        // Assert - Should still be disabled because isEnabled is false
        _sut.IsEnabled.ShouldBeFalse();
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
            (w, s) => _selectionChangedAction?.Invoke(w, s));
    }
}
