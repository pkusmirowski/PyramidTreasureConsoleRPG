namespace PyramidTreasureConsoleRPG.Ui;

public sealed class BarScreen(IGameIO io, IRandomSource rng, QuestGiverScreen questGiver, GameSettings settings)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly GameSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly QuestGiverScreen questGiver = questGiver ?? throw new ArgumentNullException(nameof(questGiver));

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            bool news = hero.CurrentRegion == RegionId.Port && QuestEngine.HasNews(hero, QuestGiverId.Barman);
            int choice = io.Menu(
                "Przy barze:",
                news ? "Zapytaj barmana, co słychać w okolicy (ma wieści!)" : "Zapytaj barmana, co słychać w okolicy",
                "Napij się czegoś",
                "Pokaż mi swoje towary",
                "Odejdź od baru");
            io.Clear();
            switch (choice)
            {
                case 1:
                    TalkToBarman(hero);
                    break;
                case 2:
                    DrinkMenu(hero);
                    break;
                case 3:
                    ShowGoods(hero);
                    break;
                default:
                    return;
            }
        }
    }

    private void TalkToBarman(Hero hero)
    {
        if (hero.CurrentRegion == RegionId.Port && QuestEngine.HasNews(hero, QuestGiverId.Barman))
        {
            questGiver.Run(hero, QuestGiverId.Barman);
            return;
        }

        io.ShowInfo(Dialogues.BarmanSmallTalk(hero, rng, settings.ProfanityEnabled));
    }

    private void ShowGoods(Hero hero)
    {
        io.ShowInfo($"Woda źródlana – {BarService.WaterCost} g – leczy 10% zdrowia.");
        io.ShowInfo(BarService.WhiskyAvailable(hero)
            ? $"Szkocka whisky – {BarService.WhiskyCost} g – leczy 25% zdrowia."
            : "Szkocka whisky – jeszcze nie dopłynęła.");
        if (hero.SpecialDrinkUsed)
        {
            io.ShowInfo("Miód \"Grunwald\" – wypity. \"To była jedyna butelka, niestety.\"");
        }
        else if (BarService.SpecialDrinkAvailable(hero))
        {
            io.ShowInfo($"Miód \"Grunwald\" z Malborka – {BarService.SpecialDrinkCost} g – jedna butelka, tylko dla weteranów (poziom {BarService.MinLevelForSpecialDrink}+). Wzmacnia na stałe.");
        }
        else
        {
            io.ShowInfo("Coś specjalnego – barman jeszcze o tym nie wspominał.");
        }
    }

    private void DrinkMenu(Hero hero)
    {
        while (true)
        {
            io.WriteLine($"Zdrowie: {hero.Hp}/{hero.MaxHp}   Złoto: {hero.Gold}", ConsoleColor.DarkYellow);
            int choice = io.Menu(
                "Co podać?",
                $"Szklanka wody źródlanej / {BarService.WaterCost} g",
                $"Szklanka szkockiej whisky / {BarService.WhiskyCost} g",
                $"Coś specjalnego / {BarService.SpecialDrinkCost} g",
                "Zrezygnuj");
            io.Clear();
            switch (choice)
            {
                case 1:
                    Report(hero, BarService.DrinkWater(hero), "Napiłeś się wody ze źródła. Czujesz, jak ciało odzyskuje siły.", "Masz pełne zdrowie. Szkoda pieniędzy na wodę.");
                    break;
                case 2:
                    Report(hero, BarService.DrinkWhisky(hero), "Whisky pali w gardle, ale ból mija.", "Masz pełne zdrowie, ale whisky i tak wchodzi. Barman patrzy z uznaniem.");
                    break;
                case 3:
                    DrinkSpecial(hero);
                    break;
                default:
                    return;
            }
        }
    }

    private void Report(Hero hero, DrinkResult result, string okText, string fullText)
    {
        switch (result.Outcome)
        {
            case DrinkOutcome.Ok:
                io.ShowSuccess($"{okText} Masz {hero.Hp}/{hero.MaxHp} HP.");
                break;
            case DrinkOutcome.FullHealth:
                io.ShowInfo(fullText);
                break;
            case DrinkOutcome.NotEnoughGold:
                io.ShowError(Dialogues.NoGold(rng, settings.ProfanityEnabled));
                break;
            case DrinkOutcome.NotAvailable:
                io.ShowError("\"Whisky? Statek jeszcze nie przypłynął. Na razie tylko woda.\"");
                break;
            default:
                io.ShowError("Barman kręci głową.");
                break;
        }
    }

    private void DrinkSpecial(Hero hero)
    {
        switch (BarService.CanDrinkSpecial(hero))
        {
            case DrinkOutcome.AlreadyUsed:
                io.ShowInfo("\"To była jedyna butelka, niestety. Nie wiem, czy kiedykolwiek dostanę podobny towar.\"");
                return;
            case DrinkOutcome.NotAvailable:
                io.ShowInfo("\"Coś specjalnego? Nie mam pojęcia, o czym mówisz.\" Barman odwraca wzrok.");
                return;
            case DrinkOutcome.LevelTooLow:
                io.ShowError($"\"Jesteś zbyt słaby, by to przeżyć. Wróć na {BarService.MinLevelForSpecialDrink}. poziomie.\"");
                return;
            case DrinkOutcome.NotEnoughGold:
                io.ShowError(Dialogues.NoGold(rng, settings.ProfanityEnabled));
                return;
        }

        io.Narrate(Dialogues.SpecialDrink);
        int statChoice = io.Menu("Którą statystykę wzmocnić?", "Siła", "Zręczność", "Żywotność");
        SpecialDrinkResult result = BarService.DrinkSpecial(hero, (StatKind)statChoice);
        io.ShowSuccess($"Wybrana statystyka wzrosła o {result.StatIncrease}. Zyskałeś {result.ExpGained} punktów doświadczenia"
            + (result.LevelsGained > 0 ? $" i {result.LevelsGained} poziom(y)! Masz teraz poziom {hero.Level}." : "."));
    }
}
