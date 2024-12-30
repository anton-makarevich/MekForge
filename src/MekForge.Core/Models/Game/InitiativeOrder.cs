namespace Sanet.MekForge.Core.Models.Game;

public class InitiativeOrder
{
    private readonly List<InitiativeResult> _results = new();
    private int _currentRollNumber = 1;

    public void AddResult(IPlayer player, int roll)
    {
        var existingResult = _results.FirstOrDefault(r => r.Player == player);
        if (existingResult != null)
        {
            existingResult.AddRoll(_currentRollNumber, roll);
        }
        else
        {
            _results.Add(new InitiativeResult(player, _currentRollNumber, roll));
        }
        
        // Sort by each roll number in sequence
        _results.Sort((a, b) =>
        {
            for (var i = 1; i <= _currentRollNumber; i++)
            {
                if (!a.HasRoll(i) || !b.HasRoll(i)) continue;
                var comparison = b.GetRoll(i).CompareTo(a.GetRoll(i));
                if (comparison != 0) return comparison;
            }
            return 0;
        });
    }

    public void StartNewRoll()
    {
        _currentRollNumber++;
    }

    public void Clear()
    {
        _results.Clear();
        _currentRollNumber = 1;
    }

    public bool HasPlayer(IPlayer player)
    {
        return _results.Any(r => r.Player == player);
    }

    public bool HasTies()
    {
        if (_results.Count <= 1) return false;

        // Check only the current roll number for ties
        var playersWithCurrentRoll = _results
            .Where(r => r.HasRoll(_currentRollNumber))
            .ToList();

        if (!playersWithCurrentRoll.Any()) return false;

        return playersWithCurrentRoll
            .GroupBy(r => r.GetRoll(_currentRollNumber))
            .Any(g => g.Count() > 1);
    }

    public List<IPlayer> GetTiedPlayers()
    {
        if (!HasTies()) return new List<IPlayer>();

        var highestCurrentRoll = _results
            .Where(r => r.HasRoll(_currentRollNumber))
            .Max(r => r.GetRoll(_currentRollNumber));

        return _results
            .Where(r => r.HasRoll(_currentRollNumber) && r.GetRoll(_currentRollNumber) == highestCurrentRoll)
            .Select(r => r.Player)
            .ToList();
    }

    public IReadOnlyList<IPlayer> GetOrderedPlayers()
    {
        return _results.Select(r => r.Player).ToList();
    }

    private class InitiativeResult
    {
        private readonly Dictionary<int, int> _rolls = new();
        public IPlayer Player { get; }

        public InitiativeResult(IPlayer player, int rollNumber, int roll)
        {
            Player = player;
            AddRoll(rollNumber, roll);
        }

        public void AddRoll(int rollNumber, int roll)
        {
            _rolls[rollNumber] = roll;
        }

        public bool HasRoll(int rollNumber) => _rolls.ContainsKey(rollNumber);
        
        public int GetRoll(int rollNumber) => _rolls[rollNumber];
    }
}
