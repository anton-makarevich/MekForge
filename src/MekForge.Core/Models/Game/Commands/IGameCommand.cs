using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public interface IGameCommand
{
    Guid GameOriginId { get; set; }
    DateTime Timestamp { get; init; } 
    string Format(ILocalizationService localizationService, IGame game); 
}