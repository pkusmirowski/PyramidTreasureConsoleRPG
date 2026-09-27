namespace PyramidTreasureConsoleRPG;

public enum PotionKind
{
    Small = 1,
    Medium = 2,
    Large = 3,
}

/// <summary>Mikstura lecząca. Cena i moc są w jednym miejscu – menu sklepu czyta je stąd.</summary>
public abstract class Potion
{
    protected Potion(PotionKind kind, string name, int price, int restoreHp)
    {
        Kind = kind;
        Name = name;
        Price = price;
        RestoreHp = restoreHp;
    }

    public PotionKind Kind { get; }

    public string Name { get; }

    public int Price { get; }

    public int RestoreHp { get; }

    public static readonly IReadOnlyList<PotionKind> AllKinds = new[] { PotionKind.Small, PotionKind.Medium, PotionKind.Large };

    public static Potion Create(PotionKind kind) => kind switch
    {
        PotionKind.Small => new SmallPotion(),
        PotionKind.Medium => new MediumPotion(),
        PotionKind.Large => new LargePotion(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
