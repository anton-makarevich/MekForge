using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public class ServerGame : BaseGame
{
    private GamePhase _currentPhase;
    private List<IPlayer> _initiativeOrder = [];
    public bool IsAutoRoll { get; set; } = true;

    public ServerGame(
        BattleMap battleMap, 
        IRulesProvider rulesProvider, 
        ICommandPublisher commandPublisher,
        IDiceRoller diceRoller,
        IToHitCalculator toHitCalculator)
        : base(battleMap, rulesProvider, commandPublisher, toHitCalculator)
    {
        DiceRoller = diceRoller;
        _currentPhase = new StartPhase(this);
    }

    public IDiceRoller DiceRoller { get; }

    public IReadOnlyList<IPlayer> InitiativeOrder => _initiativeOrder;

    public void SetInitiativeOrder(IReadOnlyList<IPlayer> order)
    {
        _initiativeOrder = order.ToList();
    }

    public void TransitionToPhase(GamePhase newPhase)
    {
        _currentPhase.Exit();
        _currentPhase = newPhase;
        SetPhase(newPhase.Name);
        _currentPhase.Enter();
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
        UnitsToPlayCurrentStep= unitsToMove;
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
    }

    public async Task Start()
    {
        while (true)
        {
            await Task.Delay(16);
            if (_isGameOver)
                return;
        }
    }

    private bool _isGameOver = false;
}