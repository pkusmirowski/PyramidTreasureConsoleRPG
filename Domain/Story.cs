namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Etapy fabuły.</summary>
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

    /// <summary>Przesuwa fabułę o jeden etap, jeśli poziom na to pozwala. Zwraca nowy etap albo null, gdy nie ma wieści.</summary>
    public static StoryStage? AdvanceByBarman(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (!BarmanHasNews(hero))
        {
            return null;
        }

        hero.Stage += 1;
        return hero.Stage;
    }

    public static string StageName(StoryStage stage) => stage switch
    {
        StoryStage.Start => "początek – pogadaj z barmanem",
        StoryStage.BanditsCalmed => "bandyci za bramą",
        StoryStage.WolvesCleared => "wilki w lasach",
        StoryStage.CaravanAnnounced => "karawana w drodze",
        StoryStage.CaravanReady => "karawana czeka przy bramie",
        _ => stage.ToString(),
    };
}
