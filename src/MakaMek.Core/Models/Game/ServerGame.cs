using Sanet.MakaMek.Core.Models.Game.Combat;
using Sanet.MakaMek.Core.Models.Game.Commands;
using Sanet.MakaMek.Core.Models.Game.Commands.Client;
using Sanet.MakaMek.Core.Models.Game.Commands.Server;
using Sanet.MakaMek.Core.Models.Game.Dice;
using Sanet.MakaMek.Core.Models.Game.Phases;
using Sanet.MakaMek.Core.Models.Game.Players;
using Sanet.MakaMek.Core.Models.Map;
using Sanet.MakaMek.Core.Services.Transport;
using Sanet.MakaMek.Core.Utils.TechRules;

namespace Sanet.MakaMek.Core.Models.Game;

public class ServerGame : BaseGame, IDisposable
{
    private IGamePhase _currentPhase;
    private List<IPlayer> _initiativeOrder = [];
    private bool _isGameOver;
    private bool _isDisposed;

    public bool IsAutoRoll { get; set; } = true;
    private IPhaseManager PhaseManager { get; }
    public IDiceRoller DiceRoller { get; }
    public IReadOnlyList<IPlayer> InitiativeOrder => _initiativeOrder;

    public ServerGame(
        IRulesProvider rulesProvider, 
        ICommandPublisher commandPublisher,
        IDiceRoller diceRoller,
        IToHitCalculator toHitCalculator,
        IPhaseManager? phaseManager = null)
        : base(rulesProvider, commandPublisher, toHitCalculator) 
    {
        DiceRoller = diceRoller;
        PhaseManager = phaseManager ?? new BattleTechPhaseManager();
        _currentPhase = new StartPhase(this); // Starts in StartPhase
    }

    public override void SetBattleMap(BattleMap map)
    {
        if (TurnPhase!= PhaseNames.Start) return; // Prevent changing map mid-game
        BattleMap = map;
        ((StartPhase)_currentPhase).TryTransitionToNextPhase();
    }
    
    public void SetInitiativeOrder(IReadOnlyList<IPlayer> order)
    {
        _initiativeOrder = order.ToList();
    }

    private void TransitionToPhase(IGamePhase newPhase)
    {
        if (_currentPhase is GamePhase currentGamePhase)
        {
            currentGamePhase.Exit();
        }
        _currentPhase = newPhase;
        SetPhase(newPhase.Name);
        _currentPhase.Enter();
    }

    public void TransitionToNextPhase(PhaseNames currentPhase)
    {
        var nextPhase = PhaseManager.GetNextPhase(currentPhase, this);
        TransitionToPhase(nextPhase);
    }

    public override void HandleCommand(IGameCommand command)
    {
        if (command is not IClientCommand) return;
        if (!ShouldHandleCommand(command)) return;
        if (!ValidateCommand(command)) return;

        _currentPhase.HandleCommand(command);
    }

    public void SetActivePlayer(IPlayer? player, int unitsToMove)
    {
        ActivePlayer = player;
        UnitsToPlayCurrentStep = unitsToMove;
        if (player != null)
        {
            CommandPublisher.PublishCommand(new ChangeActivePlayerCommand
            {
                GameOriginId = Id,
                PlayerId = player.Id,
                UnitsToPlay = unitsToMove
            });
        }
    }

    public void SetPhase(PhaseNames phase)
    {
        TurnPhase = phase;
        CommandPublisher.PublishCommand(new ChangePhaseCommand
        {
            GameOriginId = Id,
            Phase = phase
        });
    }

    public void IncrementTurn()
    {
        Turn++;
        _initiativeOrder.Clear(); // Clear initiative order at the start of new turn

        // Send turn increment command to all clients
        CommandPublisher.PublishCommand(new TurnIncrementedCommand
        {
            GameOriginId = Id,
            TurnNumber = Turn
        });
    }

    public async Task Start()
    {
        // The game loop is driven by commands and phase transitions
        // This method mainly keeps the server alive until disposed
        while (!_isDisposed && !_isGameOver)
        {
            await Task.Delay(100); // Keep the task alive but idle
        }
    }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _isGameOver = true; // Ensure the loop in Start() exits
        // Add any specific cleanup for ServerGame if needed (e.g., unsubscribe from events)
        GC.SuppressFinalize(this);
    }
}