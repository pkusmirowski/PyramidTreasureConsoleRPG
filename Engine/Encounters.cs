namespace PyramidTreasureConsoleRPG.Engine;

/// <summary>Dobór grup przeciwników z puli regionu.</summary>
public static class Encounters
{
    /// <summary>Losowa grupa wrogów z regionu, w losowej kolejności. Pusta lista, gdy region nie ma pul.</summary>
    public static List<Enemy> InRegion(RegionDefinition region, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(rng);
        if (region.Encounters.Length == 0)
        {
            return [];
        }

        EnemyDefinition[] group = rng.Pick(region.Encounters);
        return rng.Shuffle(group.Select(d => d.Spawn()));
    }

    public static List<Enemy> PyramidGuards() => [EnemyCatalog.Anubis.Spawn(), EnemyCatalog.Anubis.Spawn()];

    public static List<Enemy> FinalBoss() => [EnemyCatalog.Ra.Spawn()];
}
