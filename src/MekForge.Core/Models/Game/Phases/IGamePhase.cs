using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.Phases;

/// <summary>
/// Interface for all game phases
/// </summary>
public interface IGamePhase
{
    /// <summary>
    /// The name/type of the phase
    /// </summary>
    PhaseNames Name { get; }
    
    /// <summary>
    /// Called when entering the phase
    /// </summary>
    void Enter();
    
    /// <summary>
    /// Handles a command in the context of this phase
    /// </summary>
    /// <param name="command">The command to handle</param>
    void HandleCommand(IGameCommand command);
}
