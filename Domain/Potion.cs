namespace PyramidTreasureConsoleRPG.Domain;

public enum PotionKind
{
    Small = 1,
    Medium = 2,
    Large = 3,

    /// <summary>Whisky: +10 trafienia i −10 uników w następnej walce.</summary>
    Whisky = 4,

    /// <summary>Lotos: +25% obrażeń w następnej walce, uzależnia.</summary>
    Lotus = 5,

    /// <summary>Odtrutka: leczy truciznę i osłabienie, gasi głód lotosu.</summary>
    Antidote = 6,
}

public enum PotionUse
{
    Heal,
    Whisky,
    Lotus,
    Antidote,
}

/// <summary>Konsumowalne: mikstury lecznicze i używki. Cena i moc są w jednym miejscu.</summary>
public sealed record Potion(PotionKind Kind, string Name, int Price, int RestoreHp, PotionUse Effect = PotionUse.Heal)
{
    public static Potion Small { get; } = new(PotionKind.Small, "Mała mikstura lecząca", Price: 20, RestoreHp: 50);

    public static Potion Medium { get; } = new(PotionKind.Medium, "Średnia mikstura lecząca", Price: 50, RestoreHp: 150);

    public static Potion Large { get; } = new(PotionKind.Large, "Duża mikstura lecząca", Price: 100, RestoreHp: 350);

    public static Potion Whisky { get; } = new(PotionKind.Whisky, "Butelka whisky", Price: 25, RestoreHp: 0, PotionUse.Whisky);

    public static Potion Lotus { get; } = new(PotionKind.Lotus, "Fajka lotosu", Price: 40, RestoreHp: 0, PotionUse.Lotus);

    public static Potion Antidote { get; } = new(PotionKind.Antidote, "Odtrutka", Price: 35, RestoreHp: 0, PotionUse.Antidote);

    public static IReadOnlyList<Potion> All { get; } = [Small, Medium, Large, Whisky, Lotus, Antidote];

    /// <summary>Tylko lecznicze – to, co sprzedaje alchemik w każdym sklepie.</summary>
    public static IReadOnlyList<Potion> Healing { get; } = [Small, Medium, Large];

    public static IReadOnlyList<PotionKind> AllKinds { get; } = All.Select(p => p.Kind).ToList();

    public string Description => Effect switch
    {
        PotionUse.Heal => $"leczy {RestoreHp} HP",
        PotionUse.Whisky => "+10 trafienia, −10 uników w następnej walce",
        PotionUse.Lotus => "+25% obrażeń w następnej walce; uzależnia",
        PotionUse.Antidote => "leczy truciznę i osłabienie, gasi głód lotosu",
        _ => string.Empty,
    };

    public static Potion Create(PotionKind kind) => All.FirstOrDefault(p => p.Kind == kind) ?? throw new ArgumentOutOfRangeException(nameof(kind));
}
