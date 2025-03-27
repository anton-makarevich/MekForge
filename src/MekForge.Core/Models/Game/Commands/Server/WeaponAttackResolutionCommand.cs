using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Services.Localization;
using System.Text;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record struct WeaponAttackResolutionCommand : IGameCommand
{
    public required Guid PlayerId { get; init; }
    public required Guid AttackerId { get; init; }
    public required WeaponData WeaponData { get; init; }
    public required Guid TargetId { get; init; }
    public required AttackResolutionData ResolutionData { get; init; }

    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; set; }

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

        var rollTotal = ResolutionData.AttackRoll.Sum(d => d.Result);
        var stringBuilder = new StringBuilder();

        if (ResolutionData.IsHit)
        {
            var hitTemplate = localizationService.GetString("Command_WeaponAttackResolution_Hit");
            stringBuilder.AppendLine(string.Format(hitTemplate,
                player.Name,
                attacker.Name,
                weapon.Name,
                targetPlayer.Name,
                target.Name,
                ResolutionData.ToHitNumber,
                rollTotal));

            // Add attack direction if available
            if (ResolutionData.AttackDirection.HasValue)
            {
                var directionKey = $"AttackDirection_{ResolutionData.AttackDirection.Value}";
                var directionString = localizationService.GetString(directionKey);
                
                stringBuilder.AppendLine(string.Format(
                    localizationService.GetString("Command_WeaponAttackResolution_Direction"),
                    directionString));
            }

            // Add damage information if hit
            if (ResolutionData.HitLocationsData == null) return stringBuilder.ToString().TrimEnd();
            // Add total damage
            stringBuilder.AppendLine(string.Format(
                localizationService.GetString("Command_WeaponAttackResolution_TotalDamage"),
                ResolutionData.HitLocationsData.TotalDamage));

            // Add missiles hit information for cluster weapons
            if (ResolutionData.HitLocationsData.ClusterRoll.Count > 1)
            {
                // Add cluster roll information
                if (ResolutionData.HitLocationsData.ClusterRoll.Count > 0)
                {
                    var clusterRollTotal = ResolutionData.HitLocationsData.ClusterRoll.Sum(d => d.Result);
                    stringBuilder.AppendLine(string.Format(
                        localizationService.GetString("Command_WeaponAttackResolution_ClusterRoll"),
                        clusterRollTotal));
                }

                stringBuilder.AppendLine(string.Format(
                    localizationService.GetString("Command_WeaponAttackResolution_MissilesHit"),
                    ResolutionData.HitLocationsData.MissilesHit));
            }

            // Add hit locations with damage
            if (ResolutionData.HitLocationsData.HitLocations.Count <= 0) return stringBuilder.ToString().TrimEnd();

            stringBuilder.AppendLine(localizationService.GetString("Command_WeaponAttackResolution_HitLocations"));

            foreach (var hitLocation in ResolutionData.HitLocationsData.HitLocations)
            {
                var locationRollTotal = hitLocation.LocationRoll.Sum(d => d.Result);
                stringBuilder.AppendLine(string.Format(
                    localizationService.GetString("Command_WeaponAttackResolution_HitLocation"),
                    hitLocation.Location,
                    hitLocation.Damage,
                    locationRollTotal));
            }
        }
        else
        {
            // Miss case
            var missTemplate = localizationService.GetString("Command_WeaponAttackResolution_Miss");
            stringBuilder.AppendLine(string.Format(missTemplate, 
                player.Name,
                attacker.Name, 
                weapon.Name,
                targetPlayer.Name,
                target.Name,
                ResolutionData.ToHitNumber,
                rollTotal));
        }
        
        return stringBuilder.ToString().TrimEnd();
    }
}
