using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.UiStates;

public interface IUiState
{
    string ActionLabel { get; }
    bool IsActionRequired { get; }
    void HandleUnitSelection(Unit? unit);
    void HandleHexSelection(Hex hex);
    void HandleFacingSelection(HexDirection direction);
    IEnumerable<StateAction> GetAvailableActions() => new List<StateAction>();
}