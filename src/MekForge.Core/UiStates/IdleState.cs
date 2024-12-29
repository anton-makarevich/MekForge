using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.UiStates;

public class IdleState : IUiState
{
    public void HandleUnitSelection(Unit? unit) { }
    public void HandleHexSelection(Hex hex) { }
    public string ActionLabel => "Wait";
    public bool IsActionRequired => false;
}