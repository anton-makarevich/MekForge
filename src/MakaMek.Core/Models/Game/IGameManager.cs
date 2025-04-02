using Sanet.MakaMek.Core.Models.Map;

namespace Sanet.MakaMek.Core.Models.Game;

public interface IGameManager : IDisposable
{ 
    /// <summary>
    /// Starts a local game server
    /// </summary>
    /// <param name="battleMap">The battle map to use</param>
    void StartServer(BattleMap battleMap);
    
    /// <summary>
    /// Starts a LAN game server that remote clients can connect to
    /// </summary>
    /// <param name="battleMap">The battle map to use</param>
    /// <returns>The server IP address for clients to connect to</returns>
    Task<string?> StartLanServer(BattleMap battleMap);
    
    /// <summary>
    /// Gets a value indicating whether the LAN server is running
    /// </summary>
    bool IsLanServerRunning { get; }
}