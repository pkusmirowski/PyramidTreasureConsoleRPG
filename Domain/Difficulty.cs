namespace PyramidTreasureConsoleRPG.Domain;

public enum Difficulty
{
    Easy = 1,
    Normal = 2,
    Hard = 3,
}

/// <summary>Poziom trudności: mnożnik statystyk wrogów i nagród (złoto, doświadczenie) w procentach.</summary>
public sealed record DifficultyDefinition(Difficulty Kind, string Name, string Description, int EnemyPercent, int RewardPercent);

public static class DifficultyCatalog
{
    public static DifficultyDefinition Easy { get; } = new(
        Difficulty.Easy,
        "Łatwy",
        "wrogowie słabsi o 20%, złoto i doświadczenie +25%; dla tych, którzy chcą poznać historię",
        EnemyPercent: 80,
        RewardPercent: 125);

    public static DifficultyDefinition Normal { get; } = new(
        Difficulty.Normal,
        "Normalny",
        "gra tak, jak ją zaprojektowano",
        EnemyPercent: 100,
        RewardPercent: 100);

    public static DifficultyDefinition Hard { get; } = new(
        Difficulty.Hard,
        "Trudny",
        "wrogowie silniejsi o 25%, złoto i doświadczenie −15%; każda walka to decyzja",
        EnemyPercent: 125,
        RewardPercent: 85);

    public static IReadOnlyList<DifficultyDefinition> All { get; } = [Easy, Normal, Hard];

    public static DifficultyDefinition Get(Difficulty kind) => kind switch
    {
        Difficulty.Easy => Easy,
        Difficulty.Hard => Hard,
        _ => Normal,
    };
}
