using Sanet.MakaMek.Core.Models.Units;

namespace Sanet.MakaMek.Core.Models.Game.Players;

public class Player : IPlayer
{
    private readonly List<Unit> _units = [];
    
    public Guid Id { get; }
    public string Name { get; }
    public string Tint { get; }
    public IReadOnlyList<Unit> Units => _units;
    public PlayerStatus Status { get; set; } = PlayerStatus.Joining;

    public Player(Guid id, string name, string tint = "#ffffff")
    {
        Id = id;
        Name = name;
        Tint = tint;
    }
    
    public void AddUnit(Unit unit)
    {
        _units.Add(unit);
        unit.Owner = this;
    }
}