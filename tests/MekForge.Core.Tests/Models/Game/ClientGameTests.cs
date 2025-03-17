using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Data.Map;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Tests.Data.Community;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class ClientGameTests
{
    private readonly ClientGame _clientGame;
    private readonly ICommandPublisher _commandPublisher;

    public ClientGameTests()
    {
        var battleState = BattleMap.GenerateMap(5, 5, new SingleTerrainGenerator(5,5, new ClearTerrain()));
        _commandPublisher = Substitute.For<ICommandPublisher>();
        var rulesProvider = new ClassicBattletechRulesProvider();
        _clientGame = new ClientGame(battleState,[], rulesProvider, _commandPublisher,
            Substitute.For<IToHitCalculator>());
    }

    [Fact]
    public void HandleCommand_ShouldAddPlayer_WhenJoinGameCommandIsReceived()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(joinCommand);

        // Assert
        _clientGame.Players.Count.ShouldBe(1);
        _clientGame.Players[0].Name.ShouldBe(joinCommand.PlayerName);
    }
    
    [Fact]
    public void HandleCommand_ShouldNotProcessOwnCommands_WhenGameOriginIdMatches()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units = [],
            GameOriginId = _clientGame.Id, // Set to this game's ID
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(command);

        // Assert
        // Verify that no players were added since the command was from this game instance
        _clientGame.Players.ShouldBeEmpty();
    }

    [Fact]
    public void JoinGameWithUnits_ShouldPublishJoinGameCommand_WhenCalled()
    {
        // Arrange
        var units = new List<UnitData>();
        var player = new Player(Guid.NewGuid(), "Player1");

        // Act
        _clientGame.JoinGameWithUnits(player, units);

        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<JoinGameCommand>(cmd =>
            cmd.PlayerId == player.Id &&
            cmd.PlayerName == player.Name &&
            cmd.Units.Count == units.Count));
    }
    
    [Fact]
    public void HandleCommand_ShouldSetPlayerStatus_WhenPlayerStatusCommandIsReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name, Units = [],
            Tint = "#FF0000"
        });

        var statusCommand = new UpdatePlayerStatusCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            PlayerStatus = PlayerStatus.Playing
        };

        // Act
        _clientGame.HandleCommand(statusCommand);

        // Assert
        var updatedPlayer = _clientGame.Players.FirstOrDefault(p => p.Id == playerId);
        updatedPlayer.ShouldNotBeNull();
        updatedPlayer.Status.ShouldBe(PlayerStatus.Playing);
    }
    
    [Fact]
    public void SetPlayerReady_ShouldNotPublishPlayerStatusCommand_WhenCalled_ButPlayerIsNotInGame()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");

        // Act
        _clientGame.SetPlayerReady(player);

        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<UpdatePlayerStatusCommand>());
    }
    
    [Fact]
    public void SetPlayerReady_ShouldPublishPlayerStatusCommand_WhenCalled_AndPlayerIsInGame()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [],
            Tint = "#FF0000"
        });

        // Act
        _clientGame.SetPlayerReady(player);

        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<UpdatePlayerStatusCommand>(cmd => 
            cmd.PlayerId == player.Id && 
            cmd.PlayerStatus == PlayerStatus.Playing &&
            cmd.GameOriginId == _clientGame.Id
        ));
    }

    [Fact]
    public void ChangePhase_ShouldProcessCommand()
    {
        // Arrange
        var command = new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.End
        };
        
        // Act
        _clientGame.HandleCommand(command);
        
        // Assert
        _clientGame.TurnPhase.ShouldBe(PhaseNames.End);
    }
    
    [Fact]
    public void ChangeActivePlayer_ShouldProcessCommand()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [],
            Tint = "#FF0000"
        });
        var actualPlayer = _clientGame.Players.FirstOrDefault(p => p.Id == player.Id);
        var command = new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 0
        };
        
        // Act
        _clientGame.HandleCommand(command);
        
        // Assert
        _clientGame.ActivePlayer.ShouldBe(actualPlayer);
        actualPlayer!.Name.ShouldBe(player.Name);
        actualPlayer.Id.ShouldBe(player.Id);
    }

    [Fact]
    public void HandleCommand_ShouldAddCommandToLog_WhenCommandIsValid()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(joinCommand);

        // Assert
        _clientGame.CommandLog.Count.ShouldBe(1);
        _clientGame.CommandLog[0].ShouldBeEquivalentTo(joinCommand);
    }

    [Fact]
    public void HandleCommand_ShouldNotAddCommandToLog_WhenGameOriginIdMatches()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units = [],
            GameOriginId = _clientGame.Id,
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(command);

        // Assert
        _clientGame.CommandLog.ShouldBeEmpty();
    }

    [Fact]
    public void Commands_ShouldEmitCommand_WhenHandleCommandIsCalled()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };
        var receivedCommands = new List<IGameCommand>();
        using var subscription = _clientGame.Commands.Subscribe(cmd => receivedCommands.Add(cmd));

        // Act
        _clientGame.HandleCommand(joinCommand);

        // Assert
        receivedCommands.Count.ShouldBe(1);
        receivedCommands.First().ShouldBeEquivalentTo(joinCommand);
    }

    [Fact]
    public void DeployUnit_ShouldPublishCommand_WhenActivePlayerExists()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id= Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });
        _clientGame.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });

        var deployCommand = new DeployUnitCommand
        {
            GameOriginId = _clientGame.Id,
            PlayerId = player.Id,
            Position = new  HexCoordinateData(1, 1), 
            Direction = 0,
            UnitId = unitData.Id.Value
        };

        // Act
        _clientGame.DeployUnit(deployCommand);

        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<DeployUnitCommand>(cmd =>
            cmd.PlayerId == player.Id &&
            cmd.Position == deployCommand.Position &&
            cmd.GameOriginId == _clientGame.Id));
    }

    [Fact]
    public void DeployUnit_ShouldNotPublishCommand_WhenNoActivePlayer()
    {
        // Arrange
        var deployCommand = new DeployUnitCommand
        {
            GameOriginId = _clientGame.Id,
            PlayerId = Guid.NewGuid(),
            Position = new HexCoordinateData(1,1),
            Direction = 0,
            UnitId = Guid.NewGuid()
        };
    
        // Act
        _clientGame.DeployUnit(deployCommand);
    
        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<DeployUnitCommand>());
    }
    
    [Fact]
    public void MoveUnit_ShouldPublishCommand_WhenActivePlayerExists()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id= Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });
        _clientGame.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });
    
        var moveCommand = new MoveUnitCommand
        {
            GameOriginId = _clientGame.Id,
            PlayerId = player.Id,
            MovementType = MovementType.Walk,
            UnitId = unitData.Id.Value,
            MovementPath = [new PathSegment(new HexPosition(1, 1, HexDirection.Top), new HexPosition(2, 2, HexDirection.Top), 1).ToData()]
        };
    
        // Act
        _clientGame.MoveUnit(moveCommand);
    
        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<MoveUnitCommand>(cmd =>
            cmd.PlayerId == player.Id &&
            cmd.MovementType == moveCommand.MovementType &&
            cmd.GameOriginId == _clientGame.Id));
    }
    
    [Fact]
    public void MoveUnit_ShouldNotPublishCommand_WhenNoActivePlayer()
    {
        // Arrange
        var moveCommand = new MoveUnitCommand
        {
            GameOriginId = _clientGame.Id,
            PlayerId = Guid.NewGuid(),
            MovementType = MovementType.Walk,
            UnitId = Guid.NewGuid(),
            MovementPath = [new PathSegment(new HexPosition(1, 1, HexDirection.Top), new HexPosition(2, 2, HexDirection.Top), 1).ToData()]
        };
    
        // Act
        _clientGame.MoveUnit(moveCommand);
    
        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<MoveUnitCommand>());
    }

    [Fact]
    public void HandleCommand_ShouldDeployUnit_WhenDeployUnitCommandIsReceived()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });

        var deployCommand = new DeployUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            Position = new HexCoordinateData(1, 1),
            Direction = 0,
            UnitId = unitData.Id.Value
        };

        // Act
        _clientGame.HandleCommand(deployCommand);

        // Assert
        var deployedUnit = _clientGame.Players.First().Units.First();
        deployedUnit.IsDeployed.ShouldBeTrue();
        deployedUnit.Position!.Coordinates.Q.ShouldBe(1);
        deployedUnit.Position.Coordinates.R.ShouldBe(1);
        deployedUnit.Position.Facing.ShouldBe(HexDirection.Top);
    }

    [Fact]
    public void HandleCommand_ShouldNotDeployUnit_WhenUnitIsAlreadyDeployed()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });

        var firstDeployCommand = new DeployUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            Position = new HexCoordinateData(1, 1),
            Direction = 0,
            UnitId = unitData.Id.Value
        };
        _clientGame.HandleCommand(firstDeployCommand);

        var initialPosition = _clientGame.Players.First().Units.First().Position;

        var secondDeployCommand = new DeployUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            Position = new HexCoordinateData(2, 2),
            Direction = 1,
            UnitId = unitData.Id.Value
        };

        // Act
        _clientGame.HandleCommand(secondDeployCommand);

        // Assert
        var unit = _clientGame.Players.First().Units.First();
        unit.Position.ShouldBe(initialPosition);
    }

    [Fact]
    public void HandleCommand_ShouldMoveUnit_WhenMoveUnitCommandIsReceived()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });

        // First deploy the unit
        var deployCommand = new DeployUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            Position = new HexCoordinateData(1, 1),
            Direction = 0,
            UnitId = unitData.Id.Value
        };
        _clientGame.HandleCommand(deployCommand);

        var moveCommand = new MoveUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            MovementType = MovementType.Walk,
            UnitId = unitData.Id.Value,
            MovementPath = [new PathSegment(new HexPosition(1, 1, HexDirection.Top), new HexPosition(2, 2, HexDirection.Top), 1).ToData()]
        };

        // Act
        _clientGame.HandleCommand(moveCommand);

        // Assert
        var movedUnit = _clientGame.Players[0].Units[0];
        movedUnit.Position!.Coordinates.Q.ShouldBe(2);
        movedUnit.Position.Coordinates.R.ShouldBe(2);
        movedUnit.Position.Facing.ShouldBe(HexDirection.Top);
    }

    [Fact]
    public void HandleCommand_ShouldNotMoveUnit_WhenUnitDoesNotExist()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [],
            Tint = "#FF0000"
        });

        var moveCommand = new MoveUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            MovementType = MovementType.Walk,
            UnitId = Guid.NewGuid(),
            MovementPath = [new PathSegment(new HexPosition(1, 1, HexDirection.Top), new HexPosition(2, 2, HexDirection.Top), 1).ToData()]
        };

        // Act & Assert
        Should.NotThrow(() => _clientGame.HandleCommand(moveCommand));
    }

    [Fact]
    public void ConfigureUnitWeapons_ShouldPublishCommand_WhenActivePlayerExists()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });

        _clientGame.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });

        var command = new WeaponConfigurationCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitId = unitData.Id.Value,
            Configuration = new WeaponConfiguration
            {
                Type = WeaponConfigurationType.TorsoRotation,
                Value = 1
            }
        };

        // Act
        _clientGame.ConfigureUnitWeapons(command);

        // Assert
        _commandPublisher.Received(1).PublishCommand(command);
    }

    [Fact]
    public void ConfigureUnitWeapons_ShouldNotPublishCommand_WhenNoActivePlayer()
    {
        // Arrange
        var command = new WeaponConfigurationCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            Configuration = new WeaponConfiguration
            {
                Type = WeaponConfigurationType.TorsoRotation,
                Value = 1
            }
        };

        // Act
        _clientGame.ConfigureUnitWeapons(command);

        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(command);
    }

    [Fact]
    public void HandleCommand_ShouldRotateTorso_WhenWeaponConfigurationCommandIsReceived()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });

        // First deploy the unit
        var deployCommand = new DeployUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            Position = new HexCoordinateData(1, 1),
            Direction = 0,
            UnitId = unitData.Id.Value
        };
        _clientGame.HandleCommand(deployCommand);

        var configCommand = new WeaponConfigurationCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitId = unitData.Id.Value,
            Configuration = new WeaponConfiguration
            {
                Type = WeaponConfigurationType.TorsoRotation,
                Value = (int)HexDirection.TopRight
            }
        };

        // Act
        _clientGame.HandleCommand(configCommand);

        // Assert
        var unit = _clientGame.Players[0].Units[0];
        (unit as Mech)!.TorsoDirection.ShouldBe(HexDirection.TopRight);
    }

    [Fact]
    public void DeclareWeaponAttack_ShouldPublishCommand_WhenActivePlayerExists()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name,
            Units = [unitData],
            Tint = "#FF0000"
        });

        _clientGame.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });

        var targetPlayer = new Player(Guid.NewGuid(), "Player2");
        var targetUnitData = MechFactoryTests.CreateDummyMechData();
        targetUnitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = targetPlayer.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = targetPlayer.Name,
            Units = [targetUnitData],
            Tint = "#00FF00"
        });

        var command = new WeaponAttackDeclarationCommand
        {
            GameOriginId = _clientGame.Id,
            PlayerId = player.Id,
            AttackerId = unitData.Id.Value,
            WeaponTargets =
            [
                new WeaponTargetData()
                {
                    Weapon = new WeaponData
                    {
                        Name = "Medium Laser",
                        Location = PartLocation.RightArm,
                        Slots = [1, 2]
                    },
                    TargetId = targetUnitData.Id.Value,
                    IsPrimaryTarget = true
                }
            ]
        };

        // Act
        _clientGame.DeclareWeaponAttack(command);

        // Assert
        _commandPublisher.Received(1).PublishCommand(command);
    }

    [Fact]
    public void DeclareWeaponAttack_ShouldNotPublishCommand_WhenNoActivePlayer()
    {
        // Arrange
        var command = new WeaponAttackDeclarationCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            AttackerId = Guid.NewGuid(),
            WeaponTargets =
            [
                new WeaponTargetData
                {
                    Weapon = new WeaponData
                    {
                        Name = "Medium Laser",
                        Location = PartLocation.RightArm,
                        Slots = [1, 2]
                    },
                    TargetId = Guid.NewGuid(),
                    IsPrimaryTarget = true
                }
            ]
        };

        // Act
        _clientGame.DeclareWeaponAttack(command);

        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<WeaponAttackDeclarationCommand>());
    }

    [Fact]
    public void HandleCommand_ShouldDeclareWeaponAttack_WhenWeaponAttackDeclarationCommandIsReceived()
    {
        // Arrange
        var attackerPlayer = new Player(Guid.NewGuid(), "Attacker");
        var attackerUnitData = MechFactoryTests.CreateDummyMechData();
        attackerUnitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = attackerPlayer.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = attackerPlayer.Name,
            Units = [attackerUnitData],
            Tint = "#FF0000"
        });

        // Deploy the attacker unit
        var deployCommand = new DeployUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = attackerPlayer.Id,
            Position = new HexCoordinateData(1, 1),
            Direction = 0,
            UnitId = attackerUnitData.Id.Value
        };
        _clientGame.HandleCommand(deployCommand);

        // Add a target player and unit
        var targetPlayer = new Player(Guid.NewGuid(), "Target");
        var targetUnitData = MechFactoryTests.CreateDummyMechData();
        targetUnitData.Id = Guid.NewGuid();
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = targetPlayer.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = targetPlayer.Name,
            Units = [targetUnitData],
            Tint = "#00FF00"
        });

        // Deploy the target unit
        var deployTargetCommand = new DeployUnitCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = targetPlayer.Id,
            Position = new HexCoordinateData(2, 2),
            Direction = 0,
            UnitId = targetUnitData.Id.Value
        };
        _clientGame.HandleCommand(deployTargetCommand);

        // Create weapon attack declaration command
        var weaponAttackCommand = new WeaponAttackDeclarationCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = attackerPlayer.Id,
            AttackerId = attackerUnitData.Id.Value,
            WeaponTargets =
            [
                new WeaponTargetData
                {
                    Weapon = new WeaponData
                    {
                        Name = "Medium Laser",
                        Location = PartLocation.RightArm,
                        Slots = [1, 2]
                    },
                    TargetId = targetUnitData.Id.Value,
                    IsPrimaryTarget = true
                }
            ]
        };

        // Act
        _clientGame.HandleCommand(weaponAttackCommand);

        // Assert
        var attackerUnit = _clientGame.Players.First(p => p.Id == attackerPlayer.Id).Units.First();
        
        // Verify that the unit has declared a weapon attack
        attackerUnit.HasDeclaredWeaponAttack.ShouldBeTrue();
    }

    [Fact]
    public void HandleCommand_ShouldApplyDamage_WhenWeaponAttackResolutionCommandIsReceived()
    {
        // Arrange
        // Add target player and unit
        var targetPlayerId = Guid.NewGuid();
        var targetUnitData = MechFactoryTests.CreateDummyMechData();
        targetUnitData.Id = Guid.NewGuid();
        var targetJoinCommand = new JoinGameCommand
        {
            PlayerId = targetPlayerId,
            PlayerName = "Target",
            GameOriginId = Guid.NewGuid(),
            Units = [targetUnitData],
            Tint = "#00FF00"
        };
        _clientGame.HandleCommand(targetJoinCommand);
        var targetPlayer = _clientGame.Players.First(p => p.Id == targetPlayerId);
        var targetMech = targetPlayer.Units.First() as Mech;
        targetMech!.Deploy(new HexPosition(new HexCoordinates(1, 2), HexDirection.Top));
        
        // Create hit locations data
        var hitLocations = new List<HitLocationData>
        {
            new(PartLocation.CenterTorso, 5, []),
            new(PartLocation.LeftArm, 3, [])
        };
        
        // Create the attack resolution command
        var attackResolutionCommand = new WeaponAttackResolutionCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            AttackerId = targetMech.Id,
            TargetId = targetMech.Id,
            WeaponData = new WeaponData
            {
                Name = "Test Weapon",
                Location = PartLocation.RightArm,
                Slots = [0, 1]
            },
            ResolutionData = new AttackResolutionData(
                10,
                [],
                true,
                null,
                new AttackHitLocationsData(hitLocations, 8, [], 0))
        };

        // Get initial armor values for verification
        var centerTorsoPart = targetMech.Parts.First(p => p.Location == PartLocation.CenterTorso);
        var leftArmPart = targetMech.Parts.First(p => p.Location == PartLocation.LeftArm);
        var initialCenterTorsoArmor = centerTorsoPart.CurrentArmor;
        var initialLeftArmArmor = leftArmPart.CurrentArmor;

        // Act
        _clientGame.HandleCommand(attackResolutionCommand);

        // Assert
        // Verify that armor was reduced by the damage amount
        centerTorsoPart.CurrentArmor.ShouldBe(initialCenterTorsoArmor - 5);
        leftArmPart.CurrentArmor.ShouldBe(initialLeftArmArmor - 3);
    }

    [Fact]
    public void HandleCommand_ShouldApplyHeat_WhenHeatUpdatedCommandIsReceived()
    {
        // Arrange
        // Add player and unit
        var playerId = Guid.NewGuid();
        var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id = Guid.NewGuid();
        var joinCommand = new JoinGameCommand
        {
            PlayerId = playerId,
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [unitData],
            Tint = "#FF0000"
        };
        _clientGame.HandleCommand(joinCommand);
        
        // Get the unit and check initial heat
        var unit = _clientGame.Players.First(p => p.Id == playerId).Units.First();
        var initialHeat = unit.CurrentHeat;
        
        // Create heat data
        var heatData = new HeatData
        {
            MovementHeatSources = 
            [
                new MovementHeatData
                {
                    MovementType = MovementType.Run,
                    MovementPointsSpent = 5,
                    HeatPoints = 12
                }
            ],
            WeaponHeatSources = 
            [
                new WeaponHeatData
                {
                    WeaponName = "Medium Laser",
                    HeatPoints = 13
                }
            ],
            DissipationData = new HeatDissipationData
            {
                HeatSinks = 10,
                EngineHeatSinks = 10,
                DissipationPoints =20
            }
        };
        
        // Create the heat update command
        var heatUpdateCommand = new HeatUpdatedCommand
        {
            GameOriginId = Guid.NewGuid(),
            UnitId = unitData.Id.Value,
            HeatData = heatData,
            PreviousHeat = initialHeat,
            Timestamp = DateTime.UtcNow
        };
        
        // Act
        _clientGame.HandleCommand(heatUpdateCommand);
        
        // Assert
        unit.CurrentHeat.ShouldBe(5); //0+25-20
    }
}