using Sanet.MekForge.Core.Data;
using Sanet.MVVM.Core.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MekForge.Core.Models.Game;

namespace Sanet.MekForge.Core.ViewModels.Wrappers;

    public class PlayerViewModel : BindableBase
    {
        public Player Player { get; }
        public ObservableCollection<UnitData> Units { get; }

        public ICommand AddUnitCommand { get; }
        
        public string Name => Player.Name;

        public PlayerViewModel(Player player)
        {
            Player = player;
            Units = new ObservableCollection<UnitData>();
            AddUnitCommand = new AsyncCommand<UnitData>(AddUnit);
        }

        private Task AddUnit(UnitData unit)
        {
            Units.Add(unit);
            return Task.CompletedTask;
        }
    }