namespace Sanet.MekForge.Core.Models.Game.Dice;

public class DiceResult
{
    private int _result;

    public int Result
    {
        get => _result;
        set
        {
            if (value is < 1 or > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Result must be between 1 and 6.");
            }
            _result = value;
        }
    }
}