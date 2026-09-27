namespace PyramidTreasureConsoleRPG.Domain;

public enum PotionKind
{
    Small = 1,
    Medium = 2,
    Large = 3,
}

/// <summary>Mikstura lecząca. Cena i moc są w jednym miejscu – menu sklepu czyta je stąd.</summary>
public sealed record Potion(PotionKind Kind, string Name, int Price, int RestoreHp)
{
    public static Potion Small { get; } = new(PotionKind.Small, "Mała mikstura lecząca", Price: 20, RestoreHp: 50);

    public static Potion Medium { get; } = new(PotionKind.Medium, "Średnia mikstura lecząca", Price: 50, RestoreHp: 150);

    public static Potion Large { get; } = new(PotionKind.Large, "Duża mikstura lecząca", Price: 100, RestoreHp: 350);

    public static IReadOnlyList<Potion> All { get; } = [Small, Medium, Large];

    public static IReadOnlyList<PotionKind> AllKinds { get; } = All.Select(p => p.Kind).ToList();

    public static Potion Create(PotionKind kind) => kind switch
    {
        PotionKind.Small => Small,
        PotionKind.Medium => Medium,
        PotionKind.Large => Large,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
