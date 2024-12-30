namespace Sanet.MekForge.Core.Models.Game.Dice;

public interface IDiceRoller
{
    DiceResult Roll();
    List<DiceResult> Roll2D();
}