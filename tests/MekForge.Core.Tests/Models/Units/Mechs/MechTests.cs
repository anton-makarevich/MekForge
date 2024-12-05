using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class MechTests
{
    private static Dictionary<PartLocation, UnitPartData> CreateBasicPartsData()
    {
        return new Dictionary<PartLocation, UnitPartData>
        {
            [PartLocation.Head] = new("Head", 9, 3, 6, []),
            [PartLocation.CenterTorso] = new("Center Torso", 31, 10, 6, []),
            [PartLocation.LeftTorso] = new("Left Torso", 25, 8, 6, []),
            [PartLocation.RightTorso] = new("Right Torso", 25, 8, 6, []),
            [PartLocation.LeftArm] = new("Left Arm", 17, 6, 6, []),
            [PartLocation.RightArm] = new("Right Arm", 17, 6, 6, []),
            [PartLocation.LeftLeg] = new("Left Leg", 25, 8, 6, []),
            [PartLocation.RightLeg] = new("Right Leg", 25, 8, 6, [])
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
    [InlineData(PartLocation.Head, PartLocation.CenterTorso)]
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
        var partsData = CreateBasicPartsData();
        partsData[PartLocation.CenterTorso] = partsData[PartLocation.CenterTorso] with
        {
            Components = [new HeatSink(), new HeatSink()]
        };
        var mech = new Mech("Test", "TST-1A", 50, 4, partsData);

        // Act
        mech.ApplyHeat(5); // Apply 5 heat with 2 heat sinks

        // Assert
        mech.CurrentHeat.Should().Be(3, "5 heat - 2 heat sinks = 3 heat");
    }

    [Fact]
    public void CalculateBattleValue_IncludesWeapons()
    {
        // Arrange
        var partsData = CreateBasicPartsData();
        partsData[PartLocation.RightArm] = partsData[PartLocation.RightArm] with
        {
            Components = [new MediumLaser()]
        };
        var mech = new Mech("Test", "TST-1A", 50, 4, partsData);

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
    public void GetMovement_ReturnsCorrectMPs(int walkMP, int expectedRunMP, int jumpMP)
    {
        // Arrange
        var partsData = CreateBasicPartsData();
        if (jumpMP > 0)
        {
            partsData[PartLocation.LeftLeg] = partsData[PartLocation.LeftLeg] with
            {
                Components = [new JumpJets()]
            };
            partsData[PartLocation.RightLeg] = partsData[PartLocation.RightLeg] with
            {
                Components = [new JumpJets()]
            };
        }

        var mech = new Mech("Test", "TST-1A", 50, walkMP, partsData);

        // Act
        var walkingMP = mech.GetMovementPoints(MovementType.Walk);
        var runningMP = mech.GetMovementPoints(MovementType.Run);
        var jumpingMP = mech.GetMovementPoints(MovementType.Jump);

        // Assert
        walkingMP.Should().Be(walkMP, "walking MP should match the base movement");
        runningMP.Should().Be(expectedRunMP, "running MP should be 1.5x walking");
        jumpingMP.Should().Be(jumpMP, "jumping MP should match the number of jump jets");
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

