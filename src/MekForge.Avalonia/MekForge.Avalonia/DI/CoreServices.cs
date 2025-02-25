using Microsoft.Extensions.DependencyInjection;
using Sanet.MekForge.Avalonia.Game.Transport;
using Sanet.MekForge.Avalonia.Services;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.DI;

public static class CoreServices
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IImageService, AvaloniaAssetImageService>();
        services.AddSingleton<ILocalizationService, FakeLocalizationService>();
        services.AddSingleton<ICommandPublisher, RxCommandPublisher>();
        services.AddSingleton<IRulesProvider, ClassicBattletechRulesProvider>();
        services.AddSingleton<IDiceRoller, RandomDiceRoller>();
        services.AddSingleton<IToHitCalculator, ClassicToHitCalculator>();
        services.AddSingleton<IGameManager, GameManager>();
    }
    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<NewGameViewModel, NewGameViewModel>();
        services.AddTransient<BattleMapViewModel, BattleMapViewModel>();
    }
}