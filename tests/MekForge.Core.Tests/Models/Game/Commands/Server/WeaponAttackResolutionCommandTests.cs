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

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Server;

public class WeaponAttackResolutionCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Guid _playerId = Guid.NewGuid();
    private readonly Guid _attackerId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();
    private readonly Guid _targetPlayerId = Guid.NewGuid();
    private readonly WeaponData _weaponData;
    
    private readonly IPlayer _player;
    private readonly IPlayer _targetPlayer;
    private readonly Unit _attacker;
    private readonly Unit _target;
    private readonly Weapon _weapon;

    public WeaponAttackResolutionCommandTests()
    {
        // Setup weapon data
        _weaponData = new WeaponData
        {
            Name = "Test Weapon",
            Location = PartLocation.RightArm,
            Slots = [1]
        };
        
        // Setup mock players and units
        _player = Substitute.For<IPlayer>();
        _player.Id.Returns(_playerId);
        _player.Name.Returns("Player 1");
        
        _targetPlayer = Substitute.For<IPlayer>();
        _targetPlayer.Id.Returns(_targetPlayerId);
        _targetPlayer.Name.Returns("Player 2");
        
        _attacker = Substitute.For<Unit>();
        _attacker.Id.Returns(_attackerId);
        _attacker.Name.Returns("Attacker Mech");
        _attacker.Owner.Returns(_player);
        
        _target = Substitute.For<Unit>();
        _target.Id.Returns(_targetId);
        _target.Name.Returns("Target Mech");
        _target.Owner.Returns(_targetPlayer);
        
        _weapon = Substitute.For<Weapon>();
        _weapon.Name.Returns("Test Weapon");
        
        // Setup attacker to return the weapon
        _attacker.GetMountedComponentAtLocation<Weapon>(
            Arg.Is<PartLocation>(l => l == _weaponData.Location),
            Arg.Is<int[]>(s => s.SequenceEqual(_weaponData.Slots)))
            .Returns(_weapon);
        
        // Setup game to return players and units
        _player.Units.Returns(new List<Unit> { _attacker });
        _targetPlayer.Units.Returns(new List<Unit> { _target });
        _game.Players.Returns(new List<IPlayer> { _player, _targetPlayer });
        
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
            new List<DiceResult> { new(4), new(5) },
            true,
            hitLocationsData);
        
        return new WeaponAttackResolutionCommand
        {
            GameOriginId = _gameId,
            PlayerId = _playerId,
            AttackerId = _attackerId,
            TargetId = _targetId,
            WeaponData = _weaponData,
            ResolutionData = resolutionData,
            Timestamp = DateTime.UtcNow
        };
    }
    
    private WeaponAttackResolutionCommand CreateMissCommand()
    {
        var resolutionData = new AttackResolutionData(
            8,
            new List<DiceResult> { new(2), new(3) },
            false);
        
        return new WeaponAttackResolutionCommand
        {
            GameOriginId = _gameId,
            PlayerId = _playerId,
            AttackerId = _attackerId,
            TargetId = _targetId,
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
            new(PartLocation.LeftArm, 2, new List<DiceResult> { new(5) }),
            new(PartLocation.RightArm, 2, new List<DiceResult> { new(3) }),
            new(PartLocation.CenterTorso, 6, new List<DiceResult> { new(8) })
        };
        
        var hitLocationsData = new AttackHitLocationsData(
            hitLocations,
            10,
            new List<DiceResult> { new(6), new(4) },
            5);
        
        var resolutionData = new AttackResolutionData(
            7,
            new List<DiceResult> { new(4), new(4) },
            true,
            hitLocationsData);
        
        return new WeaponAttackResolutionCommand
        {
            GameOriginId = _gameId,
            PlayerId = _playerId,
            AttackerId = _attackerId,
            TargetId = _targetId,
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
        result.ShouldContain("Player 1's Attacker Mech hits Player 2's Target Mech with Test Weapon");
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
        result.ShouldContain("Player 1's Attacker Mech misses Player 2's Target Mech with Test Weapon");
        result.ShouldContain("Target: 8, Roll: 5");
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
        result.ShouldContain("Player 1's Attacker Mech hits Player 2's Target Mech with Test Weapon");
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
        var command = CreateHitCommand();
        _game.Players.Returns(new List<IPlayer>());

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenAttackerNotFound()
    {
        // Arrange
        var command = CreateHitCommand();
        _player.Units.Returns(new List<Unit>());

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenTargetNotFound()
    {
        // Arrange
        var command = CreateHitCommand();
        _targetPlayer.Units.Returns(new List<Unit>());

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenWeaponNotFound()
    {
        // Arrange
        var command = CreateHitCommand();
        _attacker.GetMountedComponentAtLocation<Weapon>(
            Arg.Any<PartLocation>(),
            Arg.Any<int[ ]>())
            .Returns((Weapon)null);

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }
}
