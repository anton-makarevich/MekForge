using Sanet.MekForge.Core.Models.Game.Phases;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record ChangePhaseCommand : GameCommand
{
    public PhaseNames Phase { get; init; }
}