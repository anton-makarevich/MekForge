using NSubstitute;
using Sanet.MekForge.Avalonia.Views.NewGame;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;
using Shouldly;

namespace MekForge.Avalonia.Tests.Views
{
    public class NewGameViewTests
    {
        [Fact]
        public void NewGameView_WhenCreated_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var view = new NewGameViewNarrow();

            // Assert
            view.ShouldNotBeNull();
        }

        [Fact]
        public void NewGameView_WhenViewModelSet_ShouldBindCorrectly()
        {
            // Arrange
            var view = new NewGameViewNarrow();
            var viewModel = new NewGameViewModel(Substitute.For<IGameManager>(), 
                Substitute.For<IRulesProvider>(),
                Substitute.For<ICommandPublisher>());

            // Act
            view.DataContext = viewModel;

            // Assert
            view.DataContext.ShouldBe(viewModel);
        }
    }
}
