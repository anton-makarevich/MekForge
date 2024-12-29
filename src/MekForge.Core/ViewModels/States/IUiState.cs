using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.ViewModels.States;

public interface IUiState
{
    void HandleUnitSelection(Unit? unit);
    void HandleHexSelection(Hex hex);
    string ActionLabel { get; }
    bool IsActionRequired { get; }
}

public class IdleState : IUiState
{
    public void HandleUnitSelection(Unit? unit) { }
    public void HandleHexSelection(Hex hex) { }
    public string ActionLabel => string.Empty;
    public bool IsActionRequired => false;
}
