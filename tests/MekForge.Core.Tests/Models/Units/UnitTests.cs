using Shouldly;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Tests.Models.Units;

public class UnitTests
{
    private class TestComponent(string name, int size = 1) : Component(name, [], size);
    
    private class TestWeapon : Weapon
    {
        public TestWeapon(string name, int[] slots) : base(
            name, 5, 3, 0, 3, 6, 9, WeaponType.Energy, 10, slots.Length)
        {
            Mount(slots, null!); // Will be properly mounted later
        }
    }
    
    private class TestUnitPart(string name, PartLocation location, int maxArmor, int maxStructure, int slots)
        : UnitPart(name, location, maxArmor, maxStructure, slots);
    
    private class TestUnit(
        string chassis,
        string model,
        int tonnage,
        int walkMp,
        IEnumerable<UnitPart> parts,
        Guid? id = null)
        : Unit(chassis, model, tonnage, walkMp, parts, id)
    {
        public override int CalculateBattleValue() => 0;

        public override bool CanMoveBackward(MovementType type) => true;

        protected override PartLocation? GetTransferLocation(PartLocation location) => null;
    }
    
    private TestUnit CreateTestUnit(Guid? id = null)
    {
        var parts = new List<UnitPart>
        {
            new TestUnitPart("Center Torso", PartLocation.CenterTorso, 10, 5, 10),
            new TestUnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 10),
            new TestUnitPart("Right Arm", PartLocation.RightArm, 10, 5, 10)
        };
        
        return new TestUnit("Test", "Unit", 20, 4, parts, id);
    }
    
    private void MountWeaponOnUnit(TestUnit unit, TestWeapon weapon, PartLocation location, int[] slots)
    {
        var part = unit.Parts.First(p => p.Location == location);
        part.TryAddComponent(weapon,slots);
    }
    
    [Fact]
    public void GetComponentsAtLocation_ShouldReturnComponentsAtSpecifiedLocation()
    {
        // Arrange
        var leftArmPart = new TestUnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 10);
        var rightArmPart = new TestUnitPart("Right Arm", PartLocation.RightArm, 10, 5, 10);
        var testUnit = new TestUnit("Test", "Unit", 20, 4, [leftArmPart, rightArmPart]);
        
        var leftArmComponent1 = new TestComponent("Left Arm Component 1", 2);
        var leftArmComponent2 = new TestComponent("Left Arm Component 2", 2);
        var rightArmComponent = new TestComponent("Right Arm Component", 2);
        
        leftArmPart.TryAddComponent(leftArmComponent1);
        leftArmPart.TryAddComponent(leftArmComponent2);
        rightArmPart.TryAddComponent(rightArmComponent);
        
        // Act
        var leftArmComponents = testUnit.GetComponentsAtLocation(PartLocation.LeftArm).ToList();
        var rightArmComponents = testUnit.GetComponentsAtLocation(PartLocation.RightArm).ToList();
        var headComponents = testUnit.GetComponentsAtLocation(PartLocation.Head).ToList();
        
        // Assert
        leftArmComponents.Count.ShouldBe(2);
        leftArmComponents.ShouldContain(leftArmComponent1);
        leftArmComponents.ShouldContain(leftArmComponent2);
        
        rightArmComponents.Count.ShouldBe(1);
        rightArmComponents.ShouldContain(rightArmComponent);
        
        headComponents.ShouldBeEmpty();
    }
    
    [Fact]
    public void GetComponentsAtLocation_Generic_ShouldReturnComponentsOfSpecificType()
    {
        // Arrange
        var leftArmPart = new TestUnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 10);
        var testUnit = new TestUnit("Test", "Unit", 20, 4, [leftArmPart]);
        
        var component1 = new TestComponent("Component 1", 2);
        var component2 = new TestDerivedComponent("Component 2", 2);
        
        leftArmPart.TryAddComponent(component1);
        leftArmPart.TryAddComponent(component2);
        
        // Act
        var allComponents = testUnit.GetComponentsAtLocation(PartLocation.LeftArm).ToList();
        var derivedComponents = testUnit.GetComponentsAtLocation<TestDerivedComponent>(PartLocation.LeftArm).ToList();
        
        // Assert
        allComponents.Count.ShouldBe(2);
        derivedComponents.Count.ShouldBe(1);
        derivedComponents.ShouldContain(component2);
    }
    
    [Fact]
    public void FindComponentPart_ShouldReturnCorrectPart()
    {
        // Arrange
        var leftArmPart = new TestUnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 10);
        var rightArmPart = new TestUnitPart("Right Arm", PartLocation.RightArm, 10, 5, 10);
        var testUnit = new TestUnit("Test", "Unit", 20, 4, [leftArmPart, rightArmPart]);
        
        var leftArmComponent = new TestComponent("Left Arm Component", 2);
        var rightArmComponent = new TestComponent("Right Arm Component", 2);
        var unmountedComponent = new TestComponent("Unmounted Component", 2);
        
        leftArmPart.TryAddComponent(leftArmComponent);
        rightArmPart.TryAddComponent(rightArmComponent);
        
        // Act
        var leftArmComponentPart = testUnit.FindComponentPart(leftArmComponent);
        var rightArmComponentPart = testUnit.FindComponentPart(rightArmComponent);
        var unmountedComponentPart = testUnit.FindComponentPart(unmountedComponent);
        
        // Assert
        leftArmComponentPart.ShouldBe(leftArmPart);
        rightArmComponentPart.ShouldBe(rightArmPart);
        unmountedComponentPart.ShouldBeNull();
    }
    
    [Fact]
    public void GetMountedComponentAtLocation_ShouldReturnComponentAtSpecificSlots()
    {
        // Arrange
        var unit = CreateTestUnit();
        var weapon1 = new TestWeapon("Weapon 1", [0, 1]);
        var weapon2 = new TestWeapon("Weapon 2", [2, 3]);
        
        MountWeaponOnUnit(unit, weapon1, PartLocation.LeftArm, [0, 1]);
        MountWeaponOnUnit(unit, weapon2, PartLocation.LeftArm, [2, 3]);
        
        // Act
        var foundWeapon1 = unit.GetMountedComponentAtLocation<Weapon>(PartLocation.LeftArm, [0, 1]);
        var foundWeapon2 = unit.GetMountedComponentAtLocation<Weapon>(PartLocation.LeftArm, [2, 3]);
        var notFoundWeapon = unit.GetMountedComponentAtLocation<Weapon>(PartLocation.LeftArm, [4, 5]);
        
        // Assert
        foundWeapon1.ShouldNotBeNull();
        foundWeapon1.ShouldBe(weapon1);
        
        foundWeapon2.ShouldNotBeNull();
        foundWeapon2.ShouldBe(weapon2);
        
        notFoundWeapon.ShouldBeNull();
    }
    
    [Fact]
    public void GetMountedComponentAtLocation_ShouldReturnNull_WhenEmptySlots()
    {
        // Arrange
        var unit = CreateTestUnit();
        var weapon = new TestWeapon("Weapon", [0, 1]);
        MountWeaponOnUnit(unit, weapon, PartLocation.LeftArm, [0, 1]);
        
        // Act
        var result = unit.GetMountedComponentAtLocation<Weapon>(PartLocation.LeftArm, []);
        
        // Assert
        result.ShouldBeNull();
    }
    
    [Fact]
    public void DeclareWeaponAttack_ShouldThrowException_WhenNotDeployed()
    {
        // Arrange
        var unit = CreateTestUnit();
        var targetUnit = CreateTestUnit();
        var weaponTargets = new List<WeaponTargetData>
        {
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Test Weapon",
                    Location = PartLocation.LeftArm,
                    Slots = [0, 1]
                },
                TargetId = targetUnit.Id,
                IsPrimaryTarget = true
            }
        };
        
        // Act
        var act = () => unit.DeclareWeaponAttack(weaponTargets, [targetUnit]);
        
        // Assert
        var ex = Should.Throw<InvalidOperationException>(act);
        ex.Message.ShouldBe("Unit is not deployed.");
    }
    
    [Fact]
    public void DeclareWeaponAttack_ShouldAssignTargetsToWeapons()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var attacker = CreateTestUnit(attackerId);
        var target = CreateTestUnit(targetId);
        
        var weapon = new TestWeapon("Test Weapon", [0, 1]);
        MountWeaponOnUnit(attacker, weapon, PartLocation.LeftArm, [0, 1]);
        
        attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom));
        target.Deploy(new HexPosition(new HexCoordinates(1, 2), HexDirection.Top));
        
        var weaponTargets = new List<WeaponTargetData>
        {
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Test Weapon",
                    Location = PartLocation.LeftArm,
                    Slots = [0, 1]
                },
                TargetId = targetId,
                IsPrimaryTarget = true
            }
        };
        
        // Act
        attacker.DeclareWeaponAttack(weaponTargets, [target]);
        
        // Assert
        weapon.Target.ShouldNotBeNull();
        weapon.Target.ShouldBe(target);
        attacker.HasDeclaredWeaponAttack.ShouldBeTrue();
    }
    
    [Fact]
    public void DeclareWeaponAttack_ShouldHandleMultipleWeapons()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId1 = Guid.NewGuid();
        var targetId2 = Guid.NewGuid();
        
        var attacker = CreateTestUnit(attackerId);
        var target1 = CreateTestUnit(targetId1);
        var target2 = CreateTestUnit(targetId2);
        
        var weapon1 = new TestWeapon("Weapon 1", [0, 1]);
        var weapon2 = new TestWeapon("Weapon 2", [2, 3]);
        
        MountWeaponOnUnit(attacker, weapon1, PartLocation.LeftArm, [0, 1]);
        MountWeaponOnUnit(attacker, weapon2, PartLocation.RightArm, [2, 3]);
        
        attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom));
        target1.Deploy(new HexPosition(new HexCoordinates(1, 2), HexDirection.Top));
        target2.Deploy(new HexPosition(new HexCoordinates(1, 3), HexDirection.Top));
        
        var weaponTargets = new List<WeaponTargetData>
        {
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Weapon 1",
                    Location = PartLocation.LeftArm,
                    Slots = [0, 1]
                },
                TargetId = targetId1,
                IsPrimaryTarget = true
            },
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Weapon 2",
                    Location = PartLocation.RightArm,
                    Slots = [2, 3]
                },
                TargetId = targetId2,
                IsPrimaryTarget = true
            }
        };
        
        // Act
        attacker.DeclareWeaponAttack(weaponTargets, [target1, target2]);
        
        // Assert
        weapon1.Target.ShouldNotBeNull();
        weapon1.Target.ShouldBe(target1);
        
        weapon2.Target.ShouldNotBeNull();
        weapon2.Target.ShouldBe(target2);
        
        attacker.HasDeclaredWeaponAttack.ShouldBeTrue();
    }
    
    [Fact]
    public void DeclareWeaponAttack_ShouldSkipWeaponsNotFound()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var attacker = CreateTestUnit(attackerId);
        var target = CreateTestUnit(targetId);
        
        var weapon = new TestWeapon("Test Weapon", [0, 1]);
        MountWeaponOnUnit(attacker, weapon, PartLocation.LeftArm, [0, 1]);
        
        attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom));
        target.Deploy(new HexPosition(new HexCoordinates(1, 2), HexDirection.Top));
        
        var weaponTargets = new List<WeaponTargetData>
        {
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Test Weapon",
                    Location = PartLocation.LeftArm,
                    Slots = [0, 1]
                },
                TargetId = targetId,
                IsPrimaryTarget = true
            },
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Non-existent Weapon",
                    Location = PartLocation.RightArm,
                    Slots = [4, 5]
                },
                TargetId = targetId,
                IsPrimaryTarget = false
            }
        };
        
        // Act
        attacker.DeclareWeaponAttack(weaponTargets, [target]);
        
        // Assert
        weapon.Target.ShouldNotBeNull();
        weapon.Target.ShouldBe(target);
        attacker.HasDeclaredWeaponAttack.ShouldBeTrue();
    }
    
    [Fact]
    public void DeclareWeaponAttack_ShouldSkipTargetsNotFound()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var nonExistentTargetId = Guid.NewGuid();
        
        var attacker = CreateTestUnit(attackerId);
        var target = CreateTestUnit(targetId);
        
        var weapon1 = new TestWeapon("Weapon 1", [0, 1]);
        var weapon2 = new TestWeapon("Weapon 2", [2, 3]);
        
        MountWeaponOnUnit(attacker, weapon1, PartLocation.LeftArm, [0, 1]);
        MountWeaponOnUnit(attacker, weapon2, PartLocation.RightArm, [2, 3]);
        
        attacker.Deploy(new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom));
        target.Deploy(new HexPosition(new HexCoordinates(1, 2), HexDirection.Top));
        
        var weaponTargets = new List<WeaponTargetData>
        {
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Weapon 1",
                    Location = PartLocation.LeftArm,
                    Slots = [0, 1]
                },
                TargetId = targetId,
                IsPrimaryTarget = true
            },
            new()
            {
                Weapon = new WeaponData
                {
                    Name = "Weapon 2",
                    Location = PartLocation.RightArm,
                    Slots = [2, 3]
                },
                TargetId = nonExistentTargetId,
                IsPrimaryTarget = true
            }
        };
        
        // Act
        attacker.DeclareWeaponAttack(weaponTargets, [target]);
        
        // Assert
        weapon1.Target.ShouldNotBeNull();
        weapon1.Target.ShouldBe(target);
        
        weapon2.Target.ShouldBeNull();
        
        attacker.HasDeclaredWeaponAttack.ShouldBeTrue();
    }
    
    // Helper class for testing generic methods
    private class TestDerivedComponent(string name, int size = 1) : TestComponent(name, size);
}
