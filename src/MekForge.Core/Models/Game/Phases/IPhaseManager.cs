namespace Sanet.MekForge.Core.Models.Game.Phases;

/// <summary>
/// Interface for managing game phase transitions
/// </summary>
public interface IPhaseManager
{
    /// <summary>
    /// Gets the next phase based on the current phase
    /// </summary>
    /// <param name="currentPhase">The current phase name</param>
    /// <param name="game">The game instance</param>
    /// <returns>The next phase instance</returns>
    IGamePhase GetNextPhase(PhaseNames currentPhase, ServerGame game);
}
