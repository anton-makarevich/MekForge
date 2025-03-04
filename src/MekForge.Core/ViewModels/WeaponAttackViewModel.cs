using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.ViewModels;

public class WeaponAttackViewModel
{
    public HexCoordinates From { get; set; }
    public HexCoordinates To { get; set; }
    public string WeaponName { get; set; } = string.Empty;
    public string AttackerTint { get; set; } = "#000000";
    public int LineOffset { get; set; }
}
