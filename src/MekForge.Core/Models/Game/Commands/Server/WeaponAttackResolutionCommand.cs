using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record WeaponAttackResolutionCommand : GameCommand
{
    public required Guid PlayerId { get; init; }
    public required Guid AttackerId { get; init; }
    public required WeaponData WeaponData { get; init; }
    public required Guid TargetId { get; init; }
    public required AttackResolutionData ResolutionData { get; init; }
    
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        var attacker = player?.Units.FirstOrDefault(u => u.Id == AttackerId);
        var weapon = attacker?.GetMountedComponentAtLocation<Weapon>(WeaponData.Location,WeaponData.Slots);
        var target = game.Players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == TargetId);
        var targetPlayer = target?.Owner;
        
        if (player == null || attacker == null || weapon == null || target == null || targetPlayer == null)
        {
            return string.Empty;
        }

        var template = ResolutionData.IsHit ? 
            localizationService.GetString("Command_WeaponAttackResolution_Hit") :
            localizationService.GetString("Command_WeaponAttackResolution_Miss");

        var rollTotal = ResolutionData.AttackRoll.Sum(d => d.Result);
        var locationText = ResolutionData.HitLocation != null ? $" Location: {ResolutionData.HitLocation.Result}" : "";
            
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
