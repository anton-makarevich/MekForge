using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Avalonia.Controls
{
    public partial class UnitWeaponsPanel : UserControl
    {
        public UnitWeaponsPanel()
        {
            InitializeComponent();
        }

        public static readonly DirectProperty<UnitWeaponsPanel, Unit?> UnitProperty =
            AvaloniaProperty.RegisterDirect<UnitWeaponsPanel, Unit?>(
                nameof(Unit),
                o => o.Unit,
                (o, v) => o.Unit = v);

        private Unit? _unit;
        public Unit? Unit
        {
            get => _unit;
            set
            {
                SetAndRaise(UnitProperty, ref _unit, value);
                UpdateWeaponsList();
            }
        }

        private void UpdateWeaponsList()
        {
            if (Unit == null)
            {
                WeaponsList.ItemsSource = null;
                return;
            }

            var weapons = Unit.Parts
                .SelectMany(p => p.GetComponents<Weapon>())
                .ToList();

            WeaponsList.ItemsSource = weapons;
        }
    }
}
