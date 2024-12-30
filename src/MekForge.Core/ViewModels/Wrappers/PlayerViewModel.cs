﻿using Sanet.MekForge.Core.Data;
using Sanet.MVVM.Core.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MekForge.Core.Models.Game;

namespace Sanet.MekForge.Core.ViewModels.Wrappers;

    public class PlayerViewModel : BindableBase
    {
        public Player Player { get; }
        private UnitData? _selectedUnit;
        private Action? _onUnitAdded; // Delegate to notify when a unit is added
        public ObservableCollection<UnitData> Units { get; }
        public ObservableCollection<UnitData> AvailableUnits { get;}

        public UnitData? SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                SetProperty(ref _selectedUnit, value);
                NotifyPropertyChanged(nameof(CanAddUnit));
            }
        }

        public ICommand AddUnitCommand { get; }
        
        public string Name => Player.Name;

        public PlayerViewModel(Player player, IEnumerable<UnitData> availableUnits, Action? onUnitAdded = null)
        {
            Player = player;
            Units = [];
            AddUnitCommand = new AsyncCommand(AddUnit);
            AvailableUnits = new ObservableCollection<UnitData>(availableUnits);
            _onUnitAdded = onUnitAdded;
        }

        private Task AddUnit()
        {
            if (!SelectedUnit.HasValue) return Task.CompletedTask;
            var unit = SelectedUnit.Value;
            unit.Id= Guid.NewGuid();
            Units.Add(unit);
            _onUnitAdded?.Invoke();
            return Task.CompletedTask;
        }
        
        public bool CanAddUnit => SelectedUnit.HasValue; 
    }