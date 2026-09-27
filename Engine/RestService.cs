namespace PyramidTreasureConsoleRPG.Engine;

public enum RestOutcome
{
    Ok,
    FullHealth,
    NotEnoughGold,
}

public enum NightEvent
{
    Nothing,
    Rumor,
    Robbed,
    Gift,
}

public sealed record NightResult(NightEvent Event, int Amount, int LevelsGained, Potion? Gift);

/// <summary>Pokoje na górze: nocleg i noc w towarzystwie z losowymi konsekwencjami.</summary>
public sealed class RestService(IRandomSource rng)
{
    public const int RoomCost = 10;
    public const int CompanyCost = 30;

    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));

    public static RestOutcome RentRoom(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.Hp >= hero.MaxHp)
        {
            return RestOutcome.FullHealth;
        }

        if (hero.Gold < RoomCost)
        {
            return RestOutcome.NotEnoughGold;
        }

        hero.Gold -= RoomCost;
        hero.FullHeal();
        return RestOutcome.Ok;
    }

    /// <summary>Zwraca null, gdy bohatera nie stać.</summary>
    public NightResult? SpendNight(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.Gold < CompanyCost)
        {
            return null;
        }

        hero.Gold -= CompanyCost;
        hero.FullHeal();

        int roll = rng.Range(1, 100);
        if (roll <= 40)
        {
            return new NightResult(NightEvent.Nothing, 0, 0, null);
        }

        if (roll <= 70)
        {
            int exp = Math.Max(50, hero.ExpToNextLevel / 20);
            int levels = hero.AddExp(exp);
            return new NightResult(NightEvent.Rumor, exp, levels, null);
        }

        if (roll <= 90)
        {
            int stolen = hero.Gold / 10;
            hero.Gold -= stolen;
            return new NightResult(NightEvent.Robbed, stolen, 0, null);
        }

        Potion potion = Potion.Medium;
        hero.AddPotion(potion);
        return new NightResult(NightEvent.Gift, 0, 0, potion);
    }
}
