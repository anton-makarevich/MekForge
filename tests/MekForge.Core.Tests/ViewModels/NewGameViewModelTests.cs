using AsyncAwaitBestPractices.MVVM;
using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MVVM.Core.Services;

namespace Sanet.MekForge.Core.Tests.ViewModels;

public class NewGameViewModelTests
{
    private readonly NewGameViewModel _sut;
    private readonly INavigationService _navigationService;
    private readonly BattleMapViewModel _battleMapViewModel;
    private readonly IGameManager _gameManager;
    private readonly ICommandPublisher? _commandPublisher;

    public NewGameViewModelTests()
    {
        _navigationService = Substitute.For<INavigationService>();
        var imageService = Substitute.For<IImageService>();
        _battleMapViewModel = new BattleMapViewModel(imageService);
        _navigationService.GetViewModel<BattleMapViewModel>().Returns(_battleMapViewModel);
        
        var rulesProvider = Substitute.For<IRulesProvider>();
        
        _gameManager = Substitute.For<IGameManager>();
        
        _commandPublisher = Substitute.For<ICommandPublisher>();

        _sut = new NewGameViewModel(_gameManager,rulesProvider,_commandPublisher);
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        _sut.MapWidth.Should().Be(15);
        _sut.MapHeight.Should().Be(17);
        _sut.ForestCoverage.Should().Be(20);
        _sut.LightWoodsPercentage.Should().Be(30);
        _sut.IsLightWoodsEnabled.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    public void ForestCoverage_WhenChanged_UpdatesLightWoodsEnabled(int coverage, bool expectedEnabled)
    {
        _sut.ForestCoverage = coverage;

        _sut.IsLightWoodsEnabled.Should().Be(expectedEnabled);
    }

    [Fact]
    public async Task StartGameCommand_WithZeroForestCoverage_CreatesClearTerrainMap()
    {
        _sut.ForestCoverage = 0;
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        _battleMapViewModel.Game.Should().NotBeNull();
        var hex = _battleMapViewModel.Game!.GetHexes().First();
        hex.GetTerrains().Should().HaveCount(1);
        hex.GetTerrains().First().Should().BeOfType<ClearTerrain>();
    }

    [Fact]
    public async Task StartGameCommand_WithForestCoverage_CreatesForestMap()
    {
        _sut.ForestCoverage = 100;
        _sut.LightWoodsPercentage = 100;
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        _battleMapViewModel.Game.Should().NotBeNull();
        var hexes = _battleMapViewModel.Game!.GetHexes().ToList();
        hexes.Should().Contain(h => h.GetTerrains().Any(t => t is LightWoodsTerrain));
    }

    [Fact]
    public async Task StartGameCommand_NavigatesToBattleMap()
    {
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        await _navigationService.Received(1).NavigateToViewModelAsync(_battleMapViewModel);
    }

    [Fact]
    public void MapWidth_SetAndGet_ShouldUpdateCorrectly()
    {
        // Arrange
        var newWidth = 20;

        // Act
        _sut.MapWidth = newWidth;

        // Assert
        _sut.MapWidth.Should().Be(newWidth);
    }

    [Fact]
    public async Task StartGameCommand_ShouldInitializeGame_WhenExecuted()
    {
        // Arrange
        var units = new List<UnitData> { MechFactoryTests.CreateDummyMechData() };
        _sut.InitializeUnits(units);
        _sut.AddPlayerCommand.Execute(null);
        _sut.Players.First().SelectedUnit = units.First();
        _sut.Players.First().AddUnitCommand.Execute(null);

        // Act
        await ((AsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        // Assert
        await _navigationService.Received(1).NavigateToViewModelAsync(_battleMapViewModel);
        _commandPublisher.Received(1).PublishCommand(Arg.Is<JoinGameCommand>(g => g.Units.First().Id != Guid.Empty ));
        _gameManager.Received(1).StartServer(Arg.Any<BattleMap>());
    }
    
    [Fact]
    public void AddPlayer_ShouldAddPlayer_WhenLessThanFourPlayers()
    {
        // Arrange
        var initialPlayerCount = _sut.Players.Count;

        // Act
        _sut.AddPlayerCommand.Execute(null);

        // Assert
        _sut.Players.Count.Should().Be(initialPlayerCount + 1);
        _sut.CanAddPlayer.Should().BeTrue();
    }

    [Fact]
    public void AddPlayer_ShouldNotAddPlayer_WhenFourPlayersAlreadyAdded()
    {
        // Arrange
        for (var i = 0; i < 4; i++)
        {
            _sut.AddPlayerCommand.Execute(null);
        }
        var initialPlayerCount = _sut.Players.Count;

        // Act
        _sut.AddPlayerCommand.Execute(null);

        // Assert
        _sut.Players.Count.Should().Be(initialPlayerCount); // Should not increase
        _sut.CanAddPlayer.Should().BeFalse();
    }
}
