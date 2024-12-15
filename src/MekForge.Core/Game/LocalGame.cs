using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Game;

public class LocalGame : IGame
{
    private readonly BattleState _battleState;
    private readonly IRulesProvider _rulesProvider;
    private readonly ICommandPublisher _commandPublisher;
    private readonly List<IPlayer> _players = new();
    private readonly MechFactory _mechFactory;
    public IPlayer LocalPlayer { get; }
    
    public LocalGame(BattleState battleState, 
        IRulesProvider rulesProvider, ICommandPublisher commandPublisher, IPlayer localPlayer)
    {
        _battleState = battleState;
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
        _mechFactory = new MechFactory(rulesProvider); //TODO: DI
        LocalPlayer = localPlayer;
        
        _commandPublisher.Subscribe(HandleCommand);
    }
    
    public IReadOnlyList<IPlayer> Players => _players.AsReadOnly();
    
    private void HandleCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinCmd:
                AddPlayer(joinCmd);
                break;
            // Handle other commands
        }
    }
    
    private void AddPlayer(JoinGameCommand joinGameCommand)
    {
        var player = new Player(joinGameCommand.PlayerId, joinGameCommand.PlayerName);
        foreach (var unit in joinGameCommand.Units.Select(unitData => _mechFactory.Create(unitData)))
        {
            player.AddUnit(unit);
        }
        _players.Add(player);
    }

    public Task JoinGameWithUnits(List<UnitData> units)
    {
        var joinCommand = new JoinGameCommand
        {
            PlayerId = LocalPlayer.Id,
            PlayerName = LocalPlayer.Name,
            Units = units
        };
        _commandPublisher.PublishCommand(joinCommand);
        return Task.CompletedTask;
    }
}