using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public abstract class GamePhase
{
    protected readonly ServerGame Game;

    protected GamePhase(ServerGame game)
    {
        Game = game;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    
    public abstract void HandleCommand(IGameCommand command);
    public abstract PhaseNames Name { get; }
}
