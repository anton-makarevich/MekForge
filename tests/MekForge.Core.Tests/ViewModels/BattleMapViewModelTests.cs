using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.ViewModels;

public class BattleMapViewModelTests
{
    private readonly IImageService _imageService;
    private readonly BattleMapViewModel _viewModel;
    private IGame _game;

    public BattleMapViewModelTests()
    {
        _imageService = Substitute.For<IImageService>();
        _viewModel = new BattleMapViewModel(_imageService);
        _game = Substitute.For<IGame>();
        _viewModel.Game = _game;
    }

    [Fact]
    public void GameUpdates_RaiseNotifyPropertyChanged()
    {

        // Act and Assert
        _game.Turn.Returns(1);
        _viewModel.Turn.Should().Be(1);
        _game.TurnPhase.Returns(Phase.Start);
        _viewModel.TurnPhase.Should().Be(Phase.Start);
        _game.ActivePlayer.Returns(new Player(Guid.Empty, "Player1"));
        _viewModel.ActivePlayerName.Should().Be("Player1");
    }

    [Fact]
    public async Task UnitsToDeploy_ShouldBeVisible_WhenItsPlayersTurnToDeploy()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();

        var tcs = new TaskCompletionSource<bool>();

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(BattleMapViewModel.UnitsToDeploy))
            {
                tcs.SetResult(true); // Signal that the property has changed
            }
        };

        _game = new ClientGame(BattleMap.GenerateMap(2, 2,
                new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            new[] { player }, new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>());
        _viewModel.Game = _game;

        ((ClientGame)_game).HandleCommand(new ChangePhaseCommand()
        {
            Phase = Phase.Deployment,
            GameOriginId = Guid.NewGuid()
        });
        ((ClientGame)_game).HandleCommand(new JoinGameCommand()
        {
            PlayerId = player.Id,
            Units = [unitData],
            PlayerName = player.Name,
            GameOriginId = Guid.NewGuid()
        });

        // Act
        ((ClientGame)_game).HandleCommand(new ChangeActivePlayerCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid()
        });

        // Wait for the PropertyChanged event
        await tcs.Task;
        
        // Assert
        _viewModel.UnitsToDeploy.Should().ContainSingle();
        _viewModel.AreUnitsToDeployVisible.Should().BeTrue();
        _viewModel.UserActionLabel.Should().Be("Select Unit");
        _viewModel.IsUserActionLabelVisible.Should().BeTrue();
    }
    
    [Fact]
    public void Units_ReturnsAllUnitsFromPlayers()
    {
        // Arrange
        var player1 = new Player(Guid.NewGuid(), "Player1");
        var player2 = new Player(Guid.NewGuid(), "Player2");

        var mechData = MechFactoryTests.CreateDummyMechData();
        var mechFactory = new MechFactory(new ClassicBattletechRulesProvider());
        var unit1 = mechFactory.Create(mechData); 
        var unit2 = mechFactory.Create(mechData); 
    
        player1.AddUnit(unit1);
        player2.AddUnit(unit1);
        player2.AddUnit(unit2);
    
        _game.Players.Returns(new List<Player> { player1, player2 });

        // Act
        var units = _viewModel.Units.ToList();

        // Assert
        units.Should().HaveCount(3);
        units.Should().Contain(unit1);
        units.Should().Contain(unit2);
    }
}