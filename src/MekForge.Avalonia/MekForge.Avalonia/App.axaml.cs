using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MekForge.Avalonia.DI;
using Sanet.MekForge.Avalonia.Views;
using Sanet.MekForge.Avalonia.Views.NewGame;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MVVM.Core.Services;
using Sanet.MVVM.Navigation.Avalonia.Services;
using MainWindow = Sanet.MekForge.Avalonia.Views.MainWindow;

namespace Sanet.MekForge.Avalonia;

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

        NewGameViewModel? viewModel;
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                navigationService = new NavigationService(desktop, serviceProvider);
                RegisterViews(navigationService);
                viewModel = navigationService.GetViewModel<NewGameViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    Content = IsMobile()
                    ? new NewGameViewNarrow()
                    {
                        ViewModel = viewModel
                    }
                    : new NewGameViewWide
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
                viewModel = navigationService.GetViewModel<NewGameViewModel>();
                mainViewWrapper.Content = IsMobile()
                    ? new NewGameViewNarrow
                    {
                        ViewModel = viewModel
                    }
                    : new NewGameViewWide
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
            navigationService.RegisterViews(typeof(NewGameViewNarrow), typeof(NewGameViewModel));
        }
        else
        {
            navigationService.RegisterViews(typeof(NewGameViewWide), typeof(NewGameViewModel));
        }
        
        navigationService.RegisterViews(typeof(BattleMapView), typeof(BattleMapViewModel));
    }
    private bool IsMobile()
    {
        return OperatingSystem.IsIOS() || OperatingSystem.IsAndroid();
    }
}