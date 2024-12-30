namespace Sanet.MekForge.Core.Models.Game;

public class InitiativeOrder
{
    private readonly List<InitiativeResult> _results = new();

    public void AddResult(IPlayer player, int roll)
    {
        _results.Add(new InitiativeResult(player, roll));
        _results.Sort((a, b) => b.Roll.CompareTo(a.Roll)); // Sort descending
    }

    public void Clear()
    {
        _results.Clear();
    }

    public bool HasPlayer(IPlayer player)
    {
        return _results.Any(r => r.Player == player);
    }

    public bool HasTies()
    {
        if (_results.Count <= 1) return false;
        return _results.GroupBy(r => r.Roll).Any(g => g.Count() > 1);
    }

    public List<IPlayer> GetTiedPlayers()
    {
        if (!HasTies()) return new List<IPlayer>();

        var highestRoll = _results.Max(r => r.Roll);
        return _results
            .Where(r => r.Roll == highestRoll)
            .Select(r => r.Player)
            .ToList();
    }

    public IReadOnlyList<IPlayer> GetOrderedPlayers()
    {
        return _results.Select(r => r.Player).ToList();
    }

    private record  InitiativeResult(IPlayer Player, int Roll);
}
