using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Models.Game.Players;

public interface IPlayer
{
    Guid Id { get; }
    string Name { get; }
    IReadOnlyList<Unit> Units { get; }
    
    PlayerStatus Status { get; set; }
    
    string Tint { get; }
}