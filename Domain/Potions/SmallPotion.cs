namespace PyramidTreasureConsoleRPG.Domain;

public sealed class SmallPotion : Potion
{
    public SmallPotion()
        : base(PotionKind.Small, "Mała mikstura lecząca", price: 20, restoreHp: 50)
    {
    }
}
