using Sanet.MekForge.Core.Services.Localization;
using System.Text;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record struct HeatUpdatedCommand : IGameCommand
{
    public required Guid UnitId { get; init; }
    public required int HeatGenerated { get; init; }
    public required int HeatDissipated { get; init; }
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
        
        // Heat generation
        stringBuilder.AppendLine(string.Format(
            localizationService.GetString("Command_HeatUpdated_Generated") ?? "Heat generated for {0}: {1}",
            unit.Name,
            HeatGenerated));
            
        // Heat dissipation
        stringBuilder.AppendLine(string.Format(
            localizationService.GetString("Command_HeatUpdated_Dissipated") ?? "Heat dissipated: {0}",
            HeatDissipated));
            
        // Final heat
        stringBuilder.AppendLine(string.Format(
            localizationService.GetString("Command_HeatUpdated_Final") ?? "Final heat level: {0}",
            FinalHeat));
            
        return stringBuilder.ToString().TrimEnd();
    }
}
