namespace PyramidTreasureConsoleRPG;

public enum SessionEnd
{
    ReturnedToMenu,
    Died,
    Completed,
}

/// <summary>Pętla miasta: podróż, tawerna, sklep, statystyki, sakwa, zapis.</summary>
public sealed class GameSession
{
    private readonly Hero hero;

    public GameSession(Hero hero)
    {
        this.hero = hero ?? throw new ArgumentNullException(nameof(hero));
    }

    public SessionEnd Run()
    {
        while (true)
        {
            bool pyramid = Story.CanEnterPyramid(hero);
            GameIO.Header($"{hero.Name} – {hero.ClassName}, poziom {hero.Level}, {hero.Hp}/{hero.MaxHp} HP, {hero.Gold} złota");
            string travel = pyramid
                ? "Wyrusz z karawaną do piramidy Chufu"
                : Story.BarmanHasNews(hero) ? "Udaj się w drogę (barman ma wieści!)" : "Udaj się w drogę";
            GameIO.Menu("Co chcesz zrobić?", travel, "Idź do tawerny", "Idź do sklepu", "Pokaż statystyki bohatera", "Sakwa", "Zapisz grę", "Wróć do menu głównego");
            int choice = GameIO.ReadMenuChoice(7);
            GameIO.Clear();
            switch (choice)
            {
                case 1:
                    SessionEnd? end = pyramid ? EnterPyramid() : Travel();
                    if (end.HasValue)
                    {
                        return end.Value;
                    }

                    break;
                case 2:
                    Tavern.Visit(hero);
                    break;
                case 3:
                    Shop.Visit(hero);
                    break;
                case 4:
                    ShowStats();
                    break;
                case 5:
                    Inventory.Visit(hero);
                    break;
                case 6:
                    SaveGame();
                    break;
                case 7:
                    if (ConfirmLeave())
                    {
                        return SessionEnd.ReturnedToMenu;
                    }

                    break;
            }
        }
    }

    private SessionEnd? Travel()
    {
        if (!Story.CanTravel(hero))
        {
            GameIO.Info(Story.TravelBlockedMessage);
            return null;
        }

        if (hero.Hp < hero.MaxHp / 3)
        {
            GameIO.Info($"Masz tylko {hero.Hp}/{hero.MaxHp} HP. Na pewno chcesz wyruszyć?");
            GameIO.Menu("Wyruszyć?", "Tak", "Nie, najpierw się wyleczę");
            if (GameIO.ReadMenuChoice(2) == 2)
            {
                GameIO.Clear();
                return null;
            }
        }

        CombatOutcome outcome = Combat.Run(hero, Encounters.ForLevel(hero.Level));
        return outcome == CombatOutcome.Defeat ? Defeat() : null;
    }

    private SessionEnd? EnterPyramid()
    {
        GameIO.Info("Karawana rusza o świcie. Nie będzie odwrotu.");
        GameIO.Menu("Wyruszyć?", "Tak, do piramidy", "Jeszcze nie");
        if (GameIO.ReadMenuChoice(2) == 2)
        {
            GameIO.Clear();
            return null;
        }

        GameIO.Clear();
        Dialogues.PyramidHistory();
        if (Combat.Run(hero, Encounters.PyramidGuards()) == CombatOutcome.Defeat)
        {
            return Defeat();
        }

        Dialogues.AfterGuards();
        hero.FullHeal();
        if (Combat.Run(hero, Encounters.FinalBoss()) == CombatOutcome.Defeat)
        {
            return Defeat();
        }

        hero.Completed = true;
        Dialogues.Ending();
        GameIO.Success($"Ukończyłeś grę jako {hero.Name}, {hero.ClassName} na poziomie {hero.Level}, z {hero.Gold} sztukami złota!");
        GameIO.PressAnyKey();
        return SessionEnd.Completed;
    }

    private static SessionEnd Defeat()
    {
        GameIO.Error("\nZostałeś pokonany. Twoja wyprawa kończy się w piachu, a Graal pozostaje legendą.");
        GameIO.Info("Możesz wczytać ostatni zapis z menu głównego.");
        GameIO.PressAnyKey();
        return SessionEnd.Died;
    }

    private void SaveGame()
    {
        if (SaveSystem.SaveExists())
        {
            GameIO.Menu("Nadpisać istniejący zapis?", "Nie", "Tak");
            if (GameIO.ReadMenuChoice(2) == 1)
            {
                GameIO.Clear();
                return;
            }
        }

        GameIO.Clear();
        if (SaveSystem.Save(hero, out string message))
        {
            GameIO.Success(message);
        }
        else
        {
            GameIO.Error(message);
        }
    }

    private static bool ConfirmLeave()
    {
        GameIO.Menu("Niezapisany postęp zostanie utracony. Wrócić do menu?", "Nie", "Tak");
        bool leave = GameIO.ReadMenuChoice(2) == 2;
        GameIO.Clear();
        return leave;
    }

    private void ShowStats()
    {
        GameIO.Header("Statystyki bohatera");
        GameIO.WriteLine($"Imię: {hero.Name}   Klasa: {hero.ClassName}");
        GameIO.WriteLine($"Poziom: {hero.Level}" + (hero.IsMaxLevel ? " (maksymalny)" : $"   Doświadczenie: {hero.Exp}/{hero.ExpToNextLevel}"));
        GameIO.WriteLine($"Punkty zdrowia: {hero.Hp}/{hero.MaxHp}");
        GameIO.WriteLine($"Żywotność: {hero.Vit}   Siła: {hero.Str}   Zręczność: {hero.Dex}");
        GameIO.WriteLine($"Obrażenia: {hero.MinDmg}-{hero.MaxDmg}   Szansa trafienia: {hero.HitChance:0}%   Krytyk: {hero.CritChance:0}%");
        GameIO.WriteLine($"Pancerz: {hero.Armor} (redukcja {100 - (100 * 100 / (100 + hero.Armor))}%)   Uniki: {hero.Evasion}%   Ucieczka: {hero.FleeChance}%");
        GameIO.WriteLine($"Złoto: {hero.Gold}   Mikstury: {hero.Inventory.Count}");
        GameIO.WriteLine($"Etap wyprawy: {StageName(hero.Stage)}");
        GameIO.WriteLine("-------------------");
    }

    private static string StageName(StoryStage stage) => stage switch
    {
        StoryStage.Start => "początek – pogadaj z barmanem",
        StoryStage.BanditsCalmed => "bandyci za bramą",
        StoryStage.WolvesCleared => "wilki w lasach",
        StoryStage.CaravanAnnounced => "karawana w drodze",
        StoryStage.CaravanReady => "karawana czeka przy bramie",
        _ => stage.ToString(),
    };
}
