namespace PyramidTreasureConsoleRPG.Engine;

/// <summary>Dobór grup przeciwników do poziomu bohatera. Każdy przedział ma kilka wariantów.</summary>
public static class Encounters
{
    private static readonly IReadOnlyList<IReadOnlyList<Func<List<Enemy>>>> Tables = new List<IReadOnlyList<Func<List<Enemy>>>>
    {
        // poziom 1-3
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { EnemyCatalog.Thief.Spawn(), EnemyCatalog.Thief.Spawn() },
            () => new List<Enemy> { EnemyCatalog.Thief.Spawn(), EnemyCatalog.Thief.Spawn(), EnemyCatalog.Thief.Spawn() },
        },
        // poziom 4-6
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { EnemyCatalog.Wolf.Spawn(), EnemyCatalog.Wolf.Spawn() },
            () => new List<Enemy> { EnemyCatalog.Thief.Spawn(), EnemyCatalog.Thief.Spawn(), EnemyCatalog.Wolf.Spawn() },
            () => new List<Enemy> { EnemyCatalog.Wolf.Spawn(), EnemyCatalog.WildBoar.Spawn() },
        },
        // poziom 7-9
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { EnemyCatalog.Wolf.Spawn(), EnemyCatalog.WildBoar.Spawn(), EnemyCatalog.ArmoredThief.Spawn() },
            () => new List<Enemy> { EnemyCatalog.ArmoredThief.Spawn(), EnemyCatalog.ArmoredThief.Spawn() },
        },
        // poziom 10-12
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { EnemyCatalog.WildBoar.Spawn(), EnemyCatalog.FallenKnight.Spawn(), EnemyCatalog.FallenKnight.Spawn() },
            () => new List<Enemy> { EnemyCatalog.ArmoredThief.Spawn(), EnemyCatalog.ArmoredThief.Spawn(), EnemyCatalog.FallenKnight.Spawn() },
        },
        // poziom 13-15
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { EnemyCatalog.ArmoredThief.Spawn(), EnemyCatalog.FallenKnight.Spawn(), EnemyCatalog.Templar.Spawn(), EnemyCatalog.Templar.Spawn() },
            () => new List<Enemy> { EnemyCatalog.FallenKnight.Spawn(), EnemyCatalog.FallenKnight.Spawn(), EnemyCatalog.Templar.Spawn() },
        },
        // poziom 16-17
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { EnemyCatalog.Templar.Spawn(), EnemyCatalog.Templar.Spawn(), EnemyCatalog.CryingMonk.Spawn() },
            () => new List<Enemy> { EnemyCatalog.FallenKnight.Spawn(), EnemyCatalog.Templar.Spawn(), EnemyCatalog.CryingMonk.Spawn() },
        },
        // poziom 18-20
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { EnemyCatalog.Templar.Spawn(), EnemyCatalog.CryingMonk.Spawn(), EnemyCatalog.CryingMonk.Spawn() },
            () => new List<Enemy> { EnemyCatalog.Templar.Spawn(), EnemyCatalog.Templar.Spawn(), EnemyCatalog.Templar.Spawn(), EnemyCatalog.CryingMonk.Spawn() },
        },
    };

    public static int BracketFor(int level) => level switch
    {
        <= 3 => 0,
        <= 6 => 1,
        <= 9 => 2,
        <= 12 => 3,
        <= 15 => 4,
        <= 17 => 5,
        _ => 6,
    };

    /// <summary>Losowa grupa wrogów dla poziomu, w losowej kolejności.</summary>
    public static List<Enemy> ForLevel(int level, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        var variants = Tables[BracketFor(level)];
        return rng.Shuffle(rng.Pick(variants)());
    }

    public static List<Enemy> PyramidGuards() => new() { EnemyCatalog.Anubis.Spawn(), EnemyCatalog.Anubis.Spawn() };

    public static List<Enemy> FinalBoss() => new() { EnemyCatalog.Ra.Spawn() };
}
