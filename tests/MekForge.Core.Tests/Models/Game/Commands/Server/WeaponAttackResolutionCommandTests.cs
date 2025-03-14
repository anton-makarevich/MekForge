using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data.Community;
using Sanet.MekForge.Core.Utils;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Server;

public class WeaponAttackResolutionCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1;
    private readonly Unit _attacker;
    private readonly Unit _target;
    private readonly WeaponData _weaponData;

    public WeaponAttackResolutionCommandTests()
    {
        // Create players
        _player1 = new Player(Guid.NewGuid(), "Player 1");
        var player2 = new Player(Guid.NewGuid(), "Player 2");

        // Create units using MechFactory
        var mechFactory = new MechFactory(new ClassicBattletechRulesProvider());
        var attackerData = MechFactoryTests.CreateDummyMechData();
        attackerData.Id = Guid.NewGuid();
        var targetData = MechFactoryTests.CreateDummyMechData();
        targetData.Id = Guid.NewGuid();
        
        _attacker = mechFactory.Create(attackerData);
        _target = mechFactory.Create(targetData);
        
        // Add units to players
        _player1.AddUnit(_attacker);
        player2.AddUnit(_target);
        
        // Setup game to return players
        _game.Players.Returns(new List<IPlayer> { _player1, player2 });
        
        // Setup weapon data - using the Medium Laser in the right arm
        var weapon = _attacker.Parts.SelectMany(p => p.GetComponents<Weapon>()).First();
        _weaponData = new WeaponData
        {
            Name = weapon.Name, // Added Name property
            Location = weapon.MountedOn!.Location,
            Slots = weapon.MountedAtSlots  // This might need adjustment based on actual slot position
        };
        
        // Setup localization service
        _localizationService.GetString("Command_WeaponAttackResolution_Hit")
            .Returns("{0}'s {1} hits {3}'s {4} with {2} (Target: {5}, Roll: {6})");
        _localizationService.GetString("Command_WeaponAttackResolution_Miss")
            .Returns("{0}'s {1} misses {3}'s {4} with {2} (Target: {5}, Roll: {6})");
        _localizationService.GetString("Command_WeaponAttackResolution_TotalDamage")
            .Returns("Total Damage: {0}");
        _localizationService.GetString("Command_WeaponAttackResolution_MissilesHit")
            .Returns("Missiles Hit: {0}");
        _localizationService.GetString("Command_WeaponAttackResolution_ClusterRoll")
            .Returns("Cluster Roll: {0}");
        _localizationService.GetString("Command_WeaponAttackResolution_HitLocations")
            .Returns("Hit Locations:");
        _localizationService.GetString("Command_WeaponAttackResolution_HitLocation")
            .Returns("{0}: {1} damage (Roll: {2})");
    }

    private WeaponAttackResolutionCommand CreateHitCommand()
    {
        // Create a hit with single location
        var hitLocations = new List<HitLocationData>
        {
            new(PartLocation.CenterTorso, 5, [new(6)])
        };
        
        var hitLocationsData = new AttackHitLocationsData(
            hitLocations,
            5,
            new List<DiceResult>(),
            0);
        
        var resolutionData = new AttackResolutionData(
            8,
            [new(4), new(5)],
            true,
            hitLocationsData);
        
        return new WeaponAttackResolutionCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            AttackerId = _attacker.Id,
            TargetId = _target.Id,
            WeaponData = _weaponData,
            ResolutionData = resolutionData,
            Timestamp = DateTime.UtcNow
        };
    }
    
    private WeaponAttackResolutionCommand CreateMissCommand()
    {
        var resolutionData = new AttackResolutionData(
            8,
            [new(2), new(3)],
            false);
        
        return new WeaponAttackResolutionCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            AttackerId = _attacker.Id,
            TargetId = _target.Id,
            WeaponData = _weaponData,
            ResolutionData = resolutionData,
            Timestamp = DateTime.UtcNow
        };
    }
    
    private WeaponAttackResolutionCommand CreateClusterHitCommand()
    {
        // Create a hit with multiple locations and cluster weapon
        var hitLocations = new List<HitLocationData>
        {
            new(PartLocation.LeftArm, 2, [new(2), new(3)]),
            new(PartLocation.RightArm, 2, [new(2), new(1)]),
            new(PartLocation.CenterTorso, 6, [new(5), new(3)])
        };
        
        var hitLocationsData = new AttackHitLocationsData(
            hitLocations,
            10,
            [new(6), new(4)],
            5);
        
        var resolutionData = new AttackResolutionData(
            7,
            [new(4), new(4)],
            true,
            hitLocationsData);
        
        return new WeaponAttackResolutionCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            AttackerId = _attacker.Id,
            TargetId = _target.Id,
            WeaponData = _weaponData,
            ResolutionData = resolutionData,
            Timestamp = DateTime.UtcNow
        };
    }

    [Fact]
    public void Format_ShouldFormatHit_Correctly()
    {
        // Arrange
        var command = CreateHitCommand();

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldNotBeEmpty();
        result.ShouldContain("Player 1's Locust LCT-1V hits Player 2's Locust LCT-1V with machine Gun");
        result.ShouldContain("Target: 8, Roll: 9");
        result.ShouldContain("Total Damage: 5");
        result.ShouldContain("CenterTorso: 5 damage");
    }

    [Fact]
    public void Format_ShouldFormatMiss_Correctly()
    {
        // Arrange
        var command = CreateMissCommand();

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldNotBeEmpty();
        result.ShouldBe("Player 1's Locust LCT-1V misses Player 2's Locust LCT-1V with Machine Gun (Target: 8, Roll: 5)");
    }

    [Fact]
    public void Format_ShouldFormatClusterWeapon_Correctly()
    {
        // Arrange
        var command = CreateClusterHitCommand();

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldNotBeEmpty();
        result.ShouldContain("Player 1's Locust LCT-1V hits Player 2's Locust LCT-1V with Machine Gun");
        result.ShouldContain("Target: 7, Roll: 8");
        result.ShouldContain("Total Damage: 10");
        result.ShouldContain("Cluster Roll: 10");
        result.ShouldContain("Missiles Hit: 5");
        result.ShouldContain("Hit Locations:");
        result.ShouldContain("LeftArm: 2 damage");
        result.ShouldContain("RightArm: 2 damage");
        result.ShouldContain("CenterTorso: 6 damage");
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = CreateHitCommand() with { PlayerId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenAttackerNotFound()
    {
        // Arrange
        var command = CreateHitCommand() with { AttackerId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenTargetNotFound()
    {
        // Arrange
        var command = CreateHitCommand() with { TargetId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }
}
