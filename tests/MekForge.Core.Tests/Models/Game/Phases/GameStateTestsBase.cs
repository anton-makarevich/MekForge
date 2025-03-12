using NSubstitute;
using Sanet.MekForge.Core.Data.Map;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public abstract class GameStateTestsBase
{
    protected readonly ServerGame Game;
    protected readonly ICommandPublisher CommandPublisher;
    protected readonly IDiceRoller DiceRoller;

    protected GameStateTestsBase()
    {
        CommandPublisher = Substitute.For<ICommandPublisher>();
        DiceRoller = Substitute.For<IDiceRoller>();
        IRulesProvider rulesProvider = new ClassicBattletechRulesProvider();
        var battleMap = BattleMap.GenerateMap(10, 10, new SingleTerrainGenerator(10,10,
            new ClearTerrain()));
        Game = new ServerGame(battleMap, rulesProvider, CommandPublisher, DiceRoller,
            Substitute.For<IToHitCalculator>());
    }

    protected void VerifyPhaseChange(PhaseNames expectedPhaseNames)
    {
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<ChangePhaseCommand>(cmd => 
                cmd.Phase == expectedPhaseNames && 
                cmd.GameOriginId == Game.Id));
    }

    protected void VerifyActivePlayerChange(Guid? expectedPlayerId)
    {
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<ChangeActivePlayerCommand>(cmd => 
                cmd.PlayerId == expectedPlayerId && 
                cmd.GameOriginId == Game.Id));
    }

    protected JoinGameCommand CreateJoinCommand(Guid playerId, string playerName, int unitsCount=1)
    {
        List<UnitData> units = [];
        for (var i = 0; i < unitsCount ; i++)
        {
            var mechData = MechFactoryTests.CreateDummyMechData();
            mechData.Id = Guid.NewGuid();
            units.Add(mechData);
        }
        
        return new JoinGameCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            PlayerName = playerName,
            Units = units,
            Tint = "#FF0000"
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
            GameOriginId = Game.Id,
            PlayerId = playerId,
            UnitId = unitId,
            Position = new HexCoordinateData(q,r),
            Direction = direction
        };
    }
}
