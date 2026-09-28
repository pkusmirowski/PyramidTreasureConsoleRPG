namespace PyramidTreasureConsoleRPG.Domain;

public enum RegionId
{
    Port = 0,
    OldTown = 1,
    Delta = 2,
    Desert = 3,
    Oasis = 4,
    Pyramid = 5,
}

public enum Faction
{
    Town,
    Underworld,
    Brotherhood,
}

/// <summary>Kto w regionie daje zadania.</summary>
public enum QuestGiverId
{
    Barman,
    Captain,
    Smuggler,
    Priestess,
}

/// <summary>
/// Region świata: opis, wymagany etap fabuły, koszt podróży, pule wrogów i to, co można w nim robić.
/// </summary>
public sealed record RegionDefinition(
    RegionId Id,
    string Name,
    IReadOnlyList<string> Arrival,
    string ExploreLabel,
    StoryStage RequiredStage,
    int TravelDays,
    int TravelCost,
    EnemyDefinition[][] Encounters,
    int FightChancePercent,
    int EventChancePercent,
    bool HasTavern,
    bool HasShop,
    IReadOnlyList<QuestGiverId> QuestGivers)
{
    public bool IsHome => Id == RegionId.Port;

    public bool IsFinal => Id == RegionId.Pyramid;
}

public static class RegionCatalog
{
    public static RegionDefinition Port { get; } = new(
        RegionId.Port,
        "Port Sokoła",
        [
            "Port Sokoła: smród ryb, smoły i taniego rumu. Tu zaczęła się twoja wyprawa i tu wracasz lizać rany.",
            "Za bramą kręcą się złodzieje, w tawernie barman wie wszystko, a kapitan portu szuka kogoś do brudnej roboty.",
        ],
        "Przejdź się za bramę (złodzieje)",
        StoryStage.Start,
        TravelDays: 1,
        TravelCost: 0,
        [
            [EnemyCatalog.Thief, EnemyCatalog.Thief],
            [EnemyCatalog.Thief, EnemyCatalog.Thief, EnemyCatalog.Thief],
        ],
        FightChancePercent: 55,
        EventChancePercent: 35,
        HasTavern: true,
        HasShop: true,
        [QuestGiverId.Barman, QuestGiverId.Captain]);

    public static RegionDefinition OldTown { get; } = new(
        RegionId.OldTown,
        "Stare Miasto",
        [
            "Stare Miasto to labirynt zaułków, w których straż nie zapuszcza się po zmroku.",
            "Przemytnicy, paserzy i dilerzy lotosu prowadzą tu interesy. Każdy patrzy ci na sakwę.",
        ],
        "Zapuść się w zaułki (złodzieje, opancerzeni)",
        StoryStage.BanditsCalmed,
        TravelDays: 1,
        TravelCost: 5,
        [
            [EnemyCatalog.Thief, EnemyCatalog.Thief, EnemyCatalog.ArmoredThief],
            [EnemyCatalog.ArmoredThief, EnemyCatalog.ArmoredThief],
            [EnemyCatalog.Thief, EnemyCatalog.ArmoredThief, EnemyCatalog.ArmoredThief],
        ],
        FightChancePercent: 55,
        EventChancePercent: 40,
        HasTavern: false,
        HasShop: true,
        [QuestGiverId.Smuggler]);

    public static RegionDefinition Delta { get; } = new(
        RegionId.Delta,
        "Delta i las",
        [
            "Delta Nilu: trzciny wyższe od człowieka, bagna i las, w którym od tygodni wyją wilki.",
            "Karawany omijają to miejsce, więc na drogach zostają tylko ci, którzy nie mają nic do stracenia.",
        ],
        "Ruszaj w las (wilki, dziki)",
        StoryStage.BanditsCalmed,
        TravelDays: 2,
        TravelCost: 10,
        [
            [EnemyCatalog.Wolf, EnemyCatalog.Wolf],
            [EnemyCatalog.Wolf, EnemyCatalog.WildBoar],
            [EnemyCatalog.Wolf, EnemyCatalog.Wolf, EnemyCatalog.WildBoar],
            [EnemyCatalog.WildBoar, EnemyCatalog.ArmoredThief],
        ],
        FightChancePercent: 60,
        EventChancePercent: 35,
        HasTavern: false,
        HasShop: false,
        []);

    public static RegionDefinition Desert { get; } = new(
        RegionId.Desert,
        "Szlak Karawan",
        [
            "Pustynia. Słońce wypala oczy, a wiatr odsłania kości tych, którzy szli tędy przed tobą.",
            "Upadli rycerze w zardzewiałych zbrojach i templariusze strzegący czegoś, o czym nikt nie chce mówić.",
        ],
        "Idź szlakiem (upadli rycerze, templariusze)",
        StoryStage.WolvesCleared,
        TravelDays: 3,
        TravelCost: 25,
        [
            [EnemyCatalog.WildBoar, EnemyCatalog.FallenKnight, EnemyCatalog.FallenKnight],
            [EnemyCatalog.ArmoredThief, EnemyCatalog.ArmoredThief, EnemyCatalog.FallenKnight],
            [EnemyCatalog.FallenKnight, EnemyCatalog.FallenKnight, EnemyCatalog.Templar],
            [EnemyCatalog.ArmoredThief, EnemyCatalog.FallenKnight, EnemyCatalog.Templar, EnemyCatalog.Templar],
        ],
        FightChancePercent: 60,
        EventChancePercent: 35,
        HasTavern: false,
        HasShop: false,
        []);

    public static RegionDefinition Oasis { get; } = new(
        RegionId.Oasis,
        "Oaza Siwa",
        [
            "Oaza Siwa: palmy, zimna woda i świątynia, w której Bractwo Płaczącego Mnicha odprawia rytuały.",
            "Ostatni przystanek karawan przed piramidą. Kapłanka wie o Graalu więcej, niż mówi.",
        ],
        "Przeszukaj ruiny wokół oazy (templariusze, mnisi)",
        StoryStage.CaravanAnnounced,
        TravelDays: 4,
        TravelCost: 40,
        [
            [EnemyCatalog.Templar, EnemyCatalog.Templar, EnemyCatalog.CryingMonk],
            [EnemyCatalog.FallenKnight, EnemyCatalog.Templar, EnemyCatalog.CryingMonk],
            [EnemyCatalog.Templar, EnemyCatalog.CryingMonk, EnemyCatalog.CryingMonk],
            [EnemyCatalog.Templar, EnemyCatalog.Templar, EnemyCatalog.Templar, EnemyCatalog.CryingMonk],
        ],
        FightChancePercent: 60,
        EventChancePercent: 35,
        HasTavern: true,
        HasShop: true,
        [QuestGiverId.Priestess]);

    public static RegionDefinition Pyramid { get; } = new(
        RegionId.Pyramid,
        "Piramida Chufu",
        [
            "Piramida Chufu. Karawana zostaje u jej stóp; dalej idziesz sam.",
        ],
        "Wejdź do piramidy",
        StoryStage.CaravanReady,
        TravelDays: 5,
        TravelCost: 0,
        [],
        FightChancePercent: 0,
        EventChancePercent: 0,
        HasTavern: false,
        HasShop: false,
        []);

    public static IReadOnlyList<RegionDefinition> All { get; } = [Port, OldTown, Delta, Desert, Oasis, Pyramid];

    public static RegionDefinition Get(RegionId id) => id switch
    {
        RegionId.Port => Port,
        RegionId.OldTown => OldTown,
        RegionId.Delta => Delta,
        RegionId.Desert => Desert,
        RegionId.Oasis => Oasis,
        RegionId.Pyramid => Pyramid,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    public static string FactionName(Faction faction) => faction switch
    {
        Faction.Town => "Miasto",
        Faction.Underworld => "Podziemie",
        Faction.Brotherhood => "Bractwo",
        _ => faction.ToString(),
    };

    public static string GiverName(QuestGiverId giver) => giver switch
    {
        QuestGiverId.Barman => "Barman",
        QuestGiverId.Captain => "Kapitan portu",
        QuestGiverId.Smuggler => "Przemytnik Hasan",
        QuestGiverId.Priestess => "Kapłanka Neferet",
        _ => giver.ToString(),
    };
}
