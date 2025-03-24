using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.UiStates;

public class StartState : IUiState
{
    private readonly BattleMapViewModel _viewModel;
    private readonly ILocalizationService _localizationService;

    public StartState(BattleMapViewModel viewModel)
    {
        _viewModel = viewModel;
        _localizationService = viewModel.LocalizationService;
    }

    public string ActionLabel => _localizationService.GetString("StartPhase_ActionLabel");

    public bool IsActionRequired => IsLocalPlayerActive;
    
    public bool CanExecutePlayerAction => true;

    public string PlayerActionLabel => _localizationService.GetString("StartPhase_PlayerActionLabel");

    private bool IsLocalPlayerActive => _viewModel.Game is ClientGame { ActivePlayer: not null } clientGame && 
                                        clientGame.LocalPlayers.Any(p => p.Id == clientGame.ActivePlayer.Id);

    public void HandleUnitSelection(Unit? unit)
    {
        // Not applicable in StartState as there are no units yet
    }

    public void HandleHexSelection(Hex hex)
    {
        // Not applicable in StartState
    }

    public void HandleFacingSelection(HexDirection direction)
    {
        // Not applicable in StartState
    }

    /// <summary>
    /// Sets the active local player as ready to play
    /// </summary>
    public void ExecutePlayerAction()
    {
        if (_viewModel.Game is not ClientGame clientGame || clientGame.ActivePlayer == null) return;
        
        // Only set the active player as ready if they are a local player
        if (clientGame.LocalPlayers.All(p => p.Id != clientGame.ActivePlayer.Id)) return;
        var readyCommand = new UpdatePlayerStatusCommand
        {
            GameOriginId = clientGame.Id,
            PlayerId = clientGame.ActivePlayer.Id,
            PlayerStatus = PlayerStatus.Playing,
        };

        clientGame.SetPlayerReady(readyCommand);
    }
}
