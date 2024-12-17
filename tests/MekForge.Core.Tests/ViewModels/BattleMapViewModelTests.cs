using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.ViewModels;

public class BattleMapViewModelTests
{
    [Fact]
    public void GameUpdates_RaiseNotifyPropertyChanged()
    {
        // Arrange
        var gameMock = Substitute.For<IGame>();
        var viewModel = new BattleMapViewModel(Substitute.For<IImageService>())
        {
            Game = gameMock
        };
        
        // Act and Assert
        gameMock.Turn.Returns(1);
        viewModel.Turn.Should().Be(1);
        gameMock.TurnPhase.Returns(Phase.Start);
        viewModel.TurnPhase.Should().Be(Phase.Start);
        gameMock.ActivePlayer.Returns(new Player(Guid.Empty, "Player1"));
        viewModel.ActivePlayerName.Should().Be( "Player1");
    }
}