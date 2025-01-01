﻿using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record ChangeActivePlayerCommand : GameCommand
{
    public required Guid? PlayerId { get; init; }
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;
        var localizedTemplate = localizationService.GetString("Command_ChangeActivePlayer"); 
        
        return string.Format(localizedTemplate, player.Name);
    }
}