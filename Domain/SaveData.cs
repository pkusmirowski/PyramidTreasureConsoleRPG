namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Płaski obiekt zapisu (JSON). Statystyki pochodne nie są zapisywane – liczy się je z Vit/Str/Dex.
/// Kolekcje są opcjonalne, żeby starsze zapisy (bez tych pól) dało się wczytać.</summary>
public sealed class SaveData
{
    public const int CurrentVersion = 6;

    /// <summary>Najstarsza wersja, którą umiemy wczytać (z migracją).</summary>
    public const int OldestSupportedVersion = 2;

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

    public IReadOnlyList<int>? Potions { get; init; }

    public int Day { get; init; } = 1;

    public int Region { get; init; }

    public Dictionary<string, int>? Reputation { get; init; }

    public IReadOnlyList<string>? Flags { get; init; }

    public IReadOnlyList<QuestSaveEntry>? Quests { get; init; }

    public int? Weapon { get; init; }

    public int? ArmorItem { get; init; }

    public int? Trinket { get; init; }

    public IReadOnlyList<int>? Gear { get; init; }

    public IReadOnlyList<int>? Talents { get; init; }

    public int Addiction { get; init; }

    public int LastLotusDay { get; init; } = -100;

    public bool Craving { get; init; }

    public int Debt { get; init; }

    public int DebtDay { get; init; }

    public int NightmareNights { get; init; }

    public int? NextFightBuff { get; init; }

    public StatisticsSaveData? Stats { get; init; }

    public int NewGamePlus { get; init; }

    public int? Ending { get; init; }

    /// <summary>Ustawiane przez magazyn zapisu w chwili zapisu.</summary>
    public DateTime SavedAt { get; set; }
}

public sealed class QuestSaveEntry
{
    public required string Id { get; init; }

    public int Status { get; init; }

    public int Progress { get; init; }
}
