using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.States;

public abstract class GameStateTestsBase
{
    protected readonly ServerGame Game;
    protected readonly ICommandPublisher CommandPublisher;

    protected GameStateTestsBase()
    {
        CommandPublisher = Substitute.For<ICommandPublisher>();
        IRulesProvider rulesProvider = new ClassicBattletechRulesProvider();
        var battleMap = BattleMap.GenerateMap(10, 10, new SingleTerrainGenerator(10,10, new ClearTerrain()));
        Game = new ServerGame(battleMap, rulesProvider, CommandPublisher);
    }

    protected void VerifyPhaseChange(Phase expectedPhase)
    {
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<ChangePhaseCommand>(cmd => 
                cmd.Phase == expectedPhase && 
                cmd.GameOriginId == Game.GameId));
    }

    protected void VerifyActivePlayerChange(Guid? expectedPlayerId)
    {
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<ChangeActivePlayerCommand>(cmd => 
                cmd.PlayerId == expectedPlayerId && 
                cmd.GameOriginId == Game.GameId));
    }

    protected JoinGameCommand CreateJoinCommand(Guid playerId, string playerName)
    {
        var mechData = MechFactoryTests.CreateDummyMechData();
        mechData.Id = Guid.NewGuid();
        return new JoinGameCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            PlayerName = playerName,
            Units = [mechData]
        };
    }

    protected UpdatePlayerStatusCommand CreateStatusCommand(Guid playerId, PlayerStatus status)
    {
        return new UpdatePlayerStatusCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            PlayerStatus = status
        };
    }

    protected DeployUnitCommand CreateDeployCommand(Guid playerId, Guid unitId, int q, int r, int direction)
    {
        return new DeployUnitCommand
        {
            GameOriginId = Game.GameId,
            PlayerId = playerId,
            UnitId = unitId,
            Position = new HexCoordinateData(q,r),
            Direction = direction
        };
    }
}
