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

/// <summary>Etapy fabuły przesuwają zadania główne; regiony wymagają etapu (patrz RegionCatalog).</summary>
public static class Story
{
    public static bool CanEnterPyramid(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return hero.IsMaxLevel && hero.Stage >= StoryStage.CaravanReady;
    }

    public static string StageName(StoryStage stage) => stage switch
    {
        StoryStage.Start => "początek – pogadaj z barmanem",
        StoryStage.BanditsCalmed => "bandyci za bramą przepędzeni",
        StoryStage.WolvesCleared => "delta oczyszczona z wilków",
        StoryStage.CaravanAnnounced => "szlak karawan otwarty",
        StoryStage.CaravanReady => "karawana czeka przy bramie",
        _ => stage.ToString(),
    };

    public static string ReputationName(int value) => value switch
    {
        >= 60 => "bohater",
        >= 30 => "szanowany",
        >= 10 => "znany",
        > -10 => "obcy",
        > -30 => "podejrzany",
        > -60 => "znienawidzony",
        _ => "wróg",
    };
}
