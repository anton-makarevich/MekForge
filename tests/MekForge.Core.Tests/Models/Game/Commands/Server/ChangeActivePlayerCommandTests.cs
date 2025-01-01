using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Server;

public class ChangeActivePlayerCommandTests : GameCommandTestBase<ChangeActivePlayerCommand>
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public ChangeActivePlayerCommandTests()
    {
        _game.Players.Returns([_player1]);
    }

    protected override ChangeActivePlayerCommand CreateCommand()
    {
        return new ChangeActivePlayerCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id
        };
    }

    protected override void AssertCommandSpecificProperties(ChangeActivePlayerCommand original, ChangeActivePlayerCommand? cloned)
    {
        base.AssertCommandSpecificProperties(original, cloned);
        cloned!.PlayerId.Should().Be(original.PlayerId);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();

        _localizationService.GetString("Command_ChangeActivePlayer").Returns("formatted active player command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted active player command");
        _localizationService.Received(1).GetString("Command_ChangeActivePlayer");
    }
    
    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = new ChangeActivePlayerCommand
        {
            GameOriginId = _gameId,
            PlayerId = Guid.NewGuid()
        };
        
        // Act
        var result = command.Format(_localizationService, _game);
        
        // Assert
        result.Should().BeEmpty();
    }
}