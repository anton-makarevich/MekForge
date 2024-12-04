using AsyncAwaitBestPractices.MVVM;
using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Terrains;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MVVM.Core.Services;

namespace Sanet.MekForge.Core.Tests.ViewModels;

public class NewGameViewModelTests
{
    private readonly NewGameViewModel _sut;
    private readonly INavigationService _navigationService;
    private readonly BattleMapViewModel _battleMapViewModel;

    public NewGameViewModelTests()
    {
        _navigationService = Substitute.For<INavigationService>();
        var imageService = Substitute.For<IImageService>();
        _battleMapViewModel = new BattleMapViewModel(imageService);
        _navigationService.GetViewModel<BattleMapViewModel>().Returns(_battleMapViewModel);

        _sut = new NewGameViewModel();
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

        _battleMapViewModel.BattleMap.Should().NotBeNull();
        var hex = _battleMapViewModel.BattleMap!.GetHexes().First();
        hex.GetTerrains().Should().HaveCount(1);
        hex.GetTerrains().First().Should().BeOfType<ClearTerrain>();
    }

    [Fact]
    public async Task StartGameCommand_WithForestCoverage_CreatesForestMap()
    {
        _sut.ForestCoverage = 100;
        _sut.LightWoodsPercentage = 100;
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        _battleMapViewModel.BattleMap.Should().NotBeNull();
        var hexes = _battleMapViewModel.BattleMap!.GetHexes().ToList();
        hexes.Should().Contain(h => h.GetTerrains().Any(t => t is LightWoodsTerrain));
    }

    [Fact]
    public async Task StartGameCommand_NavigatesToBattleMap()
    {
        await ((IAsyncCommand)_sut.StartGameCommand).ExecuteAsync();

        await _navigationService.Received(1).NavigateToViewModelAsync(_battleMapViewModel);
    }
}
