using AsyncAwaitBestPractices.MVVM;
using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game;
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

    public NewGameViewModelTests()
    {
        _navigationService = Substitute.For<INavigationService>();
        var imageService = Substitute.For<IImageService>();
        _battleMapViewModel = new BattleMapViewModel(imageService);
        _navigationService.GetViewModel<BattleMapViewModel>().Returns(_battleMapViewModel);
        
        var rulesProvider = Substitute.For<IRulesProvider>();
        
        _gameManager = Substitute.For<IGameManager>();
        var commandPublisher = Substitute.For<ICommandPublisher>();

        _sut = new NewGameViewModel(_gameManager,rulesProvider,commandPublisher);
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
        _sut.SelectedUnit = MechFactoryTests.CreateDummyMechData();
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
        _sut.SelectedUnit = MechFactoryTests.CreateDummyMechData();
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        _battleMapViewModel.Game.Should().NotBeNull();
        var hexes = _battleMapViewModel.Game!.GetHexes().ToList();
        hexes.Should().Contain(h => h.GetTerrains().Any(t => t is LightWoodsTerrain));
    }

    [Fact]
    public async Task StartGameCommand_NavigatesToBattleMap()
    {
        _sut.SelectedUnit = MechFactoryTests.CreateDummyMechData();
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
        _sut.SelectedUnit = units[0];

        // Act
        await ((AsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        // Assert
        await _navigationService.Received(1).NavigateToViewModelAsync(_battleMapViewModel);
        _gameManager.Received(1).StartServer(Arg.Any<BattleMap>());
    }

    [Fact]
    public void InitializeUnits_ShouldPopulateAvailableUnits()
    {
        // Arrange
        var units = new List<UnitData> { MechFactoryTests.CreateDummyMechData() };

        // Act
        _sut.InitializeUnits(units);

        // Assert
        _sut.AvailableUnits.Should().HaveCount(1);
    }

    [Fact]
    public void CanStartGame_ShouldReturnTrue_WhenUnitIsSelected()
    {
        // Arrange
        var unit = MechFactoryTests.CreateDummyMechData();
        _sut.InitializeUnits([unit]);
        _sut.SelectedUnit = unit;

        // Act
        var canStart = _sut.CanStartGame;

        // Assert
        canStart.Should().BeTrue();
    }

    [Fact]
    public void CanStartGame_ShouldReturnFalse_WhenNoUnitIsSelected()
    {
        // Arrange
        _sut.SelectedUnit = null;

        // Act
        var canStart = _sut.CanStartGame;

        // Assert
        canStart.Should().BeFalse();
    }
}
