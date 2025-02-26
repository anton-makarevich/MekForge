using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Utils;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels.Wrappers;

public class WeaponSelectionViewModel : BindableBase
{
    private bool _isSelected;
    private readonly Action<Weapon, bool> _onSelectionChanged;
    private Unit? _target;
    private bool _isEnabled;
    private bool _isInRange;
    private ToHitBreakdown? _modifiersBreakdown;
    private readonly ILocalizationService _localizationService;

    public WeaponSelectionViewModel(
        Weapon weapon,
        bool isInRange,
        bool isSelected,
        bool isEnabled,
        Unit? target,
        Action<Weapon, bool> onSelectionChanged,
        ILocalizationService localizationService)
    {
        Weapon = weapon;
        IsInRange = isInRange;
        IsSelected = isSelected;
        IsEnabled = isEnabled;
        Target = target;
        _onSelectionChanged = onSelectionChanged;
        _localizationService = localizationService;
    }

    public Weapon Weapon { get; }

    public bool IsInRange
    {
        get => _isInRange;
        set => SetProperty(ref _isInRange, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (!IsEnabled) return;
            if (value==_isSelected) return;
            SetProperty(ref _isSelected, value);
            _onSelectionChanged(Weapon, value);
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled && HitProbability > 0;
        set => SetProperty(ref _isEnabled, value);
    }

    public Unit? Target
    {
        get => _target;
        set => SetProperty(ref _target, value);
    }
    
    /// <summary>
    /// Gets or sets the detailed breakdown of hit modifiers
    /// </summary>
    public ToHitBreakdown? ModifiersBreakdown
    {
        get => _modifiersBreakdown;
        set
        {
            SetProperty(ref _modifiersBreakdown, value);
            NotifyPropertyChanged(nameof(HitProbability));
            NotifyPropertyChanged(nameof(HitProbabilityText));
            NotifyPropertyChanged(nameof(ModifiersDescription));
        }
    }
    
    /// <summary>
    /// Gets the hit probability as a value between 0 and 100
    /// </summary>
    public double HitProbability => 
        ModifiersBreakdown is { HasLineOfSight: true, Total: <= 12 }
            ? DiceUtils.Calculate2d6Probability(ModifiersBreakdown.Total)
            : 0;
    
    /// <summary>
    /// Gets the formatted hit probability string for display
    /// </summary>
    public string HitProbabilityText => HitProbability <= 0 ? "N/A" : $"{HitProbability:F0}%";
    
    /// <summary>
    /// Gets a formatted string with the modifier breakdown details
    /// </summary>
    public string ModifiersDescription
    {
        get
        {
            if (ModifiersBreakdown == null)
                return string.Empty;

            if (!ModifiersBreakdown.HasLineOfSight)
                return _localizationService.GetString("Attack_NoLineOfSight");
            
            var lines = new List<string>
            {
                $"{_localizationService.GetString("Attack_TargetNumber")}: {ModifiersBreakdown.Total}"
            };
            lines.AddRange(ModifiersBreakdown.AllModifiers.Select(modifier => modifier.Format(_localizationService)));

            // Add all modifiers using their Format method
            return string.Join(Environment.NewLine, lines);
        }
    }

    // Additional properties for UI display
    public string Name => Weapon.Name;
    public string RangeInfo => $"{Weapon.MinimumRange}-{Weapon.LongRange}";
    public string Damage => $"\u26a1: {Weapon.Damage}";
    public string Heat => $"\uf06d: {Weapon.Heat}";
}
