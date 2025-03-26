using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

// Client command skeletons to enable the Transport architecture 
// These are minimal implementations to satisfy the CommandTransportAdapter references

public class JoinGameCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Join Game";
}

public class SetPlayerReadyCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Set Player Ready";
}

public class DeployUnitCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Deploy Unit";
}

public class MoveUnitCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Move Unit";
}

public class WeaponConfigurationCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Weapon Configuration";
}

public class WeaponAttackDeclarationCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Weapon Attack Declaration";
}

public class PhysicalAttackCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Physical Attack";
}

public class TurnEndedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Turn Ended";
}

public class UpdatePlayerStatusCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Format(ILocalizationService localizationService, IGame game) => "Update Player Status";
}
