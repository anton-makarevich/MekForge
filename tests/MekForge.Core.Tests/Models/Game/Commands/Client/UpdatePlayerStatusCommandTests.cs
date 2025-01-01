using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class UpdatePlayerStatusCommandTests : GameCommandTestBase<UpdatePlayerStatusCommand>
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public UpdatePlayerStatusCommandTests()
    {
        _game.Players.Returns([_player1]);
    }

    protected override UpdatePlayerStatusCommand CreateCommand()
    {
        return new UpdatePlayerStatusCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            PlayerStatus = PlayerStatus.Playing
        };
    }

    protected override void AssertCommandSpecificProperties(UpdatePlayerStatusCommand original, UpdatePlayerStatusCommand? cloned)
    {
        base.AssertCommandSpecificProperties(original, cloned);
        cloned!.PlayerId.Should().Be(original.PlayerId);
        cloned.PlayerStatus.Should().Be(original.PlayerStatus);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();
        _localizationService.GetString("Command_UpdatePlayerStatus").Returns("formatted status command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted status command");
        _localizationService.Received(1).GetString("Command_UpdatePlayerStatus");
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = CreateCommand() with { PlayerId = Guid.NewGuid() };

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().BeEmpty();
    }
}