namespace PyramidTreasureConsoleRPG.Domain;

public sealed class LargePotion : Potion
{
    public LargePotion()
        : base(PotionKind.Large, "Duża mikstura lecząca", price: 100, restoreHp: 350)
    {
    }
}
