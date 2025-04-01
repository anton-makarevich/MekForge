using Avalonia;
using Avalonia.Controls;
using Sanet.MakaMek.Core.Models.Units;
using Sanet.MakaMek.Core.Models.Units.Mechs;

namespace Sanet.MakaMek.Avalonia.Controls
{
    public partial class UnitHealthIndicator : UserControl
    {
        public UnitHealthIndicator()
        {
            InitializeComponent();
        }

        public static readonly DirectProperty<UnitHealthIndicator, Unit?> UnitProperty =
            AvaloniaProperty.RegisterDirect<UnitHealthIndicator, Unit?>(
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
                UpdateHealthBars();
            }
        }

        private void UpdateHealthBars()
        {
            if (Unit == null)
            {
                ClearHealthBars();
                return;
            }

            foreach (var part in Unit.Parts)
            {
                switch (part.Location)
                {
                    case PartLocation.Head:
                        UpdateBar(HeadArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(HeadStructure, part.CurrentStructure, part.MaxStructure);
                        break;
                    case PartLocation.CenterTorso:
                        UpdateBar(CenterTorsoArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(CenterTorsoStructure, part.CurrentStructure, part.MaxStructure);
                        if (part is Torso torso)
                        {
                            UpdateBar(CenterTorsoRearArmor, torso.CurrentRearArmor, torso.MaxRearArmor);
                        }
                        break;
                    case PartLocation.LeftTorso:
                        UpdateBar(LeftTorsoArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(LeftTorsoStructure, part.CurrentStructure, part.MaxStructure);
                        if (part is Torso torsoLT)
                        {
                            UpdateBar(LeftTorsoRearArmor, torsoLT.CurrentRearArmor, torsoLT.MaxRearArmor);
                        }
                        break;
                    case PartLocation.RightTorso:
                        UpdateBar(RightTorsoArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(RightTorsoStructure, part.CurrentStructure, part.MaxStructure);
                        if (part is Torso torsoRT)
                        {
                            UpdateBar(RightTorsoRearArmor, torsoRT.CurrentRearArmor, torsoRT.MaxRearArmor);
                        }
                        break;
                    case PartLocation.LeftArm:
                        UpdateBar(LeftArmArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(LeftArmStructure, part.CurrentStructure, part.MaxStructure);
                        break;
                    case PartLocation.RightArm:
                        UpdateBar(RightArmArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(RightArmStructure, part.CurrentStructure, part.MaxStructure);
                        break;
                    case PartLocation.LeftLeg:
                        UpdateBar(LeftLegArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(LeftLegStructure, part.CurrentStructure, part.MaxStructure);
                        break;
                    case PartLocation.RightLeg:
                        UpdateBar(RightLegArmor, part.CurrentArmor, part.MaxArmor);
                        UpdateBar(RightLegStructure, part.CurrentStructure, part.MaxStructure);
                        break;
                }
            }
        }

        private void UpdateBar(ProgressBar? bar, int current, int max)
        {
            if (bar == null) return;
            bar.Value = current;
            bar.Maximum = max;
        }

        private void ClearHealthBars()
        {
            // Head
            UpdateBar(HeadArmor, 0, 1);
            UpdateBar(HeadStructure, 0, 1);

            // Center Torso
            UpdateBar(CenterTorsoArmor, 0, 1);
            UpdateBar(CenterTorsoStructure, 0, 1);
            UpdateBar(CenterTorsoRearArmor, 0, 1);

            // Left Torso
            UpdateBar(LeftTorsoArmor, 0, 1);
            UpdateBar(LeftTorsoStructure, 0, 1);
            UpdateBar(LeftTorsoRearArmor, 0, 1);

            // Right Torso
            UpdateBar(RightTorsoArmor, 0, 1);
            UpdateBar(RightTorsoStructure, 0, 1);
            UpdateBar(RightTorsoRearArmor, 0, 1);

            // Left Arm
            UpdateBar(LeftArmArmor, 0, 1);
            UpdateBar(LeftArmStructure, 0, 1);

            // Right Arm
            UpdateBar(RightArmArmor, 0, 1);
            UpdateBar(RightArmStructure, 0, 1);

            // Left Leg
            UpdateBar(LeftLegArmor, 0, 1);
            UpdateBar(LeftLegStructure, 0, 1);

            // Right Leg
            UpdateBar(RightLegArmor, 0, 1);
            UpdateBar(RightLegStructure, 0, 1);
        }
    }
}
