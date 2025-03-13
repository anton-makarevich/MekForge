using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class WeaponAttackResolutionPhase(ServerGame game) : GamePhase(game)
{
    private int _currentPlayerIndex;
    private int _currentUnitIndex;
    private int _currentWeaponIndex;
    
    // List of players in initiative order for attack resolution
    private List<IPlayer> _playersInOrder = [];
    
    // Units with weapons that have targets, organized by player
    private readonly Dictionary<Guid, List<Unit>> _unitsWithTargets = new();

    public override void Enter()
    {
        base.Enter();
        
        // Initialize attack resolution process
        _playersInOrder = Game.InitiativeOrder.ToList();
        _currentPlayerIndex = 0;
        _currentUnitIndex = 0;
        _currentWeaponIndex = 0;
        
        // Prepare the dictionary of units with targets for each player
        PrepareUnitsWithTargets();
        
        // Start resolving attacks
        ResolveNextAttack();
    }
    
    private void PrepareUnitsWithTargets()
    {
        _unitsWithTargets.Clear();
        
        foreach (var player in _playersInOrder)
        {
            var unitsWithWeaponTargets = player.Units
                .Where(unit => unit.Parts.SelectMany(p=>p.GetComponents<Weapon>()).Any(weapon => weapon.Target != null))
                .ToList();
            
            if (unitsWithWeaponTargets.Count > 0)
            {
                _unitsWithTargets[player.Id] = unitsWithWeaponTargets;
            }
        }
    }

    private void ResolveNextAttack()
    {
        // Check if we've processed all players
        if (_currentPlayerIndex >= _playersInOrder.Count)
        {
            Game.TransitionToPhase(GetNextPhase());
            return;
        }

        var currentPlayer = _playersInOrder[_currentPlayerIndex];

        // Skip players with no units that have targets
        if (!_unitsWithTargets.TryGetValue(currentPlayer.Id, out var unitsWithTargets) || unitsWithTargets.Count == 0)
        {
            MoveToNextPlayer();
            ResolveNextAttack();
            return;
        }

        // Check if we've processed all units for the current player
        if (_currentUnitIndex >= unitsWithTargets.Count)
        {
            MoveToNextPlayer();
            ResolveNextAttack();
            return;
        }

        var currentUnit = unitsWithTargets[_currentUnitIndex];
        var weaponsWithTargets = currentUnit.Parts.SelectMany(p => p.GetComponents<Weapon>())
            .Where(weapon => weapon.Target != null)
            .ToList();

        // Check if we've processed all weapons for the current unit
        if (_currentWeaponIndex >= weaponsWithTargets.Count)
        {
            MoveToNextUnit();
            ResolveNextAttack();
            return;
        }

        var currentWeapon = weaponsWithTargets[_currentWeaponIndex];
        var targetUnit = currentWeapon.Target;

        if (targetUnit != null)
        {
            var resolution = ResolveAttack(currentUnit, targetUnit, currentWeapon);
            PublishAttackResolution(currentPlayer, currentUnit, currentWeapon, targetUnit, resolution);
        }

        // Move to the next weapon
        _currentWeaponIndex++;

        // Continue resolving attacks
        ResolveNextAttack();
    }

    private AttackResolutionData ResolveAttack(Unit attacker, Unit target, Weapon weapon)
    {
        // Calculate to-hit number
        var toHitNumber = Game.ToHitCalculator.GetToHitNumber(
            attacker,
            target,
            weapon,
            Game.BattleMap);

        // Roll 2D6 for attack
        var attackRoll = Game.DiceRoller.Roll2D6();
        var totalRoll = attackRoll.Sum(d => d.Result);
        
        var isHit = totalRoll >= toHitNumber;
        
        // If hit, determine location and damage
        AttackHitLocationsData? hitLocationsData = null;
        
        if (isHit)
        {
            // Check if it's a cluster weapon
            if (weapon.WeaponSize > 1)
            {
                // It's a cluster weapon, handle multiple hits
                hitLocationsData = ResolveClusterWeaponHit(weapon, target, attackRoll);
            }
            else
            {
                // Standard weapon, single hit location
                var locationRoll = Game.DiceRoller.Roll2D6();
                var locationRollTotal = locationRoll.Sum(d => d.Result);
                
                // Determine attack direction (which firing arc the attack is coming from)
                var attackDirection = DetermineAttackDirection(attacker, target);
                
                // Get hit location based on roll and attack direction
                var hitLocation = Game.RulesProvider.GetHitLocation(locationRollTotal, attackDirection);
                
                // Create hit locations data with a single hit
                var hitLocationData = new HitLocationData(hitLocation, weapon.Damage, locationRoll);
                hitLocationsData = new AttackHitLocationsData(
                    new List<HitLocationData> { hitLocationData },
                    weapon.Damage,
                    0, // No cluster roll for standard weapons
                    1  // Single hit
                );
            }
        }

        return new AttackResolutionData(toHitNumber, attackRoll, isHit, hitLocationsData);
    }
    
    private AttackHitLocationsData ResolveClusterWeaponHit(Weapon weapon, Unit target, List<Models.Game.Dice.DiceResult> attackRoll)
    {
        // Roll for cluster hits
        var clusterRoll = Game.DiceRoller.Roll2D6();
        var clusterRollTotal = clusterRoll.Sum(d => d.Result);
        
        // Determine how many missiles hit using the cluster hit table
        var missilesHit = Game.RulesProvider.GetClusterHits(clusterRollTotal, weapon.WeaponSize);
        
        // Calculate damage per missile
        var damagePerMissile = weapon.Damage / weapon.WeaponSize;
        
        // Calculate how many complete clusters hit and if there's a partial cluster
        var completeClusterHits = missilesHit / weapon.ClusterSize;
        var remainingMissiles = missilesHit % weapon.ClusterSize;
        
        var hitLocations = new List<HitLocationData>();
        var totalDamage = 0;
        
        // For each complete cluster that hit
        for (var i = 0; i < completeClusterHits; i++)
        {
            // Roll for hit location
            var locationRoll = Game.DiceRoller.Roll2D6();
            var locationRollTotal = locationRoll.Sum(d => d.Result);
            
            // Determine attack direction
            var attackDirection = DetermineAttackDirection(null, target);
            
            // Get hit location
            var hitLocation = Game.RulesProvider.GetHitLocation(locationRollTotal, attackDirection);
            
            // Calculate damage for this cluster
            var clusterDamage = weapon.ClusterSize * damagePerMissile;
            totalDamage += clusterDamage;
            
            // Add to hit locations
            hitLocations.Add(new HitLocationData(hitLocation, clusterDamage, locationRoll));
        }
        
        // If there are remaining missiles (partial cluster)
        if (remainingMissiles > 0)
        {
            // Roll for hit location for the partial cluster
            var locationRoll = Game.DiceRoller.Roll2D6();
            var locationRollTotal = locationRoll.Sum(d => d.Result);
            
            // Determine attack direction
            var attackDirection = DetermineAttackDirection(null, target);
            
            // Get hit location
            var hitLocation = Game.RulesProvider.GetHitLocation(locationRollTotal, attackDirection);
            
            // Calculate damage for the partial cluster
            var partialClusterDamage = remainingMissiles * damagePerMissile;
            totalDamage += partialClusterDamage;
            
            // Add to hit locations
            hitLocations.Add(new HitLocationData(hitLocation, partialClusterDamage, locationRoll));
        }
        
        return new AttackHitLocationsData(hitLocations, totalDamage, clusterRollTotal, missilesHit);
    }
    
    private FiringArc DetermineAttackDirection(Unit? attacker, Unit target)
    {
        // Default to forward if no attacker is provided (used in cluster resolution)
        if (attacker == null || attacker.Position == null || target.Position == null)
            return FiringArc.Forward;
            
        // Check each firing arc to determine which one contains the attacker
        foreach (var arc in Enum.GetValues<FiringArc>())
        {
            if (target.Position.Coordinates.IsInFiringArc(attacker.Position.Coordinates, target.Position.Facing, arc))
            {
                return arc;
            }
        }
        
        // Default to forward if no arc is determined
        return FiringArc.Forward;
    }

    private void MoveToNextUnit()
    {
        _currentUnitIndex++;
        _currentWeaponIndex = 0;
    }
    
    private void MoveToNextPlayer()
    {
        _currentPlayerIndex++;
        _currentUnitIndex = 0;
        _currentWeaponIndex = 0;
    }
    
    private void PublishAttackResolution(IPlayer player, Unit attacker, Weapon weapon, Unit target, AttackResolutionData resolution)
    {
        // Create and publish a command to inform clients about the attack resolution
        var command = new WeaponAttackResolutionCommand
        {
            GameOriginId = Game.Id,
            PlayerId = player.Id,
            AttackerId = attacker.Id,
            WeaponData = new WeaponData
            {
                Location = weapon.MountedOn!.Location,
                Name = weapon.Name,
                Slots = weapon.MountedAtSlots
            },
            TargetId = target.Id,
            ResolutionData = resolution
        };
        
        Game.CommandPublisher.PublishCommand(command);
    }

    private GamePhase GetNextPhase() => new PhysicalAttackPhase(Game);

    public override void HandleCommand(IGameCommand command)
    {
        // This phase doesn't process incoming commands, but we need to implement this method
    }

    public override PhaseNames Name => PhaseNames.WeaponAttackResolution;
}
