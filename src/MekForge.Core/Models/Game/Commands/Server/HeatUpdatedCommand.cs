using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Services.Localization;
using System.Text;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record struct HeatUpdatedCommand : IGameCommand
{
    public required Guid UnitId { get; init; }
    public required List<MovementHeatData> MovementHeatSources { get; init; }
    public required List<WeaponHeatData> WeaponHeatSources { get; init; }
    public required HeatDissipationData DissipationData { get; init; }
    public required int PreviousHeat { get; init; }
    public required int FinalHeat { get; init; }
    
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; }

    public string Format(ILocalizationService localizationService, IGame game)
    {
        var command = this;
        var unit = game.Players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == command.UnitId);
            
        if (unit == null)
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder();
        
        // Unit name and previous heat
        stringBuilder.AppendLine(string.Format(
            localizationService.GetString("Command_HeatUpdated_Header"),
            unit.Name,
            PreviousHeat));
            
        // Heat sources
        stringBuilder.AppendLine(localizationService.GetString("Command_HeatUpdated_Sources"));
        
        // Movement heat sources
        foreach (var source in MovementHeatSources)
        {
            stringBuilder.AppendLine(string.Format(
                localizationService.GetString("Command_HeatUpdated_MovementHeat"),
                source.MovementType,
                source.MovementPointsSpent,
                source.HeatPoints));
        }
        
        // Weapon heat sources
        foreach (var source in WeaponHeatSources)
        {
            stringBuilder.AppendLine(string.Format(
                localizationService.GetString("Command_HeatUpdated_WeaponHeat"),
                source.WeaponName,
                source.HeatPoints));
        }
        
        // Total heat generated
        var totalGenerated = 
            MovementHeatSources.Sum(s => s.HeatPoints) + 
            WeaponHeatSources.Sum(s => s.HeatPoints);
            
        stringBuilder.AppendLine(string.Format(
            localizationService.GetString("Command_HeatUpdated_TotalGenerated"),
            totalGenerated));
            
        // Heat dissipation
        stringBuilder.AppendLine(string.Format(
            localizationService.GetString("Command_HeatUpdated_Dissipation"),
            DissipationData.HeatSinks,
            DissipationData.EngineHeatSinks,
            DissipationData.DissipationPoints));
            
        // Final heat
        stringBuilder.AppendLine(string.Format(
            localizationService.GetString("Command_HeatUpdated_Final"),
            FinalHeat));
            
        return stringBuilder.ToString().TrimEnd();
    }
}
