using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.Avalonia.Services;
using Sanet.MakaMek.Core.Models.Game;
using Sanet.MakaMek.Core.Models.Game.Combat;
using Sanet.MakaMek.Core.Models.Game.Dice;
using Sanet.MakaMek.Core.Models.Game.Transport;
using Sanet.MakaMek.Core.Services;
using Sanet.MakaMek.Core.Services.Localization;
using Sanet.MakaMek.Core.Utils.TechRules;
using Sanet.MakaMek.Core.ViewModels;
using Sanet.Transport;
using Sanet.Transport.Rx;

namespace Sanet.MakaMek.Avalonia.DI;

public static class CoreServices
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IImageService, AvaloniaAssetImageService>();
        services.AddSingleton<ILocalizationService, FakeLocalizationService>();
        
        // Register RxTransportPublisher for local players
        services.AddSingleton<RxTransportPublisher>();
        
        // Register SignalRHostService for LAN players
        services.AddSingleton<SignalRHostService>();
        
        // Register CommandTransportAdapter with both publishers
        services.AddSingleton<CommandTransportAdapter>(sp => 
            new CommandTransportAdapter(
                sp.GetRequiredService<RxTransportPublisher>(),
                sp.GetRequiredService<SignalRHostService>().Publisher));
            
        services.AddSingleton<ICommandPublisher, CommandPublisher>();
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