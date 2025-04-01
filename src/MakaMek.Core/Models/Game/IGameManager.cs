using Sanet.MakaMek.Core.Models.Map;

namespace Sanet.MakaMek.Core.Models.Game;

public interface IGameManager
{ 
    void StartServer(BattleMap battleMap);
}