namespace PyramidTreasureConsoleRPG.Engine;

public enum DrinkOutcome
{
    Ok,
    FullHealth,
    NotEnoughGold,
    NotAvailable,
    LevelTooLow,
    AlreadyUsed,
}

public sealed record DrinkResult(DrinkOutcome Outcome, int Healed);

public sealed record SpecialDrinkResult(StatKind Stat, int StatIncrease, int ExpGained, int LevelsGained);

/// <summary>Bar: napoje leczą, nie zmieniają statystyk na stałe. Wyjątkiem jest jednorazowy miód.</summary>
public static class BarService
{
    public const int WaterCost = 5;
    public const int WhiskyCost = 15;
    public const int SpecialDrinkCost = 150;
    public const int SpecialDrinkExpGain = 5000;
    public const int SpecialDrinkStatIncrease = 5;
    public const int MinLevelForSpecialDrink = 15;

    public static bool WhiskyAvailable(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return hero.Stage >= StoryStage.WolvesCleared;
    }

    public static bool SpecialDrinkAvailable(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return hero.Stage >= StoryStage.CaravanAnnounced && !hero.SpecialDrinkUsed;
    }

    public static DrinkResult DrinkWater(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.Hp >= hero.MaxHp)
        {
            return new DrinkResult(DrinkOutcome.FullHealth, 0);
        }

        if (hero.Gold < WaterCost)
        {
            return new DrinkResult(DrinkOutcome.NotEnoughGold, 0);
        }

        hero.Gold -= WaterCost;
        return new DrinkResult(DrinkOutcome.Ok, HealBy(hero, Math.Max(10, hero.MaxHp / 10)));
    }

    public static DrinkResult DrinkWhisky(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (!WhiskyAvailable(hero))
        {
            return new DrinkResult(DrinkOutcome.NotAvailable, 0);
        }

        if (hero.Gold < WhiskyCost)
        {
            return new DrinkResult(DrinkOutcome.NotEnoughGold, 0);
        }

        hero.Gold -= WhiskyCost;
        return new DrinkResult(DrinkOutcome.Ok, HealBy(hero, Math.Max(20, hero.MaxHp / 4)));
    }

    /// <summary>Sprawdza, czy bohater może wypić miód (bez pobierania złota).</summary>
    public static DrinkOutcome CanDrinkSpecial(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.SpecialDrinkUsed)
        {
            return DrinkOutcome.AlreadyUsed;
        }

        if (!SpecialDrinkAvailable(hero))
        {
            return DrinkOutcome.NotAvailable;
        }

        if (hero.Level < MinLevelForSpecialDrink)
        {
            return DrinkOutcome.LevelTooLow;
        }

        return hero.Gold < SpecialDrinkCost ? DrinkOutcome.NotEnoughGold : DrinkOutcome.Ok;
    }

    /// <summary>Pobiera złoto, oznacza miód jako wypity, wzmacnia wybraną statystykę i dodaje doświadczenie.</summary>
    public static SpecialDrinkResult DrinkSpecial(Hero hero, StatKind stat)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (CanDrinkSpecial(hero) != DrinkOutcome.Ok)
        {
            throw new InvalidOperationException("Bohater nie może teraz wypić miodu.");
        }

        hero.Gold -= SpecialDrinkCost;
        hero.SpecialDrinkUsed = true;
        hero.IncreaseStat(stat, SpecialDrinkStatIncrease);
        int levels = hero.AddExp(SpecialDrinkExpGain);
        return new SpecialDrinkResult(stat, SpecialDrinkStatIncrease, SpecialDrinkExpGain, levels);
    }

    private static int HealBy(Hero hero, int amount)
    {
        int before = hero.Hp;
        hero.Heal(amount);
        return hero.Hp - before;
    }
}
