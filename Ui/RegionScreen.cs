namespace PyramidTreasureConsoleRPG.Ui;

public enum SessionEnd
{
    ReturnedToMenu,
    Died,
    Completed,
}

/// <summary>Ekran bieżącego regionu: eksploracja, mapa, usługi regionu, zadania, zapis. Dawniej „miasto”.</summary>
public sealed class RegionScreen(
    IGameIO io,
    IRandomSource rng,
    ISaveStore saves,
    CombatScreen combat,
    TavernScreen tavern,
    ShopScreen shop,
    InventoryScreen inventory,
    EventScreen events,
    QuestGiverScreen questGiver,
    QuestLogScreen questLog,
    MapScreen map,
    TalentScreen talents)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly ISaveStore saves = saves ?? throw new ArgumentNullException(nameof(saves));
    private readonly CombatScreen combat = combat ?? throw new ArgumentNullException(nameof(combat));
    private readonly TavernScreen tavern = tavern ?? throw new ArgumentNullException(nameof(tavern));
    private readonly ShopScreen shop = shop ?? throw new ArgumentNullException(nameof(shop));
    private readonly InventoryScreen inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
    private readonly EventScreen events = events ?? throw new ArgumentNullException(nameof(events));
    private readonly QuestGiverScreen questGiver = questGiver ?? throw new ArgumentNullException(nameof(questGiver));
    private readonly QuestLogScreen questLog = questLog ?? throw new ArgumentNullException(nameof(questLog));
    private readonly MapScreen map = map ?? throw new ArgumentNullException(nameof(map));
    private readonly TalentScreen talents = talents ?? throw new ArgumentNullException(nameof(talents));

    public SessionEnd Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            RegionDefinition region = RegionCatalog.Get(hero.CurrentRegion);
            io.Header($"{region.Name}, dzień {hero.Day} – {hero.Name}, {hero.ClassName} {hero.Level} lvl, {hero.Hp}/{hero.MaxHp} HP, {hero.Gold} złota");
            var options = BuildMenu(hero, region);
            int choice = io.Menu("Co chcesz zrobić?", options.Select(o => o.Label).ToArray());
            io.Clear();
            SessionEnd? end = options[choice - 1].Action();
            if (end.HasValue)
            {
                return end.Value;
            }
        }
    }

    private List<(string Label, Func<SessionEnd?> Action)> BuildMenu(Hero hero, RegionDefinition region)
    {
        var options = new List<(string, Func<SessionEnd?>)>();
        if (region.IsFinal)
        {
            options.Add((region.ExploreLabel, () => EnterPyramid(hero)));
        }
        else
        {
            options.Add((region.ExploreLabel, () => Explore(hero, region)));
        }

        if (hero.PendingTalentLevels().Count > 0)
        {
            options.Add(("Wybierz talent (nowy!)", Wrap(() => talents.Run(hero))));
        }

        options.Add(("Mapa – podróż", () => map.Run(hero)));
        if (region.HasTavern)
        {
            bool news = region.IsHome && QuestEngine.HasNews(hero, QuestGiverId.Barman);
            options.Add((news ? "Tawerna (barman ma wieści!)" : "Tawerna", () => tavern.Run(hero) == CombatStatus.Defeat ? Defeat() : null));
        }

        if (region.HasShop)
        {
            options.Add(("Sklep", Wrap(() => shop.Run(hero))));
        }

        foreach (QuestGiverId giver in region.QuestGivers.Where(g => g != QuestGiverId.Barman))
        {
            string place = GiverPlace(giver);
            bool news = QuestEngine.HasNews(hero, giver);
            options.Add((news ? $"{place} (zlecenie!)" : place, Wrap(() => VisitGiver(hero, giver))));
        }

        options.Add(("Dziennik zadań", Wrap(() => questLog.Run(hero))));
        options.Add(("Statystyki bohatera", Wrap(() => io.ShowStats(hero))));
        options.Add(("Sakwa", Wrap(() => inventory.Run(hero))));
        options.Add(("Zapisz grę", Wrap(() => SaveGame(hero))));
        options.Add(("Wróć do menu głównego", () => ConfirmLeave() ? SessionEnd.ReturnedToMenu : null));
        return options;
    }

    private static Func<SessionEnd?> Wrap(Action action) => () =>
    {
        action();
        return null;
    };

    private void VisitGiver(Hero hero, QuestGiverId giver)
    {
        if (giver == QuestGiverId.Priestess)
        {
            hero.SetFlag("priestess:met");
        }

        questGiver.Run(hero, giver);
    }

    private static string GiverPlace(QuestGiverId giver) => giver switch
    {
        QuestGiverId.Captain => "Kapitanat portu",
        QuestGiverId.Smuggler => "Melina Hasana",
        QuestGiverId.Priestess => "Świątynia Bractwa",
        _ => RegionCatalog.GiverName(giver),
    };

    private SessionEnd? Explore(Hero hero, RegionDefinition region)
    {
        if (hero.Hp < hero.MaxHp / 3)
        {
            io.ShowInfo($"Masz tylko {hero.Hp}/{hero.MaxHp} HP. Na pewno chcesz wyruszyć?");
            if (io.Menu("Wyruszyć?", "Tak", "Nie, najpierw się wyleczę") == 2)
            {
                io.Clear();
                return null;
            }
        }

        TravelStep step = TravelEngine.Explore(hero, region, rng, out IReadOnlyList<string> dayNotes);
        foreach (string note in dayNotes)
        {
            io.WriteLine(note, ConsoleColor.Magenta);
        }

        return RunStep(hero, step);
    }

    /// <summary>Wykonuje krok podróży lub eksploracji. Zwraca koniec sesji tylko przy śmierci bohatera.</summary>
    public SessionEnd? RunStep(Hero hero, TravelStep step)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(step);
        switch (step.Kind)
        {
            case TravelStepKind.Fight:
                io.ShowInfo(step.Text);
                return AfterFight(hero, combat.Run(hero, step.Enemies!), step.Enemies!);
            case TravelStepKind.Event:
                return events.Run(hero, step.Event!) == CombatStatus.Defeat ? Defeat() : null;
            default:
                io.ShowInfo(step.Text);
                return null;
        }
    }

    /// <summary>Po zwycięstwie nad ludźmi czasem jeden z nich żyje: egzekucja, łaska albo przesłuchanie.</summary>
    private SessionEnd? AfterFight(Hero hero, CombatStatus status, IEnumerable<Enemy> enemies)
    {
        if (status == CombatStatus.Defeat)
        {
            return Defeat();
        }

        if (status != CombatStatus.Victory)
        {
            return null;
        }

        var killed = enemies.Where(e => !e.IsAlive).Select(e => e.Definition).ToList();
        if (!InterrogationService.PrisonerSurvives(killed, rng))
        {
            return null;
        }

        io.WriteLine("Jeden z nich jeszcze dyszy. Krew bulgocze mu w gardle, ale oczy są przytomne.", ConsoleColor.White);
        int choice = io.Menu("Co robisz z jeńcem?", "Dobij go (Podziemie +3, Miasto −3)", "Puść wolno (Miasto +5)", "Przesłuchaj (test siły; Miasto −8, Podziemie +5, koszmary; sukces = informacje)");
        io.Clear();
        PrisonerResult result = InterrogationService.Resolve(hero, (PrisonerChoice)choice, hero.CurrentRegion, rng);
        io.Narrate([result.Text], ConsoleColor.DarkRed);
        foreach (string note in result.Notes)
        {
            io.WriteLine(note, ConsoleColor.Cyan);
        }

        io.PressAnyKey();
        return null;
    }

    private SessionEnd? EnterPyramid(Hero hero)
    {
        io.ShowInfo("Za wejściem nie będzie odwrotu.");
        if (io.Menu("Wejść?", "Tak, do piramidy", "Jeszcze nie") == 2)
        {
            io.Clear();
            return null;
        }

        io.Clear();
        io.Narrate(Dialogues.PyramidHistory);
        if (combat.Run(hero, Encounters.PyramidGuards()) == CombatStatus.Defeat)
        {
            return Defeat();
        }

        io.Narrate(Dialogues.AfterGuards);
        hero.FullHeal();
        if (combat.Run(hero, Encounters.FinalBoss()) == CombatStatus.Defeat)
        {
            return Defeat();
        }

        hero.Completed = true;
        io.Narrate(Dialogues.Ending);
        io.ShowSuccess($"Ukończyłeś grę jako {hero.Name}, {hero.ClassName} na poziomie {hero.Level}, w {hero.Day} dni, z {hero.Gold} sztukami złota!");
        io.PressAnyKey();
        return SessionEnd.Completed;
    }

    public SessionEnd Defeat()
    {
        io.ShowError("\nZostałeś pokonany. Twoja wyprawa kończy się w piachu, a Graal pozostaje legendą.");
        io.ShowInfo("Możesz wczytać ostatni zapis z menu głównego.");
        io.PressAnyKey();
        return SessionEnd.Died;
    }

    private void SaveGame(Hero hero)
    {
        if (saves.Exists() && io.Menu("Nadpisać istniejący zapis?", "Nie", "Tak") == 1)
        {
            io.Clear();
            return;
        }

        io.Clear();
        if (saves.Save(hero.ToSaveData(), out string message))
        {
            io.ShowSuccess(message);
        }
        else
        {
            io.ShowError(message);
        }
    }

    private bool ConfirmLeave()
    {
        bool leave = io.Menu("Niezapisany postęp zostanie utracony. Wrócić do menu?", "Nie", "Tak") == 2;
        io.Clear();
        return leave;
    }
}
