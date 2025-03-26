using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

// Server command skeletons to enable the Transport architecture 
// These are minimal implementations to satisfy the CommandTransportAdapter references

public class PlayerJoinedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Player Joined";
}

public class PlayerStatusUpdatedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Player Status Updated";
}

public class PhaseChangedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Phase Changed";
}

public class UnitDeployedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Unit Deployed";
}

public class ActivePlayerChangedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Active Player Changed";
}

public class UnitMovedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Unit Moved";
}

public class WeaponAttackResolutionCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Weapon Attack Resolution";
}

public class HeatUpdatedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Heat Updated";
}
