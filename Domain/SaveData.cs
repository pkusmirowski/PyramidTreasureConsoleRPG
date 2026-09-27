namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Płaski obiekt zapisu (JSON). Statystyki pochodne nie są zapisywane – liczy się je z Vit/Str/Dex.</summary>
public sealed class SaveData
{
    public const int CurrentVersion = 2;

    public int Version { get; set; } = CurrentVersion;

    public string Name { get; set; } = string.Empty;

    public int Class { get; set; }

    public int Level { get; set; } = 1;

    public int Exp { get; set; }

    public int Vit { get; set; }

    public int Str { get; set; }

    public int Dex { get; set; }

    public int Hp { get; set; }

    public int Gold { get; set; }

    public int Stage { get; set; }

    public bool SpecialDrinkUsed { get; set; }

    public bool Completed { get; set; }

    public List<int> Potions { get; set; } = new();

    public DateTime SavedAt { get; set; }
}
