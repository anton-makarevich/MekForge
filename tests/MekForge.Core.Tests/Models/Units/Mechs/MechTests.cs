using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class MechTests
{
    private static List<UnitPart> CreateBasicPartsData()
    {
        return new List<UnitPart>
        {
            new Head( 9, 3),
            new CenterTorso( 31, 10, 6),
            new SideTorso(PartLocation.LeftTorso, 25, 8, 6),
            new SideTorso(PartLocation.RightTorso, 25, 8, 6),
            new Arm(PartLocation.RightArm, 17, 6),
            new Arm(PartLocation.LeftArm, 17, 6),
            new Leg(PartLocation.RightLeg, 25, 8),
            new Leg(PartLocation.LeftLeg, 25, 8)
        };
    }

    [Fact]
    public void Constructor_InitializesAllParts()
    {
        // Arrange & Act
        var mech = new Mech("Test", "TST-1A", 50, 4, CreateBasicPartsData());

        // Assert
        mech.Parts.Should().HaveCount(8, "all mech locations should be initialized");
        mech.Parts.Should().Contain(p => p.Location == PartLocation.Head);
        mech.Parts.Should().Contain(p => p.Location == PartLocation.CenterTorso);
        mech.Parts.Should().Contain(p => p.Location == PartLocation.LeftTorso);
        mech.Parts.Should().Contain(p => p.Location == PartLocation.RightTorso);
        mech.Parts.Should().Contain(p => p.Location == PartLocation.LeftArm);
        mech.Parts.Should().Contain(p => p.Location == PartLocation.RightArm);
        mech.Parts.Should().Contain(p => p.Location == PartLocation.LeftLeg);
        mech.Parts.Should().Contain(p => p.Location == PartLocation.RightLeg);
    }

    [Theory]
    [InlineData(PartLocation.LeftArm, PartLocation.LeftTorso)]
    [InlineData(PartLocation.RightArm, PartLocation.RightTorso)]
    [InlineData(PartLocation.LeftLeg, PartLocation.LeftTorso)]
    [InlineData(PartLocation.RightLeg, PartLocation.RightTorso)]
    [InlineData(PartLocation.LeftTorso, PartLocation.CenterTorso)]
    [InlineData(PartLocation.RightTorso, PartLocation.CenterTorso)]
    public void GetTransferLocation_ReturnsCorrectLocation(PartLocation from, PartLocation expected)
    {
        // Arrange
        var mech = new Mech("Test", "TST-1A", 50, 4, CreateBasicPartsData());

        // Act
        var transferLocation = mech.TestGetTransferLocation(from);

        // Assert
        transferLocation.Should().Be(expected);
    }

    [Fact]
    public void ApplyHeat_DissipatesHeatBasedOnHeatSinks()
    {
        // Arrange
        var parts = CreateBasicPartsData();
        var centerTorso = parts.Single(p => p.Location == PartLocation.CenterTorso);
        centerTorso.TryAddComponent(new HeatSink());
        centerTorso.TryAddComponent(new HeatSink());
        var mech = new Mech("Test", "TST-1A", 50, 4, parts);

        // Act
        mech.ApplyHeat(5); // Apply 5 heat with 2 heat sinks

        // Assert
        mech.CurrentHeat.Should().Be(3, "5 heat - 2 heat sinks = 3 heat");
    }

    [Fact]
    public void CalculateBattleValue_IncludesWeapons()
    {
        // Arrange
        var parts = CreateBasicPartsData();
        var centerTorso = parts.Single(p => p.Location == PartLocation.CenterTorso);
        centerTorso.TryAddComponent(new MediumLaser());
        var mech = new Mech("Test", "TST-1A", 50, 4, parts);

        // Act
        var bv = mech.CalculateBattleValue();

        // Assert
        bv.Should().Be(5046, "5000 (base BV for 50 tons) + 46 (medium laser)");
    }

    [Fact]
    public void Status_StartsActive()
    {
        // Arrange & Act
        var mech = new Mech("Test", "TST-1A", 50, 4, CreateBasicPartsData());

        // Assert
        mech.Status.Should().Be(MechStatus.Active);
    }

    [Fact]
    public void Shutdown_ChangesStatusToShutdown()
    {
        // Arrange
        var mech = new Mech("Test", "TST-1A", 50, 4, CreateBasicPartsData());

        // Act
        mech.Shutdown();

        // Assert
        mech.Status.Should().Be(MechStatus.Shutdown);
    }

    [Fact]
    public void Startup_ChangesStatusToActive()
    {
        // Arrange
        var mech = new Mech("Test", "TST-1A", 50, 4, CreateBasicPartsData());
        mech.Shutdown();

        // Act
        mech.Startup();

        // Assert
        mech.Status.Should().Be(MechStatus.Active);
    }

    [Fact]
    public void SetProne_AddsProneStatus()
    {
        // Arrange
        var mech = new Mech("Test", "TST-1A", 50, 4, CreateBasicPartsData());

        // Act
        mech.SetProne();

        // Assert
        (mech.Status & MechStatus.Prone).Should().Be(MechStatus.Prone);
    }

    [Fact]
    public void StandUp_RemovesProneStatus()
    {
        // Arrange
        var mech = new Mech("Test", "TST-1A", 50, 4, CreateBasicPartsData());
        mech.SetProne();

        // Act
        mech.StandUp();

        // Assert
        (mech.Status & MechStatus.Prone).Should().NotBe(MechStatus.Prone);
    }

    [Theory]
    [InlineData(5, 8, 2)] // Standard mech without jump jets
    [InlineData(4, 6, 0)] // Fast mech with jump jets
    [InlineData(3, 5, 2)] // Slow mech with lots of jump jets
    public void GetMovement_ReturnsCorrectMPs(int walkMp, int runMp, int jumpMp)
    {
        // Arrange
        var parts = CreateBasicPartsData();
        if (jumpMp > 0)
        {
            var centerTorso = parts.Single(p => p.Location == PartLocation.CenterTorso);
            centerTorso.TryAddComponent(new JumpJets());
            centerTorso.TryAddComponent(new JumpJets());
        }

        var mech = new Mech("Test", "TST-1A", 50, walkMp, parts);

        // Act
        var walkingMp = mech.GetMovementPoints(MovementType.Walk);
        var runningMp = mech.GetMovementPoints(MovementType.Run);
        var jumpingMp = mech.GetMovementPoints(MovementType.Jump);

        // Assert
        walkingMp.Should().Be(walkMp, "walking MP should match the base movement");
        runningMp.Should().Be(runMp, "running MP should be 1.5x walking");
        jumpingMp.Should().Be(jumpMp, "jumping MP should match the number of jump jets");
    }

    [Theory]
    [InlineData(105, WeightClass.Unknown)]
    [InlineData(20, WeightClass.Light)]
    [InlineData(25, WeightClass.Light)]
    [InlineData(30, WeightClass.Light)]
    [InlineData(35, WeightClass.Light)]
    [InlineData(40, WeightClass.Medium)]
    [InlineData(45, WeightClass.Medium)]
    [InlineData(50, WeightClass.Medium)]
    [InlineData(55, WeightClass.Medium)]
    [InlineData(60, WeightClass.Heavy)]
    [InlineData(65, WeightClass.Heavy)]
    [InlineData(70, WeightClass.Heavy)]
    [InlineData(75, WeightClass.Heavy)]
    [InlineData(80, WeightClass.Assault)]
    [InlineData(85, WeightClass.Assault)]
    [InlineData(90, WeightClass.Assault)]
    [InlineData(95, WeightClass.Assault)]
    [InlineData(100, WeightClass.Assault)]
    public void WeightClass_Calculation_ReturnsCorrectClass(int tonnage, WeightClass expectedClass)
    {
        // Arrange
        var mech = new Mech("Test", "TST-1A", tonnage, 4, CreateBasicPartsData());

        // Act
        var weightClass = mech.Class;

        // Assert
        weightClass.Should().Be(expectedClass);
    }
}

// Helper extension for testing protected methods
public static class MechTestExtensions
{
    public static PartLocation? TestGetTransferLocation(this Mech mech, PartLocation location)
    {
        var method = typeof(Mech).GetMethod("GetTransferLocation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (PartLocation?)method?.Invoke(mech, new object[] { location });
    }
}
