using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.Avalonia.DI;
using Sanet.MakaMek.Avalonia.Views;
using Sanet.MakaMek.Avalonia.Views.NewGame;
using Sanet.MakaMek.Avalonia.Views.StartNewGame;
using Sanet.MakaMek.Core.ViewModels;
using Sanet.MVVM.Core.Services;
using Sanet.MVVM.Navigation.Avalonia.Services;
using MainWindow = Sanet.MakaMek.Avalonia.Views.MainWindow;

namespace Sanet.MakaMek.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Resources[MVVM.DI.Avalonia.Extensions.AppBuilderExtensions.ServiceCollectionResourceKey] is not IServiceCollection services)
        {
            throw new Exception("Services are not initialized");
        }

        services.RegisterServices();
        services.RegisterViewModels();

        var serviceProvider = services.BuildServiceProvider();
        INavigationService navigationService;

        StartNewGameViewModel? viewModel;
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                navigationService = new NavigationService(desktop, serviceProvider);
                RegisterViews(navigationService);
                viewModel = navigationService.GetViewModel<StartNewGameViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    Content = IsMobile()
                    ? new StartNewGameViewNarrow()
                    {
                        ViewModel = viewModel
                    }
                    : new StartNewGameViewWide
                    {
                        ViewModel = viewModel
                    }
                };

                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                var mainViewWrapper = new ContentControl();
                navigationService =
                    new SingleViewNavigationService(singleViewPlatform, mainViewWrapper, serviceProvider);
                RegisterViews(navigationService);
                viewModel = navigationService.GetViewModel<StartNewGameViewModel>();
                mainViewWrapper.Content = IsMobile()
                    ? new StartNewGameViewNarrow
                    {
                        ViewModel = viewModel
                    }
                    : new StartNewGameViewWide
                    {

                        ViewModel = viewModel
                    };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterViews(INavigationService navigationService)
    {
        if (IsMobile())
        {
            navigationService.RegisterViews(typeof(StartNewGameViewNarrow), typeof(StartNewGameViewModel));
        }
        else
        {
            navigationService.RegisterViews(typeof(StartNewGameViewWide), typeof(StartNewGameViewModel));
        }
        
        navigationService.RegisterViews(typeof(BattleMapView), typeof(BattleMapViewModel));
    }
    private bool IsMobile()
    {
        return OperatingSystem.IsIOS() || OperatingSystem.IsAndroid();
    }
}