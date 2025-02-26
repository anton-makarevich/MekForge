using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.ViewModels.Wrappers;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.ViewModels.Wrappers;

public class WeaponSelectionViewModelTests
{
    private WeaponSelectionViewModel _sut = null!;
    private Weapon _weapon = null!;
    private Unit? _target;
    private Action<Weapon, bool> _selectionChangedAction = null!;
    private ILocalizationService _localizationService = null!;

    public WeaponSelectionViewModelTests()
    {
        _weapon = Substitute.For<Weapon>();
        _weapon.Name.Returns("Test Weapon");
        _weapon.MinimumRange.Returns(0);
        _weapon.LongRange.Returns(12);
        _weapon.Damage.Returns(5);
        _weapon.Heat.Returns(3);
        
        _target = Substitute.For<Unit>();
        _localizationService = Substitute.For<ILocalizationService>();
        _localizationService.GetString(Arg.Any<string>()).Returns(x => x[0].ToString());
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = true;
        const bool isEnabled = true;
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        
        // Act
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction,
            _localizationService);
        
        // Assert
        _sut.Weapon.ShouldBe(_weapon);
        _sut.IsInRange.ShouldBe(isInRange);
        _sut.IsSelected.ShouldBe(isSelected);
        _sut.IsEnabled.ShouldBe(isEnabled);
        _sut.Target.ShouldBe(_target);
    }

    [Fact]
    public void IsSelected_WhenChanged_CallsSelectionChangedAction()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = false;
        const bool isEnabled = true;
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction,
            _localizationService);
        
        // Act
        _sut.IsSelected = true;
        
        // Assert
        _selectionChangedAction.Received(1).Invoke(_weapon, true);
    }

    [Fact]
    public void IsSelected_WhenChangedButDisabled_DoesNotCallSelectionChangedAction()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = false;
        const bool isEnabled = false;
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction,
            _localizationService);
        
        // Act
        _sut.IsSelected = true;
        
        // Assert
        _selectionChangedAction.DidNotReceive().Invoke(Arg.Any<Weapon>(), Arg.Any<bool>());
        _sut.IsSelected.ShouldBe(isSelected); // Should not have changed
    }

    [Fact]
    public void IsSelected_WhenSetToSameValue_DoesNotCallSelectionChangedAction()
    {
        // Arrange
        const bool isInRange = true;
        const bool isSelected = true;
        const bool isEnabled = true;
        
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction,
            _localizationService);
        
        // Act
        _sut.IsSelected = true;
        
        // Assert
        _selectionChangedAction.DidNotReceive().Invoke(Arg.Any<Weapon>(), Arg.Any<bool>());
    }

    [Fact]
    public void ModifiersBreakdown_CanBeSetAndRetrieved()
    {
        // Arrange
        CreateSut();
        var breakdown = CreateTestBreakdown(8);
        
        // Act
        _sut.ModifiersBreakdown = breakdown;
        
        // Assert
        _sut.ModifiersBreakdown.ShouldBe(breakdown);
    }
    
    [Fact]
    public void ModifiersBreakdown_NotifiesPropertyChanged()
    {
        // Arrange
        CreateSut();
        var breakdown = CreateTestBreakdown(8);
        
        var propertyChangedEvents = new List<string>();
        _sut.PropertyChanged += (sender, args) =>
        {
            propertyChangedEvents.Add(args.PropertyName);
        };
        
        // Act
        _sut.ModifiersBreakdown = breakdown;
        
        // Assert
        propertyChangedEvents.ShouldContain(nameof(WeaponSelectionViewModel.ModifiersBreakdown));
        propertyChangedEvents.ShouldContain(nameof(WeaponSelectionViewModel.HitProbability));
        propertyChangedEvents.ShouldContain(nameof(WeaponSelectionViewModel.HitProbabilityText));
        propertyChangedEvents.ShouldContain(nameof(WeaponSelectionViewModel.ModifiersDescription));
    }
    
    [Fact]
    public void HitProbability_CalculatesCorrectly()
    {
        // Arrange
        CreateSut();
        
        // Act & Assert - Valid target number
        var breakdown = CreateTestBreakdown(8);
        _sut.ModifiersBreakdown = breakdown;
        _sut.HitProbability.ShouldBeEquivalentTo(41.67); // Probability for target number 8
        
        // Act & Assert - Impossible target number
        breakdown = CreateTestBreakdown(13);
        _sut.ModifiersBreakdown = breakdown;
        _sut.HitProbability.ShouldBeEquivalentTo(0);
        
        // Act & Assert - No line of sight
        breakdown = CreateTestBreakdown(8, hasLineOfSight: false);
        _sut.ModifiersBreakdown = breakdown;
        _sut.HitProbability.ShouldBeEquivalentTo(0);
        
        // Act & Assert - Null breakdown
        _sut.ModifiersBreakdown = null;
        _sut.HitProbability.ShouldBeEquivalentTo(0);
    }
    
    [Fact]
    public void HitProbabilityText_FormatsCorrectly()
    {
        // Arrange
        CreateSut();
        
        // Act & Assert - Positive probability
        var breakdown = CreateTestBreakdown(8);
        _sut.ModifiersBreakdown = breakdown;
        _sut.HitProbabilityText.ShouldBe("42%");
        
        // Act & Assert - Zero probability
        breakdown = CreateTestBreakdown(13);
        _sut.ModifiersBreakdown = breakdown;
        _sut.HitProbabilityText.ShouldBe("N/A");
        
        // Act & Assert - No line of sight
        breakdown = CreateTestBreakdown(8, hasLineOfSight: false);
        _sut.ModifiersBreakdown = breakdown;
        _sut.HitProbabilityText.ShouldBe("N/A");
        
        // Act & Assert - Null breakdown
        _sut.ModifiersBreakdown = null;
        _sut.HitProbabilityText.ShouldBe("N/A");
    }
    
    [Fact]
    public void IsEnabled_ReturnsFalseWhenHitProbabilityIsZero()
    {
        // Arrange
        CreateSut(isEnabled: true);
        
        // Act - Set modifiers breakdown with impossible target number
        _sut.ModifiersBreakdown = CreateTestBreakdown(13);
        
        // Assert - Should be disabled despite isEnabled being true
        _sut.IsEnabled.ShouldBeFalse();
        
        // Act - Set modifiers breakdown with valid target number
        _sut.ModifiersBreakdown = CreateTestBreakdown(8);
        
        // Assert - Should be enabled
        _sut.IsEnabled.ShouldBeTrue();
    }
    
    [Fact]
    public void IsEnabled_ReturnsFalseWhenExplicitlyDisabled()
    {
        // Arrange
        CreateSut(isEnabled: false);
        
        // Act - Set modifiers breakdown with valid target number
        _sut.ModifiersBreakdown = CreateTestBreakdown(8);
        
        // Assert - Should still be disabled because isEnabled is false
        _sut.IsEnabled.ShouldBeFalse();
    }
    
    [Fact]
    public void ModifiersDescription_FormatsCorrectly()
    {
        // Arrange
        CreateSut();
        var breakdown = CreateTestBreakdown(8);
        
        // Act
        _sut.ModifiersBreakdown = breakdown;
        
        // Assert
        var description = _sut.ModifiersDescription;
        description.ShouldContain("Target Number: 8");
        
        // The Format method of each modifier should be called
        _localizationService.Received().GetString("TargetNumber");
        
        // Each modifier's Format method would be called, but we can't verify that directly
        // in this test since we're using real objects, not mocks
    }
    
    [Fact]
    public void ModifiersDescription_HandlesNoLineOfSight()
    {
        // Arrange
        CreateSut();
        var breakdown = CreateTestBreakdown(8, hasLineOfSight: false);
        
        // Act
        _sut.ModifiersBreakdown = breakdown;
        
        // Assert
        _sut.ModifiersDescription.ShouldBe("NoLineOfSight");
        _localizationService.Received().GetString("NoLineOfSight");
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
        bool isEnabled = true)
    {
        _selectionChangedAction = Substitute.For<Action<Weapon, bool>>();
        _sut = new WeaponSelectionViewModel(
            _weapon,
            isInRange,
            isSelected,
            isEnabled,
            _target,
            _selectionChangedAction,
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
            RangeModifier = new RangeAttackModifier { Value = otherModifiers, Range = WeaponRange.Medium, Distance = 5, WeaponName = "Test" },
            OtherModifiers = new List<AttackModifier>(),
            TerrainModifiers = new List<TerrainAttackModifier>(),
            HasLineOfSight = hasLineOfSight
        };
    }
}
