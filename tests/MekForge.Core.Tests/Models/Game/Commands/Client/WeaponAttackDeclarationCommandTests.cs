using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class WeaponAttackDeclarationCommandTests : GameCommandTestBase<WeaponAttackDeclarationCommand>
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");
    private readonly Player _player2 = new Player(Guid.NewGuid(), "Player 2");
    private readonly Unit _attacker;
    private readonly Unit _target;

    public WeaponAttackDeclarationCommandTests()
    {
        _game.Players.Returns([_player1, _player2]);
        
        // Create attacker unit
        var attackerData = MechFactoryTests.CreateDummyMechData();
        attackerData.Id=Guid.NewGuid();
        _attacker = new MechFactory(new ClassicBattletechRulesProvider()).Create(attackerData);
        _player1.AddUnit(_attacker);
        
        // Create target unit
        var targetData = MechFactoryTests.CreateDummyMechData();
        targetData.Id = Guid.NewGuid();
        _target = new MechFactory(new ClassicBattletechRulesProvider()).Create(targetData);
        _player2.AddUnit(_target);

        _localizationService.GetString("Command_WeaponAttackDeclaration_NoAttacks")
            .Returns("{0}'s {1} didn't declare any attacks");
        _localizationService.GetString("Command_WeaponAttackDeclaration_Header")
            .Returns("{0}'s {1} declared following attacks:");
        _localizationService.GetString("Command_WeaponAttackDeclaration_WeaponLine")
            .Returns("- {0} at {1}'s {2}");
    }

    protected override WeaponAttackDeclarationCommand CreateCommand()
    {
        return new WeaponAttackDeclarationCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            AttackerId = _attacker.Id,
            WeaponTargets = new List<WeaponTargetData>
            {
                new WeaponTargetData
                {
                    Weapon = new WeaponData
                    {
                        Name = "Medium Laser",
                        Location = PartLocation.RightArm,
                        Slots = new[] { 1, 2 }
                    },
                    TargetId = _target.Id,
                    IsPrimaryTarget = true
                }
            }
        };
    }

    protected override void AssertCommandSpecificProperties(WeaponAttackDeclarationCommand original, WeaponAttackDeclarationCommand? cloned)
    {
        base.AssertCommandSpecificProperties(original, cloned);
        cloned!.PlayerId.ShouldBe(original.PlayerId);
        cloned.AttackerId.ShouldBe(original.AttackerId);
        cloned.WeaponTargets.Count.ShouldBe(original.WeaponTargets.Count);
        
        for (int i = 0; i < original.WeaponTargets.Count; i++)
        {
            var originalTarget = original.WeaponTargets[i];
            var clonedTarget = cloned.WeaponTargets[i];
            
            clonedTarget.TargetId.ShouldBe(originalTarget.TargetId);
            clonedTarget.IsPrimaryTarget.ShouldBe(originalTarget.IsPrimaryTarget);
            clonedTarget.Weapon.Name.ShouldBe(originalTarget.Weapon.Name);
            clonedTarget.Weapon.Location.ShouldBe(originalTarget.Weapon.Location);
            clonedTarget.Weapon.Slots.Length.ShouldBe(originalTarget.Weapon.Slots.Length);
            
            for (int j = 0; j < originalTarget.Weapon.Slots.Length; j++)
            {
                clonedTarget.Weapon.Slots[j].ShouldBe(originalTarget.Weapon.Slots[j]);
            }
        }
    }

    [Fact]
    public void Format_ReturnsEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = CreateCommand() with { PlayerId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ReturnsEmpty_WhenAttackerNotFound()
    {
        // Arrange
        var command = CreateCommand() with { AttackerId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Format_ReturnsNoAttacksMessage_WhenNoWeaponTargets()
    {
        // Arrange
        var command = CreateCommand() with { WeaponTargets = new List<WeaponTargetData>() };
        _attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Top));

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_NoAttacks");
        result.ShouldBe($"{_player1.Name}'s {_attacker.Name} didn't declare any attacks");
    }

    [Fact]
    public void Format_SkipsInvalidTargets_WhenTargetNotFound()
    {
        // Arrange
        var command = CreateCommand();
        command.WeaponTargets.Add(new WeaponTargetData
        {
            Weapon = new WeaponData
            {
                Name = "Large Laser",
                Location = PartLocation.LeftArm,
                Slots = [1, 2, 3]
            },
            TargetId = Guid.NewGuid(), // Invalid target ID
            IsPrimaryTarget = false
        });
        
        _attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Top));
        _target.Deploy(new HexPosition(new HexCoordinates(2, 2), HexDirection.Top));

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_Header");
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_WeaponLine");
        
        var expectedResult = $"{_player1.Name}'s {_attacker.Name} declared following attacks:" + Environment.NewLine +
                             $"- Medium Laser at {_player2.Name}'s {_target.Name}";
        
        result.ShouldBe(expectedResult);
    }
    
    [Fact]
    public void Format_ReturnsNoAttacksMessage_WhenTargetsNotFound()
    {
        // Arrange
        var command = CreateCommand();
        command.WeaponTargets.Clear();
        
        _attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Top));
        _target.Deploy(new HexPosition(new HexCoordinates(2, 2), HexDirection.Top));

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_NoAttacks");

        var expectedResult = $"{_player1.Name}'s {_attacker.Name} didn't declare any attacks";
        
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void Format_ReturnsAttackDeclarationMessage_WhenAllDataIsValid()
    {
        // Arrange
        var command = CreateCommand();
        _attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Top));
        _target.Deploy(new HexPosition(new HexCoordinates(2, 2), HexDirection.Top));

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_Header");
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_WeaponLine");
        
        var expectedResult = $"{_player1.Name}'s {_attacker.Name} declared following attacks:" +Environment.NewLine +
                             $"- Medium Laser at {_player2.Name}'s {_target.Name}";
        
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void Format_ReturnsMultipleWeaponLines_WhenMultipleWeaponsTarget()
    {
        // Arrange
        var command = CreateCommand();
        // Add a second weapon targeting the same unit
        command.WeaponTargets.Add(new WeaponTargetData
        {
            Weapon = new WeaponData
            {
                Name = "Large Laser",
                Location = PartLocation.LeftArm,
                Slots = new[] { 1, 2, 3 }
            },
            TargetId = _target.Id,
            IsPrimaryTarget = false
        });
        
        _attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Top));
        _target.Deploy(new HexPosition(new HexCoordinates(2, 2), HexDirection.Top));

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_Header");
        _localizationService.Received(1).GetString("Command_WeaponAttackDeclaration_WeaponLine");
        
        var expectedResult = $"{_player1.Name}'s {_attacker.Name} declared following attacks:" + Environment.NewLine +
                             $"- Medium Laser at {_player2.Name}'s {_target.Name}" + Environment.NewLine +
                             $"- Large Laser at {_player2.Name}'s {_target.Name}";
        
        result.ShouldBe(expectedResult);
    }
}
