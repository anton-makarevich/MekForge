namespace Sanet.MekForge.Core.Models.Units;

[System.Flags]
public enum UnitStatus
{
    None = 0,
    Active = 1,
    Shutdown = 2,
    Prone = 4,
    Immobile = 8,
    PoweredDown = 16,
    Destroyed = 32
}
