namespace PyramidTreasureConsoleRPG;

/// <summary>Etapy fabuły. Dawniej int 0–4 w polu GameStatus.</summary>
public enum StoryStage
{
    Start = 0,
    BanditsCalmed = 1,
    WolvesCleared = 2,
    CaravanAnnounced = 3,
    CaravanReady = 4,
}

/// <summary>Bramkowanie fabuły: przed każdym nowym przedziałem poziomów trzeba pogadać z barmanem.</summary>
public static class Story
{
    public const string TravelBlockedMessage = "Zanim wyruszysz w drogę, pogadaj z barmanem w tawernie – ma dla ciebie wieści.";

    public static StoryStage RequiredStageForTravel(int level) => level switch
    {
        < 5 => StoryStage.BanditsCalmed,
        < 10 => StoryStage.WolvesCleared,
        < CombatMath.MaxLevel => StoryStage.CaravanAnnounced,
        _ => StoryStage.CaravanReady,
    };

    public static bool CanTravel(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return hero.Stage >= RequiredStageForTravel(hero.Level);
    }

    public static bool CanEnterPyramid(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return hero.IsMaxLevel && hero.Stage >= StoryStage.CaravanReady;
    }

    /// <summary>Czy barman ma coś nowego do powiedzenia (następny etap jest odblokowany poziomem).</summary>
    public static bool BarmanHasNews(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return hero.Stage < RequiredStageForTravel(hero.Level);
    }

    /// <summary>Rozmowa z barmanem: przesuwa fabułę o jeden etap, jeśli poziom na to pozwala.</summary>
    public static void TalkToBarman(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (!BarmanHasNews(hero))
        {
            GameIO.Info("Barman wzrusza ramionami: \"Na ten moment nie mam żadnych nowych wieści. Wróć, jak nabierzesz doświadczenia.\"");
            return;
        }

        StoryStage next = hero.Stage + 1;
        Dialogues.Barman(next);
        hero.Stage = next;
    }
}
