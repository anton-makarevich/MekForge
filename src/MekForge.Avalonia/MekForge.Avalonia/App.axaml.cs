using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MekForge.Avalonia.ViewModels;
using Sanet.MekForge.Avalonia.Views;
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

        services.AddTransient<BattleMapViewModel>();

        var serviceProvider = services.BuildServiceProvider();
        INavigationService navigationService;

        BattleMapViewModel? viewModel;
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                navigationService = new NavigationService(desktop, serviceProvider);
                RegisterViews(navigationService);
                viewModel = navigationService.GetViewModel<BattleMapViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    Content = new BattleMapView
                    {
                        ViewModel = viewModel
                    }
                };
                break;
            }
            case ISingleViewApplicationLifetime singleViewPlatform:
                var mainViewWrapper = new ContentControl();
                navigationService = new SingleViewNavigationService(singleViewPlatform, mainViewWrapper, serviceProvider);
                RegisterViews(navigationService);
                viewModel = navigationService.GetViewModel<BattleMapViewModel>();
                mainViewWrapper.Content = new BattleMapView
                {
                    ViewModel = viewModel
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterViews(INavigationService navigationService)
    {
        navigationService.RegisterViews(typeof(BattleMapView), typeof(BattleMapViewModel));
    }
}