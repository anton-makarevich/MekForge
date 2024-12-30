namespace Sanet.MekForge.Core.Models.Game.Dice;

public interface IDiceRoller
{
    DiceResult RollD6();
    List<DiceResult> Roll2D6();
}