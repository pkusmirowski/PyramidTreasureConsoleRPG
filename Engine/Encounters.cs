namespace PyramidTreasureConsoleRPG.Engine;

/// <summary>Dobór grup przeciwników do poziomu bohatera. Każdy przedział ma kilka wariantów.</summary>
public static class Encounters
{
    private static readonly IReadOnlyList<IReadOnlyList<Func<List<Enemy>>>> Tables = new List<IReadOnlyList<Func<List<Enemy>>>>
    {
        // poziom 1-3
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { new Thief(), new Thief() },
            () => new List<Enemy> { new Thief(), new Thief(), new Thief() },
        },
        // poziom 4-6
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { new Wolf(), new Wolf() },
            () => new List<Enemy> { new Thief(), new Thief(), new Wolf() },
            () => new List<Enemy> { new Wolf(), new WildBoar() },
        },
        // poziom 7-9
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { new Wolf(), new WildBoar(), new ArmoredThief() },
            () => new List<Enemy> { new ArmoredThief(), new ArmoredThief() },
        },
        // poziom 10-12
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { new WildBoar(), new FallenKnight(), new FallenKnight() },
            () => new List<Enemy> { new ArmoredThief(), new ArmoredThief(), new FallenKnight() },
        },
        // poziom 13-15
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { new ArmoredThief(), new FallenKnight(), new Templar(), new Templar() },
            () => new List<Enemy> { new FallenKnight(), new FallenKnight(), new Templar() },
        },
        // poziom 16-17
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { new Templar(), new Templar(), new CryingMonk() },
            () => new List<Enemy> { new FallenKnight(), new Templar(), new CryingMonk() },
        },
        // poziom 18-20
        new List<Func<List<Enemy>>>
        {
            () => new List<Enemy> { new Templar(), new CryingMonk(), new CryingMonk() },
            () => new List<Enemy> { new Templar(), new Templar(), new Templar(), new CryingMonk() },
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

    public static List<Enemy> PyramidGuards() => new() { new Anubis(), new Anubis() };

    public static List<Enemy> FinalBoss() => new() { new Ra() };
}
