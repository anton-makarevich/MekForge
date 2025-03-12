using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record struct WeaponAttackResolutionCommand : IGameCommand
{
    public required Guid PlayerId { get; init; }
    public required Guid AttackerId { get; init; }
    public required WeaponData WeaponData { get; init; }
    public required Guid TargetId { get; init; }
    public required AttackResolutionData ResolutionData { get; init; }

    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; }

    public string Format(ILocalizationService localizationService, IGame game)
    {
        var command = this;
        var player = game.Players.FirstOrDefault(p => p.Id == command.PlayerId);
        var attacker = player?.Units.FirstOrDefault(u => u.Id == command.AttackerId);
        var weapon = attacker?.GetMountedComponentAtLocation<Weapon>(WeaponData.Location,WeaponData.Slots);
        var target = game.Players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == command.TargetId);
        var targetPlayer = target?.Owner;
        
        if (player == null || attacker == null || weapon == null || target == null || targetPlayer == null)
        {
            return string.Empty;
        }

        var template = ResolutionData.IsHit ? 
            localizationService.GetString("Command_WeaponAttackResolution_Hit") :
            localizationService.GetString("Command_WeaponAttackResolution_Miss");

        var rollTotal = ResolutionData.AttackRoll.Sum(d => d.Result);
        var locationText = ResolutionData.HitLocation != null ? $" Location: {ResolutionData.HitLocation.Value}" : "";
            
        return string.Format(template, 
            player.Name,
            attacker.Name, 
            weapon.Name,
            targetPlayer.Name,
            target.Name,
            ResolutionData.ToHitNumber,
            rollTotal,
            locationText);
    }
}
