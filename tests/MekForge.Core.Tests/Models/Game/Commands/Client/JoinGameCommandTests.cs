using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class JoinGameCommandTests : GameCommandTestBase<JoinGameCommand>
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public JoinGameCommandTests()
    {
        _game.Players.Returns([_player1]);
    }

    protected override JoinGameCommand CreateCommand()
    {
        return new JoinGameCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            PlayerName = _player1.Name,
            Units = [],
            Tint = "#FF0000"
        };
    }

    protected override void AssertCommandSpecificProperties(JoinGameCommand original, JoinGameCommand? cloned)
    {
        base.AssertCommandSpecificProperties(original, cloned);
        cloned!.PlayerId.ShouldBe(original.PlayerId);
        cloned.PlayerName.ShouldBe(original.PlayerName);
        cloned.Units.ShouldBeEquivalentTo(original.Units);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();

        _localizationService.GetString("Command_JoinGame").Returns("formatted join command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBe("formatted join command");
        _localizationService.Received(1).GetString("Command_JoinGame");
    }
}