using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Models.Game;

public interface IPlayer
{
    Guid Id { get; }
    string Name { get; }
    IReadOnlyList<Unit> Units { get; }
}