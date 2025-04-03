using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.Core.Models.Game.Transport;

namespace Sanet.MakaMek.Avalonia.Android.DependencyInjection;

public static class AndroidServices
{
    public static void RegisterAndroidServices(this IServiceCollection services)
    {
        // Register the dummy network host service for Android
        services.AddSingleton<INetworkHostService, DummyNetworkHostService>();
    }
}
