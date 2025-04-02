using Sanet.MakaMek.Core.Models.Map;

namespace Sanet.MakaMek.Core.Models.Game;

public interface IGameManager : IDisposable
{ 
    /// <summary>
    /// Starts a game server
    /// </summary>
    /// <param name="battleMap">The battle map to use</param>
    /// <param name="enableLan">Whether to enable LAN access</param>
    void StartServer(BattleMap battleMap, bool enableLan = false);
    
    /// <summary>
    /// Gets the LAN server address for clients to connect to
    /// </summary>
    /// <returns>The server IP address</returns>
    string? GetLanServerAddress();
    
    /// <summary>
    /// Gets a value indicating whether the LAN server is running
    /// </summary>
    bool IsLanServerRunning { get; }
}