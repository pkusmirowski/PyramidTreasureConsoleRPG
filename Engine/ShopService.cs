namespace PyramidTreasureConsoleRPG.Engine;

public enum PurchaseOutcome
{
    Bought,
    NotEnoughGold,
}

public sealed record PurchaseResult(PurchaseOutcome Outcome, Potion Potion);

public static class ShopService
{
    public static IReadOnlyList<Potion> Offers { get; } = Potion.AllKinds.Select(Potion.Create).ToList();

    public static PurchaseResult Buy(Hero hero, PotionKind kind)
    {
        ArgumentNullException.ThrowIfNull(hero);
        Potion potion = Potion.Create(kind);
        if (hero.Gold < potion.Price)
        {
            return new PurchaseResult(PurchaseOutcome.NotEnoughGold, potion);
        }

        hero.Gold -= potion.Price;
        hero.AddPotion(potion);
        return new PurchaseResult(PurchaseOutcome.Bought, potion);
    }
}
