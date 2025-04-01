using NSubstitute;
using Sanet.MakaMek.Avalonia.Views.NewGame;
using Sanet.MakaMek.Core.Models.Game;
using Sanet.MakaMek.Core.Models.Game.Combat;
using Sanet.MakaMek.Core.Models.Game.Transport;
using Sanet.MakaMek.Core.Utils.TechRules;
using Sanet.MakaMek.Core.ViewModels;
using Shouldly;

namespace MakaMek.Avalonia.Tests.Views
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
                Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());

            // Act
            view.DataContext = viewModel;

            // Assert
            view.DataContext.ShouldBe(viewModel);
        }
    }
}
