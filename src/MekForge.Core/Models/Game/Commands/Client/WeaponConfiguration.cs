namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public enum WeaponConfigurationType
{
    TorsoRotation,
    ArmsFlip
}

public class WeaponConfiguration
{
    public WeaponConfigurationType Type { get; set; }
    public int Value { get; set; }
}
