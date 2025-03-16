using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Units;
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
        var heatGenerated = 0;
        
        // 1. Calculate movement heat
        if (unit.MovementTypeUsed.HasValue)
        {
            heatGenerated += Game.RulesProvider.GetMovementHeatPoints(
                unit.MovementTypeUsed.Value, 
                unit.MovementPointsSpent);
        }
        
        // 1.2 Calculate weapon heat for weapons with targets
        var weaponsWithTargets = unit.Parts
            .SelectMany(p => p.GetComponents<Weapon>())
            .Where(weapon => weapon.Target != null);
            
        foreach (var weapon in weaponsWithTargets)
        {
            heatGenerated += weapon.Heat;
        }
        
        // 2. Get heat dissipation
        var heatDissipation = unit.HeatDissipation;
        
        // Apply heat to the unit
        unit.ApplyHeat(heatGenerated);
        
        // Dissipate heat
        unit.DissipateHeat();
        var finalHeat = unit.CurrentHeat;
        
        // 3. Publish heat updated command
        PublishHeatUpdatedCommand(unit, heatGenerated, heatDissipation, finalHeat);
    }
    
    private void PublishHeatUpdatedCommand(Unit unit, int heatGenerated, int heatDissipated, int finalHeat)
    {
        var command = new HeatUpdatedCommand
        {
            UnitId = unit.Id,
            HeatGenerated = heatGenerated,
            HeatDissipated = heatDissipated,
            FinalHeat = finalHeat,
            Timestamp = DateTime.UtcNow
        };
        
        Game.CommandPublisher.PublishCommand(command);
    }
}
