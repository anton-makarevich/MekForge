using Microsoft.Extensions.DependencyInjection;
using Sanet.MekForge.Avalonia.Services;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.DI;

public static class CoreServices
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IImageService, AvaloniaAssetImageService>();
    }
    public static void RegisterViewModels(this IServiceCollection services)
    {
        services.AddTransient<NewGameViewModel, NewGameViewModel>();
        services.AddTransient<BattleMapViewModel, BattleMapViewModel>();
    }
}