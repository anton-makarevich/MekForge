using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data.Community;
using Sanet.MekForge.Core.Utils;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Server;

public class HeatUpdatedCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Unit _unit;

    public HeatUpdatedCommandTests()
    {
        var player =
            // Create player
            new Player(Guid.NewGuid(), "Player 1");

        // Create unit using MechFactory
        var mechFactory = new MechFactory(new ClassicBattletechRulesProvider());
        var mechData = MechFactoryTests.CreateDummyMechData();
        mechData.Id = Guid.NewGuid();
        
        _unit = mechFactory.Create(mechData);
        
        // Add unit to player
        player.AddUnit(_unit);
        
        // Setup game to return players
        _game.Players.Returns(new List<IPlayer> { player });
        
        // Setup localization service
        _localizationService.GetString("Command_HeatUpdated_Header")
            .Returns("Heat update for {0} (Previous: {1})");
        _localizationService.GetString("Command_HeatUpdated_Sources")
            .Returns("Heat sources:");
        _localizationService.GetString("Command_HeatUpdated_MovementHeat")
            .Returns("  + {0} movement ({1} MP): {2} heat");
        _localizationService.GetString("Command_HeatUpdated_WeaponHeat")
            .Returns("  + Firing {0}: {1} heat");
        _localizationService.GetString("Command_HeatUpdated_TotalGenerated")
            .Returns("Total heat generated: {0}");
        _localizationService.GetString("Command_HeatUpdated_Dissipation")
            .Returns("  - Heat dissipation from {0} heat sinks and {1} engine heat sinks: -{2} heat");
        _localizationService.GetString("Command_HeatUpdated_Final")
            .Returns("Final heat level: {0}");
    }

    [Fact]
    public void Format_WithNoHeatSources_ReturnsExpectedString()
    {
        // Arrange
        var command = new HeatUpdatedCommand
        {
            UnitId = _unit.Id,
            UnitName = _unit.Name,
            MovementHeatSources = [],
            WeaponHeatSources = [],
            DissipationData = new HeatDissipationData
            {
                HeatSinks = 10,
                EngineHeatSinks = 10,
                DissipationPoints = 20
            },
            PreviousHeat = 0,
            FinalHeat = 0,
            GameOriginId = _gameId,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldContain($"Heat update for {_unit.Name} (Previous: 0)");
        result.ShouldContain("Heat sources:");
        result.ShouldContain("Total heat generated: 0");
        result.ShouldContain("Heat dissipation from 10 heat sinks and 10 engine heat sinks: -20 heat");
        result.ShouldContain("Final heat level: 0");
    }

    [Fact]
    public void Format_WithMovementHeat_ReturnsExpectedString()
    {
        // Arrange
        var movementHeatSources = new List<MovementHeatData>
        {
            new() { MovementType = MovementType.Run, MovementPointsSpent = 5, HeatPoints = 2 }
        };

        var command = new HeatUpdatedCommand
        {
            UnitId = _unit.Id,
            UnitName = _unit.Name,
            MovementHeatSources = movementHeatSources,
            WeaponHeatSources = [],
            DissipationData = new HeatDissipationData
            {
                HeatSinks = 10,
                EngineHeatSinks = 10,
                DissipationPoints = 20
            },
            PreviousHeat = 0,
            FinalHeat = 0,
            GameOriginId = _gameId,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldContain($"Heat update for {_unit.Name} (Previous: 0)");
        result.ShouldContain("Heat sources:");
        result.ShouldContain("Run movement (5 MP): 2 heat");
        result.ShouldContain("Total heat generated: 2");
        result.ShouldContain("Heat dissipation from 10 heat sinks and 10 engine heat sinks: -20 heat");
        result.ShouldContain("Final heat level: 0");
    }

    [Fact]
    public void Format_WithWeaponHeat_ReturnsExpectedString()
    {
        // Arrange
        var weaponHeatSources = new List<WeaponHeatData>
        {
            new() { WeaponName = "Medium Laser", HeatPoints = 3 },
            new() { WeaponName = "Large Laser", HeatPoints = 8 }
        };

        var command = new HeatUpdatedCommand
        {
            UnitId = _unit.Id,
            UnitName = _unit.Name,
            MovementHeatSources = [],
            WeaponHeatSources = weaponHeatSources,
            DissipationData = new HeatDissipationData
            {
                HeatSinks = 10,
                EngineHeatSinks = 10,
                DissipationPoints = 20
            },
            PreviousHeat = 0,
            FinalHeat = 0,
            GameOriginId = _gameId,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldContain($"Heat update for {_unit.Name} (Previous: 0)");
        result.ShouldContain("Heat sources:");
        result.ShouldContain("Firing Medium Laser: 3 heat");
        result.ShouldContain("Firing Large Laser: 8 heat");
        result.ShouldContain("Total heat generated: 11");
        result.ShouldContain("Heat dissipation from 10 heat sinks and 10 engine heat sinks: -20 heat");
        result.ShouldContain("Final heat level: 0");
    }

    [Fact]
    public void Format_WithCombinedHeatSources_ReturnsExpectedString()
    {
        // Arrange
        var movementHeatSources = new List<MovementHeatData>
        {
            new() { MovementType = MovementType.Jump, MovementPointsSpent = 3, HeatPoints = 3 }
        };

        var weaponHeatSources = new List<WeaponHeatData>
        {
            new() { WeaponName = "Medium Laser", HeatPoints = 3 },
            new() { WeaponName = "PPC", HeatPoints = 10 }
        };

        var command = new HeatUpdatedCommand
        {
            UnitId = _unit.Id,
            UnitName = _unit.Name,
            MovementHeatSources = movementHeatSources,
            WeaponHeatSources = weaponHeatSources,
            DissipationData = new HeatDissipationData
            {
                HeatSinks = 10,
                EngineHeatSinks = 10,
                DissipationPoints = 20
            },
            PreviousHeat = 5,
            FinalHeat = 1,
            GameOriginId = _gameId,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldContain($"Heat update for {_unit.Name} (Previous: 5)");
        result.ShouldContain("Heat sources:");
        result.ShouldContain("Jump movement (3 MP): 3 heat");
        result.ShouldContain("Firing Medium Laser: 3 heat");
        result.ShouldContain("Firing PPC: 10 heat");
        result.ShouldContain("Total heat generated: 16");
        result.ShouldContain("Heat dissipation from 10 heat sinks and 10 engine heat sinks: -20 heat");
        result.ShouldContain("Final heat level: 1");
    }

    [Fact]
    public void Format_WithUnitNotFound_ReturnsEmptyString()
    {
        // Arrange
        var command = new HeatUpdatedCommand
        {
            UnitId = Guid.NewGuid(), // Different unit ID that doesn't exist in the game
            UnitName = "Unknown Unit",
            MovementHeatSources = [],
            WeaponHeatSources = [],
            DissipationData = new HeatDissipationData
            {
                HeatSinks = 10,
                EngineHeatSinks = 10,
                DissipationPoints = 20
            },
            PreviousHeat = 0,
            FinalHeat = 0,
            GameOriginId = _gameId,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }
}
