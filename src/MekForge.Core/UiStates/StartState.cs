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

    public bool IsActionRequired => IsLocalPlayer;

    private bool IsLocalPlayer => _viewModel.Game is ClientGame clientGame &&
                                 clientGame.LocalPlayers.Any();

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
    /// Sets all local players as ready to play
    /// </summary>
    public void ExecutePlayerAction()
    {
        if (_viewModel.Game is not ClientGame clientGame) return;

        foreach (var player in clientGame.LocalPlayers)
        {
            var readyCommand = new UpdatePlayerStatusCommand
            {
                GameOriginId = clientGame.Id,
                PlayerId = player.Id,
                PlayerStatus = PlayerStatus.Playing,
                Timestamp = DateTime.UtcNow
            };

            clientGame.SetPlayerReady(readyCommand);
        }
    }
}
