using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record WeaponAttackDeclarationCommand : ClientCommand
{
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        var attacker = player?.Units.FirstOrDefault(u => u.Id == AttackerId);
        
        if (attacker == null) return string.Empty;
        
        var primaryTarget = WeaponTargets
            .FirstOrDefault(wt => wt.IsPrimaryTarget);
            
        if (primaryTarget == null) return string.Empty;
        
        var targetUnit = game.Players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == primaryTarget.TargetId);
            
        if (targetUnit == null) return string.Empty;
        
        var localizedTemplate = localizationService.GetString("Command_WeaponAttackDeclaration");
        return string.Format(localizedTemplate, 
            player?.Name, 
            attacker.Name,
            targetUnit.Name,
            WeaponTargets.Count); 
    }

    public required Guid AttackerId { get; init; }
    public required List<WeaponTargetData> WeaponTargets { get; init; }
}
