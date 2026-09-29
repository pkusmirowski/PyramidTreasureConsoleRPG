namespace PyramidTreasureConsoleRPG.Engine;

/// <summary>Dobór grup przeciwników z puli regionu.</summary>
public static class Encounters
{
    /// <summary>
    /// Losowa grupa wrogów z regionu, w losowej kolejności. Pule regionu są ułożone od najłatwiejszej;
    /// na zalecanym poziomie dostępna jest pierwsza, każde dwa poziomy wyżej odblokowują kolejną.
    /// Pusta lista, gdy region nie ma pul.
    /// </summary>
    public static List<Enemy> InRegion(RegionDefinition region, IRandomSource rng, int heroLevel)
    {
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(rng);
        if (region.Encounters.Length == 0)
        {
            return [];
        }

        int unlocked = Math.Clamp(UnlockedGroups(region, heroLevel), 1, region.Encounters.Length);
        EnemyDefinition[] group = rng.Pick(region.Encounters[..unlocked]);
        return rng.Shuffle(group.Select(d => d.Spawn()));
    }

    /// <summary>Ile pul regionu jest dostępnych na danym poziomie (1 na zalecanym, +1 co dwa poziomy).</summary>
    public static int UnlockedGroups(RegionDefinition region, int heroLevel)
    {
        ArgumentNullException.ThrowIfNull(region);
        return 1 + (Math.Max(0, heroLevel - region.RecommendedLevel + 1) / 2);
    }

    public static List<Enemy> PyramidGuards() => [EnemyCatalog.Anubis.Spawn(), EnemyCatalog.Anubis.Spawn()];

    public static List<Enemy> FinalBoss() => [EnemyCatalog.Ra.Spawn()];
}
