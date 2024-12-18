using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Models.Game;

public interface IGameManager
{ 
    void StartServer(BattleMap battleMap);
}