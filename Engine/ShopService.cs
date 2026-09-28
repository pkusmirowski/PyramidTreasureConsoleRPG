namespace PyramidTreasureConsoleRPG.Engine;

public enum PurchaseOutcome
{
    Bought,
    NotEnoughGold,
    BagFull,
}

public sealed record PurchaseResult(PurchaseOutcome Outcome, string Name, int Price);

/// <summary>Sklepy: mikstury wszędzie, wyposażenie zależne od regionu, ceny zależne od reputacji i amuletu.</summary>
public static class ShopService
{
    /// <summary>Mikstury lecznicze wszędzie; odtrutka w porcie i oazie; lotos i whisky w Starym Mieście i oazie.</summary>
    public static IReadOnlyList<Potion> Potions(RegionDefinition region)
    {
        ArgumentNullException.ThrowIfNull(region);
        var list = Potion.Healing.ToList();
        if (region.Id is RegionId.Port or RegionId.Oasis)
        {
            list.Add(Potion.Antidote);
        }

        if (region.Id is RegionId.OldTown or RegionId.Oasis)
        {
            list.Add(Potion.Whisky);
            list.Add(Potion.Lotus);
        }

        return list;
    }

    public static IReadOnlyList<Item> Stock(RegionDefinition region)
    {
        ArgumentNullException.ThrowIfNull(region);
        return region.Stock.Select(ItemCatalog.Get).ToList();
    }

    /// <summary>Mnożnik cen w procentach: reputacja (Miasto, a w Starym Mieście Podziemie) i zniżka z amuletu.</summary>
    public static int PricePercent(Hero hero, RegionDefinition region)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(region);
        Faction faction = region.Id == RegionId.OldTown ? Faction.Underworld : Faction.Town;
        int reputation = hero.GetReputation(faction);
        int percent = reputation switch
        {
            >= 30 => 90,
            <= -30 => 125,
            _ => 100,
        };
        return Math.Max(50, percent - hero.ShopDiscountPercent);
    }

    public static int Price(Hero hero, RegionDefinition region, int basePrice) => Math.Max(1, basePrice * PricePercent(hero, region) / 100);

    public static PurchaseResult BuyPotion(Hero hero, RegionDefinition region, PotionKind kind)
    {
        ArgumentNullException.ThrowIfNull(hero);
        Potion potion = Potion.Create(kind);
        int price = Price(hero, region, potion.Price);
        if (hero.Gold < price)
        {
            return new PurchaseResult(PurchaseOutcome.NotEnoughGold, potion.Name, price);
        }

        hero.Gold -= price;
        hero.AddPotion(potion);
        return new PurchaseResult(PurchaseOutcome.Bought, potion.Name, price);
    }

    public static PurchaseResult BuyItem(Hero hero, RegionDefinition region, Item item)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(item);
        int price = Price(hero, region, item.Price);
        if (hero.Gold < price)
        {
            return new PurchaseResult(PurchaseOutcome.NotEnoughGold, item.Name, price);
        }

        if (!hero.AddGear(item))
        {
            return new PurchaseResult(PurchaseOutcome.BagFull, item.Name, price);
        }

        hero.Gold -= price;
        return new PurchaseResult(PurchaseOutcome.Bought, item.Name, price);
    }

    /// <summary>Sprzedaje przedmiot z torby za cenę skupu. Zwraca zysk albo 0, gdy przedmiotu nie było.</summary>
    public static int Sell(Hero hero, Item item)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(item);
        if (!hero.RemoveGear(item))
        {
            return 0;
        }

        hero.Gold += item.SellPrice;
        return item.SellPrice;
    }
}
