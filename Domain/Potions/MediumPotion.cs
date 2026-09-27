namespace PyramidTreasureConsoleRPG;

public sealed class MediumPotion : Potion
{
    public MediumPotion()
        : base(PotionKind.Medium, "Średnia mikstura lecząca", price: 50, restoreHp: 150)
    {
    }
}
