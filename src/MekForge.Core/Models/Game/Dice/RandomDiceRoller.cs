namespace Sanet.MekForge.Core.Models.Game.Dice;

public class RandomDiceRoller : IDiceRoller
{
    private readonly Random _random = new();

    public DiceResult RollD6()
    {
        var result = new DiceResult { Result = _random.Next(1, 7) };
        return result;
    }

    public List<DiceResult> Roll2D6()
    {
        return [RollD6(), RollD6()];
    }
}