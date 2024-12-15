using Sanet.MekForge.Core.Game;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;

public class ServerGame : IGame
{
    private readonly BattleState _battleState;
    private readonly ICommandPublisher _commandPublisher;
    private readonly List<IPlayer> _players = [];
    
    public ServerGame(BattleState battleState, ICommandPublisher commandPublisher)
    {
        _battleState = battleState;
        _commandPublisher = commandPublisher;
    }
    
    public void HandleCommand(GameCommand command)
    {
        if (!ValidateCommand(command)) return;
        ExecuteCommand(command);
        _commandPublisher.PublishCommand(command); // Broadcast to all clients
    }
    
    private bool ValidateCommand(GameCommand command)
    {
        return command switch
        {
            JoinGameCommand joinCmd => ValidateJoinCommand(joinCmd),
            DeployUnitCommand deployCmd => ValidateDeployCommand(deployCmd),
            _ => false
        };
    }

    private bool ValidateJoinCommand(JoinGameCommand joinCmd)
    {
        return true;
    }

    private bool ValidateDeployCommand(DeployUnitCommand cmd)
    {
        //var unit = _battleState.GetUnit(cmd.UnitId);
        return true; //unit != null && !unit.Position.HasValue;
    }
    
    private void ExecuteCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinCmd:
                AddPlayer(joinCmd);
                break;
            case MoveUnitCommand moveCmd:
                break;
        }
    }

    private void AddPlayer(JoinGameCommand joinGameCommand)
    {
        var player = new Player(joinGameCommand.PlayerId, joinGameCommand.PlayerName);
        _players.Add(player);
    }
    
    public IReadOnlyList<IPlayer> Players => _players;

    public async Task? Start()
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
