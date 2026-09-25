namespace PyramidTreasureConsoleRPG;

/// <summary>Bar: rozmowa z barmanem (fabuła) i napoje. Napoje leczą, nie zmieniają statystyk na stałe.</summary>
public static class Bar
{
    public const int WaterCost = 5;
    public const int WhiskyCost = 15;
    public const int SpecialDrinkCost = 150;
    public const int SpecialDrinkExpGain = 5000;
    public const int SpecialDrinkStatIncrease = 5;
    public const int MinLevelForSpecialDrink = 15;

    public static void Visit(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            GameIO.Menu(
                "Przy barze:",
                "Zapytaj barmana, co słychać w okolicy",
                "Napij się czegoś",
                "Pokaż mi swoje towary",
                "Odejdź od baru");
            int choice = GameIO.ReadMenuChoice(4);
            GameIO.Clear();
            switch (choice)
            {
                case 1:
                    Story.TalkToBarman(hero);
                    break;
                case 2:
                    DrinkMenu(hero);
                    break;
                case 3:
                    ShowGoods(hero);
                    break;
                case 4:
                    return;
            }
        }
    }

    public static bool WhiskyAvailable(Hero hero) => hero.Stage >= StoryStage.WolvesCleared;

    public static bool SpecialDrinkAvailable(Hero hero) => hero.Stage >= StoryStage.CaravanAnnounced && !hero.SpecialDrinkUsed;

    private static void ShowGoods(Hero hero)
    {
        GameIO.Info($"Woda źródlana – {WaterCost} g – leczy 10% zdrowia.");
        GameIO.Info(WhiskyAvailable(hero)
            ? $"Szkocka whisky – {WhiskyCost} g – leczy 25% zdrowia."
            : "Szkocka whisky – jeszcze nie dopłynęła.");
        if (hero.SpecialDrinkUsed)
        {
            GameIO.Info("Miód \"Grunwald\" – wypity. \"To była jedyna butelka, niestety.\"");
        }
        else if (SpecialDrinkAvailable(hero))
        {
            GameIO.Info($"Miód \"Grunwald\" z Malborka – {SpecialDrinkCost} g – jedna butelka, tylko dla weteranów (poziom {MinLevelForSpecialDrink}+). Wzmacnia na stałe.");
        }
        else
        {
            GameIO.Info("Coś specjalnego – barman jeszcze o tym nie wspominał.");
        }
    }

    private static void DrinkMenu(Hero hero)
    {
        while (true)
        {
            GameIO.WriteLine($"Zdrowie: {hero.Hp}/{hero.MaxHp}   Złoto: {hero.Gold}", ConsoleColor.DarkYellow);
            GameIO.Menu(
                "Co podać?",
                $"Szklanka wody źródlanej / {WaterCost} g",
                $"Szklanka szkockiej whisky / {WhiskyCost} g",
                $"Coś specjalnego / {SpecialDrinkCost} g",
                "Zrezygnuj");
            int choice = GameIO.ReadMenuChoice(4);
            GameIO.Clear();
            switch (choice)
            {
                case 1:
                    DrinkWater(hero);
                    break;
                case 2:
                    DrinkWhisky(hero);
                    break;
                case 3:
                    DrinkSpecial(hero);
                    break;
                case 4:
                    return;
            }
        }
    }

    private static bool Pay(Hero hero, int cost)
    {
        if (hero.Gold < cost)
        {
            Dialogues.NoGold();
            return false;
        }

        hero.Gold -= cost;
        return true;
    }

    private static void DrinkWater(Hero hero)
    {
        if (hero.Hp >= hero.MaxHp)
        {
            GameIO.Info("Masz pełne zdrowie. Szkoda pieniędzy na wodę.");
            return;
        }

        if (!Pay(hero, WaterCost))
        {
            return;
        }

        hero.Heal(Math.Max(10, hero.MaxHp / 10));
        GameIO.Success($"Napiłeś się wody ze źródła. Czujesz, jak ciało odzyskuje siły. Masz {hero.Hp}/{hero.MaxHp} HP.");
    }

    private static void DrinkWhisky(Hero hero)
    {
        if (!WhiskyAvailable(hero))
        {
            GameIO.Error("\"Whisky? Statek jeszcze nie przypłynął. Na razie tylko woda.\"");
            return;
        }

        if (hero.Hp >= hero.MaxHp)
        {
            GameIO.Info("Masz pełne zdrowie, ale whisky i tak wchodzi. Barman patrzy z uznaniem.");
        }

        if (!Pay(hero, WhiskyCost))
        {
            return;
        }

        hero.Heal(Math.Max(20, hero.MaxHp / 4));
        GameIO.Success($"Whisky pali w gardle, ale ból mija. Masz {hero.Hp}/{hero.MaxHp} HP.");
    }

    private static void DrinkSpecial(Hero hero)
    {
        if (hero.SpecialDrinkUsed)
        {
            GameIO.Info("\"To była jedyna butelka, niestety. Nie wiem, czy kiedykolwiek dostanę podobny towar.\"");
            return;
        }

        if (!SpecialDrinkAvailable(hero))
        {
            GameIO.Info("\"Coś specjalnego? Nie mam pojęcia, o czym mówisz.\" Barman odwraca wzrok.");
            return;
        }

        if (hero.Level < MinLevelForSpecialDrink)
        {
            GameIO.Error($"\"Jesteś zbyt słaby, by to przeżyć. Wróć na {MinLevelForSpecialDrink}. poziomie.\"");
            return;
        }

        if (!Pay(hero, SpecialDrinkCost))
        {
            return;
        }

        GameIO.Narrate(
            "\"Specjalność prosto od krzyżaków z Malborka. Miód pitny zwany Grunwald!\"",
            "Pijesz legendarny miód, który podobno stał na stołach biesiadnych przed bitwą pod Grunwaldem.",
            "Świat wiruje. Kiedy dochodzisz do siebie, czujesz się silniejszy niż kiedykolwiek.");
        hero.SpecialDrinkUsed = true;

        GameIO.Menu("Którą statystykę wzmocnić?", "Siła", "Zręczność", "Żywotność");
        var stat = (StatKind)GameIO.ReadMenuChoice(3);
        hero.IncreaseStat(stat, SpecialDrinkStatIncrease);
        int levels = hero.AddExp(SpecialDrinkExpGain);
        GameIO.Success($"Wybrana statystyka wzrosła o {SpecialDrinkStatIncrease}. Zyskałeś {SpecialDrinkExpGain} punktów doświadczenia" + (levels > 0 ? $" i {levels} poziom(y)! Masz teraz poziom {hero.Level}." : "."));
    }
}
