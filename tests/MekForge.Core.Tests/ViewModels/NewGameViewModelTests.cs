using AsyncAwaitBestPractices.MVVM;
using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
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
        var localizationService = Substitute.For<ILocalizationService>();
        var imageService = Substitute.For<IImageService>();
        _battleMapViewModel = new BattleMapViewModel(imageService, localizationService);
        _navigationService.GetViewModel<BattleMapViewModel>().Returns(_battleMapViewModel);
        
        var rulesProvider = Substitute.For<IRulesProvider>();
        
        _gameManager = Substitute.For<IGameManager>();
        
        _commandPublisher = Substitute.For<ICommandPublisher>();

        _sut = new NewGameViewModel(_gameManager,rulesProvider,_commandPublisher,
            Substitute.For<IToHitCalculator>());
        _sut.SetNavigationService(_navigationService);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        _sut.MapWidth.ShouldBe(15);
        _sut.MapHeight.ShouldBe(17);
        _sut.ForestCoverage.ShouldBe(20);
        _sut.LightWoodsPercentage.ShouldBe(30);
        _sut.IsLightWoodsEnabled.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    public void ForestCoverage_WhenChanged_UpdatesLightWoodsEnabled(int coverage, bool expectedEnabled)
    {
        _sut.ForestCoverage = coverage;

        _sut.IsLightWoodsEnabled.ShouldBe(expectedEnabled);
    }

    [Fact]
    public async Task StartGameCommand_WithZeroForestCoverage_CreatesClearTerrainMap()
    {
        _sut.ForestCoverage = 0;
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        _battleMapViewModel.Game.ShouldNotBeNull();
        var hex = _battleMapViewModel.Game!.BattleMap.GetHexes().First();
        hex.GetTerrains().ToList().Count.ShouldBe(1);
        hex.GetTerrains().First().ShouldBeOfType<ClearTerrain>();
    }

    [Fact]
    public async Task StartGameCommand_WithForestCoverage_CreatesForestMap()
    {
        _sut.ForestCoverage = 100;
        _sut.LightWoodsPercentage = 100;
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        _battleMapViewModel.Game.ShouldNotBeNull();
        var hexes = _battleMapViewModel.Game!.BattleMap.GetHexes().ToList();
        hexes.ShouldContain(h => h.GetTerrains().Any(t => t is LightWoodsTerrain));
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
        _sut.MapWidth.ShouldBe(newWidth);
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
        _commandPublisher.Received(1)!.PublishCommand(Arg.Is<JoinGameCommand>(g => g.Units.First().Id != Guid.Empty ));
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
        _sut.Players.Count.ShouldBe(initialPlayerCount + 1);
        _sut.CanAddPlayer.ShouldBeTrue();
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
        _sut.Players.Count.ShouldBe(initialPlayerCount); // Should not increase
        _sut.CanAddPlayer.ShouldBeFalse();
    }
    
    [Fact]
    public void CanStartGame_ShouldBeFalse_WhenNoPlayers()
    {
        // Arrange
        // No players added

        // Act
        var result = _sut.CanStartGame;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanStartGame_ShouldBeFalse_WhenPlayersHaveNoUnits()
    {
        // Arrange
        _sut.AddPlayerCommand.Execute(null); // Add a player

        // Act
        var result = _sut.CanStartGame;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanStartGame_ShouldBeTrue_WhenPlayersHaveUnits()
    {
        // Arrange
        var units = new List<UnitData> { MechFactoryTests.CreateDummyMechData() };
        _sut.InitializeUnits(units);
        _sut.AddPlayerCommand.Execute(null);
        _sut.Players.First().SelectedUnit = units.First();
        _sut.Players.First().AddUnitCommand.Execute(null);
    
        // Act
        var result = _sut.CanStartGame;
    
        // Assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void CanStartGame_ShouldBeFalse_WhenOnePlayerHasNoUnits()
    {
        // Arrange
        var units = new List<UnitData> { MechFactoryTests.CreateDummyMechData() };
        _sut.InitializeUnits(units);
        _sut.AddPlayerCommand.Execute(null); // first player
        _sut.AddPlayerCommand.Execute(null); // second player
        _sut.Players.First().SelectedUnit = units.First();
        _sut.Players.First().AddUnitCommand.Execute(null);
    
        // Act
        var result = _sut.CanStartGame;
    
        // Assert
        result.ShouldBeFalse();
    }
}
