using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Server;

public class ChangePhaseCommandTests : GameCommandTestBase<ChangePhaseCommand>
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();

    protected override ChangePhaseCommand CreateCommand()
    {
        return new ChangePhaseCommand
        {
            GameOriginId = _gameId,
            Phase = PhaseNames.Movement
        };
    }

    protected override void AssertCommandSpecificProperties(ChangePhaseCommand original, ChangePhaseCommand? cloned)
    {
        base.AssertCommandSpecificProperties(original, cloned);
        cloned!.Phase.Should().Be(original.Phase);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();
        _localizationService.GetString("Command_ChangePhase").Returns("formatted phase command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted phase command");
        _localizationService.Received(1).GetString("Command_ChangePhase");
    }
}