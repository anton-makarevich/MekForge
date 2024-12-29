using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.States;

public abstract class GameState
{
    protected readonly ServerGame Game;

    protected GameState(ServerGame game)
    {
        Game = game;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    
    public abstract void HandleCommand(GameCommand command);
    public abstract string Name { get; }
}
