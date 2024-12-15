using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;

namespace Sanet.MekForge.Core.Game;

public class LocalGame : IGame
{
    private readonly BattleState _battleState;
    private readonly ICommandPublisher _commandPublisher;
    private readonly List<IPlayer> _players = new();
    public IPlayer LocalPlayer { get; }
    
    public LocalGame(BattleState battleState, ICommandPublisher commandPublisher, IPlayer localPlayer)
    {
        _battleState = battleState;
        _commandPublisher = commandPublisher;
        LocalPlayer = localPlayer;
        
        _commandPublisher.Subscribe(HandleCommand);
    }
    
    public IReadOnlyList<IPlayer> Players => _players.AsReadOnly();
    
    private void HandleCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinCmd:
                var player = new Player(joinCmd.PlayerId, joinCmd.PlayerName);
                _players.Add(player);
                break;
            // Handle other commands
        }
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