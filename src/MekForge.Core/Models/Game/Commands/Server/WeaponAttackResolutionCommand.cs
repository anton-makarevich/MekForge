using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record WeaponAttackResolutionCommand : GameCommand
{
    public required Guid PlayerId { get; init; }
    public required Guid AttackerId { get; init; }
    public required WeaponData WeaponData { get; init; }
    public required Guid TargetId { get; init; }
    public required int ToHitNumber { get; init; }
    
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
        
        var template = localizationService.GetString("Command_WeaponAttackResolution");
        return string.Format(template, 
            player.Name, 
            attacker.Name, 
            weapon.Name, 
            targetPlayer.Name, 
            target.Name, 
            ToHitNumber);
    }
}
