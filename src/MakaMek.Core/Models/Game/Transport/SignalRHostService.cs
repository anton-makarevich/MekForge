using Sanet.Transport;
using Sanet.Transport.SignalR.Infrastructure;

namespace Sanet.MakaMek.Core.Models.Game.Transport;

/// <summary>
/// Service that manages a SignalR host for LAN multiplayer
/// </summary>
public class SignalRHostService : IDisposable
{
    private SignalRHostManager? _hostManager;
    private bool _isDisposed;
    
    /// <summary>
    /// Gets the transport publisher associated with this host
    /// </summary>
    public ITransportPublisher Publisher => _hostManager?.Publisher ?? 
        throw new InvalidOperationException("Host has not been started");
    
    /// <summary>
    /// Gets the server IP address and port for clients to connect to
    /// </summary>
    public string? ServerIpAddress { get; private set; }
    
    /// <summary>
    /// Starts the SignalR host on the specified port
    /// </summary>
    /// <param name="port">Port to host the SignalR hub on</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task Start(int port = 2439)
    {
        if (_hostManager != null)
            return; // Already started
            
        _hostManager = new SignalRHostManager(port);
        await _hostManager.Start();
        
        // Extract the IP address from the hub URL
        var hubUrl = _hostManager.HubUrl;
        var uri = new Uri(hubUrl);
        ServerIpAddress = $"{uri.Host}:{uri.Port}";
    }
    
    /// <summary>
    /// Gets a value indicating whether the host is running
    /// </summary>
    public bool IsRunning => _hostManager != null;
    
    /// <summary>
    /// Disposes the host service and stops the SignalR host
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        _hostManager?.Dispose();
        _hostManager = null;
    }
}
