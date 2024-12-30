using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.States;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class DeploymentPhase : GamePhase
{
    private Queue<IPlayer> _deploymentOrderQueue;

    public DeploymentPhase(ServerGame game) : base(game)
    {
        _deploymentOrderQueue = new Queue<IPlayer>();
    }

    public override void Enter()
    {
        RandomizeDeploymentOrder();
        SetNextDeployingPlayer();
    }

    public override void HandleCommand(GameCommand command)
    {
        if (command is DeployUnitCommand deployCommand)
        {
            Game.OnDeployUnit(deployCommand);
            HandleDeploymentProgress();
        }
    }

    private void HandleDeploymentProgress()
    {
        if (Game.ActivePlayer != null && !Game.ActivePlayer.Units.All(unit => unit.IsDeployed)) 
            return;
            
        if (_deploymentOrderQueue.Count > 0)
        {
            SetNextDeployingPlayer();
        }
        else if (AllUnitsDeployed())
        {
            Game.TransitionToPhase(new InitiativePhase(Game));
        }
    }

    private bool AllUnitsDeployed()
    {
        return Game.Players
            .Where(p => p.Status == PlayerStatus.Playing)
            .All(p => p.Units.All(u => u.IsDeployed));
    }

    private void RandomizeDeploymentOrder()
    {
        var players = Game.Players.Where(p => p.Status == PlayerStatus.Playing).ToList();
        var randomizedPlayers = players.OrderBy(p => Guid.NewGuid()).ToList();
        _deploymentOrderQueue = new Queue<IPlayer>(randomizedPlayers);
    }

    private void SetNextDeployingPlayer()
    {
        Game.SetActivePlayer(_deploymentOrderQueue.Dequeue());
    }

    public override PhaseNames Name => PhaseNames.Deployment;
}
