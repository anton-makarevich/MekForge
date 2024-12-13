// using Sanet.MekForge.Core.Models;
// using Sanet.MekForge.Core.Models.Units;
// using Sanet.MekForge.Core.Models.Units.Mechs;
//
// public class BattleStateTests
// {
//     private BattleMap _map;
//     private BattleState _battleState;
//
//     public BattleStateTests()
//     {
//         _map = new BattleMap(10, 10);
//         _battleState = new BattleState(_map);
//     }
//
//     [Fact]
//     public void TryDeployUnit_ShouldDeployUnit_WhenValid()
//     {
//         var unit = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
//         var coordinates = new HexCoordinates(1, 1);
//
//         var result = _battleState.TryDeployUnit(unit, coordinates);
//
//         Assert.True(result);
//         Assert.True(unit.IsDeployed);
//     }
//
//     [Fact]
//     public void TryMoveUnit_ShouldMoveUnit_WhenValid()
//     {
//         var unit = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
//         var coordinates = new HexCoordinates(1, 1);
//         _battleState.TryDeployUnit(unit, coordinates);
//
//         var newCoordinates = new HexCoordinates(2, 2);
//         var result = _battleState.TryMoveUnit(unit, newCoordinates);
//
//         Assert.True(result);
//         Assert.Equal(newCoordinates, unit.Position);
//     }
//
//     [Fact]
//     public void HasLineOfSight_ShouldReturnFalse_WhenBlockedByUnit()
//     {
//         var attacker = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
//         var target = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
//         var coordinates1 = new HexCoordinates(1, 1);
//         var coordinates2 = new HexCoordinates(2, 2);
//         _battleState.TryDeployUnit(attacker, coordinates1);
//         _battleState.TryDeployUnit(target, coordinates2);
//
//         var result = _battleState.HasLineOfSight(attacker, target);
//
//         Assert.False(result);
//     }
//
//     [Fact]
//     public void FindPath_ShouldReturnPath_WhenValid()
//     {
//         var unit = new Mech("Chassis", "Model", 50, 4, new List<UnitPart>());
//         var startCoordinates = new HexCoordinates(1, 1);
//         var targetCoordinates = new HexCoordinates(2, 2);
//         _battleState.TryDeployUnit(unit, startCoordinates);
//
//         var path = _battleState.FindPath(unit, targetCoordinates);
//
//         Assert.NotNull(path);
//         Assert.Contains(targetCoordinates, path);
//     }
// }
