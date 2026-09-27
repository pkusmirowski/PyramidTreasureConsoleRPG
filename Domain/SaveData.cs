namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Płaski obiekt zapisu (JSON). Statystyki pochodne nie są zapisywane – liczy się je z Vit/Str/Dex.</summary>
public sealed class SaveData
{
    public const int CurrentVersion = 2;

    public int Version { get; init; } = CurrentVersion;

    public required string Name { get; init; }

    public required int Class { get; init; }

    public int Level { get; init; } = 1;

    public int Exp { get; init; }

    public int Vit { get; init; }

    public int Str { get; init; }

    public int Dex { get; init; }

    public int Hp { get; init; }

    public int Gold { get; init; }

    public int Stage { get; init; }

    public bool SpecialDrinkUsed { get; init; }

    public bool Completed { get; init; }

    public IReadOnlyList<int> Potions { get; init; } = [];

    /// <summary>Ustawiane przez magazyn zapisu w chwili zapisu.</summary>
    public DateTime SavedAt { get; set; }
}
