namespace PyramidTreasureConsoleRPG.Engine;

/// <summary>Dobór grup przeciwników do poziomu bohatera. Każdy przedział ma kilka wariantów.</summary>
public static class Encounters
{
    private static readonly EnemyDefinition[][][] Tables =
    [
        // poziom 1-3
        [
            [EnemyCatalog.Thief, EnemyCatalog.Thief],
            [EnemyCatalog.Thief, EnemyCatalog.Thief, EnemyCatalog.Thief],
        ],
        // poziom 4-6
        [
            [EnemyCatalog.Wolf, EnemyCatalog.Wolf],
            [EnemyCatalog.Thief, EnemyCatalog.Thief, EnemyCatalog.Wolf],
            [EnemyCatalog.Wolf, EnemyCatalog.WildBoar],
        ],
        // poziom 7-9
        [
            [EnemyCatalog.Wolf, EnemyCatalog.WildBoar, EnemyCatalog.ArmoredThief],
            [EnemyCatalog.ArmoredThief, EnemyCatalog.ArmoredThief],
        ],
        // poziom 10-12
        [
            [EnemyCatalog.WildBoar, EnemyCatalog.FallenKnight, EnemyCatalog.FallenKnight],
            [EnemyCatalog.ArmoredThief, EnemyCatalog.ArmoredThief, EnemyCatalog.FallenKnight],
        ],
        // poziom 13-15
        [
            [EnemyCatalog.ArmoredThief, EnemyCatalog.FallenKnight, EnemyCatalog.Templar, EnemyCatalog.Templar],
            [EnemyCatalog.FallenKnight, EnemyCatalog.FallenKnight, EnemyCatalog.Templar],
        ],
        // poziom 16-17
        [
            [EnemyCatalog.Templar, EnemyCatalog.Templar, EnemyCatalog.CryingMonk],
            [EnemyCatalog.FallenKnight, EnemyCatalog.Templar, EnemyCatalog.CryingMonk],
        ],
        // poziom 18-20
        [
            [EnemyCatalog.Templar, EnemyCatalog.CryingMonk, EnemyCatalog.CryingMonk],
            [EnemyCatalog.Templar, EnemyCatalog.Templar, EnemyCatalog.Templar, EnemyCatalog.CryingMonk],
        ],
    ];

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
        EnemyDefinition[] group = rng.Pick(Tables[BracketFor(level)]);
        return rng.Shuffle(group.Select(d => d.Spawn()));
    }

    public static List<Enemy> PyramidGuards() => [EnemyCatalog.Anubis.Spawn(), EnemyCatalog.Anubis.Spawn()];

    public static List<Enemy> FinalBoss() => [EnemyCatalog.Ra.Spawn()];
}
