using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class HeatPhase(ServerGame game) : GamePhase(game)
{
    private int _currentPlayerIndex;
    private int _currentUnitIndex;
    
    // List of players in initiative order for heat resolution
    private List<IPlayer> _playersInOrder = [];

    public override void Enter()
    {
        base.Enter();
        
        // Initialize heat resolution process
        _playersInOrder = Game.InitiativeOrder.ToList();
        _currentPlayerIndex = 0;
        _currentUnitIndex = 0;
        
        // Start processing heat for all units
        ProcessNextUnitHeat();
    }

    public override void HandleCommand(IGameCommand command)
    {
        // No commands to handle in this phase
    }

    public override PhaseNames Name => PhaseNames.Heat;
    
    private GamePhase GetNextPhase() => new EndPhase(Game);

    private void ProcessNextUnitHeat()
    {
        // Check if we've processed all players
        if (_currentPlayerIndex >= _playersInOrder.Count)
        {
            Game.TransitionToPhase(GetNextPhase());
            return;
        }

        var currentPlayer = _playersInOrder[_currentPlayerIndex];
        var units = currentPlayer.Units;

        // Check if we've processed all units for the current player
        if (_currentUnitIndex >= units.Count)
        {
            MoveToNextPlayer();
            ProcessNextUnitHeat();
            return;
        }

        var currentUnit = units[_currentUnitIndex];
        
        // Calculate and apply heat for the current unit
        CalculateAndApplyHeat(currentUnit);
        
        // Move to the next unit
        _currentUnitIndex++;
        
        // Continue processing heat for the next unit
        ProcessNextUnitHeat();
    }
    
    private void MoveToNextPlayer()
    {
        _currentPlayerIndex++;
        _currentUnitIndex = 0;
    }
    
    private void CalculateAndApplyHeat(Unit unit)
    {
        var movementHeatSources = new List<MovementHeatData>();
        var weaponHeatSources = new List<WeaponHeatData>();
        
        // 1. Calculate movement heat
        if (unit.MovementTypeUsed.HasValue)
        {
            var movementHeatPoints = Game.RulesProvider.GetMovementHeatPoints(
                unit.MovementTypeUsed.Value, 
                unit.MovementPointsSpent);
                
            if (movementHeatPoints > 0)
            {
                movementHeatSources.Add(new MovementHeatData
                {
                    MovementType = unit.MovementTypeUsed.Value,
                    MovementPointsSpent = unit.MovementPointsSpent,
                    HeatPoints = movementHeatPoints
                });
            }
        }
        
        // 1.2 Calculate weapon heat for weapons with targets
        var weaponsWithTargets = unit.Parts
            .SelectMany(p => p.GetComponents<Weapon>())
            .Where(weapon => weapon.Target != null);
            
        foreach (var weapon in weaponsWithTargets)
        {
            if (weapon.Heat > 0)
            {
                weaponHeatSources.Add(new WeaponHeatData
                {
                    WeaponName = weapon.Name,
                    HeatPoints = weapon.Heat
                });
            }
        }
        
        // 2. Get heat dissipation
        var heatSinks = unit.GetAllComponents<HeatSink>().Count();
        var engineHeatSinks = 10; // Always 10 engine heat sinks
        var heatDissipation = unit.HeatDissipation;
        var dissipationData = new HeatDissipationData
        {
            HeatSinks = heatSinks,
            EngineHeatSinks = engineHeatSinks,
            DissipationPoints = heatDissipation
        };
        
        // Store previous heat before applying new heat
        var previousHeat = unit.CurrentHeat;
        
        // Apply heat to the unit
        var totalHeatGenerated = 
            movementHeatSources.Sum(source => source.HeatPoints) + 
            weaponHeatSources.Sum(source => source.HeatPoints);
            
        unit.ApplyHeat(totalHeatGenerated);
        
        // Dissipate heat
        unit.DissipateHeat();
        var finalHeat = unit.CurrentHeat;
        
        // 3. Publish heat updated command
        PublishHeatUpdatedCommand(
            unit, 
            movementHeatSources, 
            weaponHeatSources, 
            dissipationData, 
            previousHeat, 
            finalHeat);
    }
    
    private void PublishHeatUpdatedCommand(
        Unit unit, 
        List<MovementHeatData> movementHeatSources,
        List<WeaponHeatData> weaponHeatSources,
        HeatDissipationData dissipationData,
        int previousHeat, 
        int finalHeat)
    {
        var command = new HeatUpdatedCommand
        {
            UnitId = unit.Id,
            UnitName = unit.Name,
            MovementHeatSources = movementHeatSources,
            WeaponHeatSources = weaponHeatSources,
            DissipationData = dissipationData,
            PreviousHeat = previousHeat,
            FinalHeat = finalHeat,
            Timestamp = DateTime.UtcNow,
            GameOriginId = Game.Id
        };
        
        Game.CommandPublisher.PublishCommand(command);
    }
}
