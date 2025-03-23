using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.UiStates;

public interface IUiState
{
    string ActionLabel { get; }
    bool IsActionRequired { get; }
    bool CanExecutePlayerAction => false;
    void HandleUnitSelection(Unit? unit);
    void HandleHexSelection(Hex hex);
    void HandleFacingSelection(HexDirection direction);
    IEnumerable<StateAction> GetAvailableActions() => new List<StateAction>();
    
    /// <summary>
    /// Executes the primary player action for the current state
    /// </summary>
    void ExecutePlayerAction() { }
}