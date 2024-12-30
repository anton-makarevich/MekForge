using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.States;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public class ServerGame : BaseGame
{
    private GamePhase _currentPhase;
    private readonly IDiceRoller _diceRoller;
    private List<IPlayer> _initiativeOrder = new();

    public ServerGame(
        BattleMap battleMap, 
        IRulesProvider rulesProvider, 
        ICommandPublisher commandPublisher,
        IDiceRoller diceRoller)
        : base(battleMap, rulesProvider, commandPublisher)
    {
        _diceRoller = diceRoller;
        _currentPhase = new StartPhase(this);
    }

    public IDiceRoller DiceRoller => _diceRoller;

    public IReadOnlyList<IPlayer> InitiativeOrder => _initiativeOrder;

    public void SetInitiativeOrder(IReadOnlyList<IPlayer> order)
    {
        _initiativeOrder = order.ToList();
    }

    public void TransitionToPhase(GamePhase newPhase)
    {
        _currentPhase?.Exit();
        _currentPhase = newPhase;
        SetPhase(newPhase.Name);
        _currentPhase.Enter();
    }

    public override void HandleCommand(GameCommand command)
    {
        if (command is not ClientCommand) return;
        if (!ShouldHandleCommand(command)) return;
        if (!ValidateCommand(command)) return;

        _currentPhase.HandleCommand(command);
        
        // Broadcast the command to all clients
        command.GameOriginId = GameId;
        CommandPublisher.PublishCommand(command);
    }

    public void SetActivePlayer(IPlayer? player)
    {
        ActivePlayer = player;
        if (player != null)
        {
            CommandPublisher.PublishCommand(new ChangeActivePlayerCommand
            {
                GameOriginId = GameId,
                PlayerId = player.Id
            });
        }
    }

    public void SetPhase(PhaseNames phase)
    {
        TurnPhase = phase;
        CommandPublisher.PublishCommand(new ChangePhaseCommand
        {
            GameOriginId = GameId,
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