using Microsoft.Extensions.DependencyInjection;
using Sanet.MakaMek.Core.Services.Transport;

namespace Sanet.MakaMek.Avalonia.iOS.DependencyInjection;

public static class IosServices
{
    public static void RegisterIosServices(this IServiceCollection services)
    {
        // Register the dummy network host service for iOS
        services.AddSingleton<INetworkHostService, DummyNetworkHostService>();
    }
}
