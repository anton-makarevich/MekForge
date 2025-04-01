using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Sanet.MakaMek.Avalonia.Android.DependencyInjection;
using Sanet.MVVM.DI.Avalonia.Extensions;

namespace Sanet.MakaMek.Avalonia.Android;

[Activity(
    Label = "MakaMek.Avalonia.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .UseDependencyInjection(services=>services.RegisterAndroidServices())
            .WithInterFont();
    }
}