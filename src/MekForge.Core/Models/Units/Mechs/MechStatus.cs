namespace Sanet.MekForge.Core.Models.Units.Mechs;

[System.Flags]
public enum MechStatus
{
    None = 0,
    Active = 1,
    Shutdown = 2,
    Prone = 4,
    Immobile = 8,
    PoweredDown = 16
}
