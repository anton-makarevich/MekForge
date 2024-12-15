using Sanet.MekForge.Core.Models;

namespace Sanet.MekForge.Core.Game;

public interface IGameManager
{ 
    void StartServer(BattleState battleState);
}