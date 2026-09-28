namespace PyramidTreasureConsoleRPG.Engine;

public sealed record EndingOption(EndingDefinition Ending, bool Available, string? Reason);

/// <summary>Po Ra: które zakończenia są dostępne i co zmieniają w bohaterze.</summary>
public static class EndingEngine
{
    public const int BrotherhoodReputationRequired = 30;

    public static IReadOnlyList<EndingOption> Options(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return EndingCatalog.All.Select(e => Check(hero, e)).ToList();
    }

    public static EndingOption Check(Hero hero, EndingDefinition ending)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(ending);
        return ending.Kind switch
        {
            EndingKind.GiveToBrotherhood when hero.GetReputation(Faction.Brotherhood) < BrotherhoodReputationRequired
                && !hero.HasFlag("neferet:vow") && !hero.HasFlag("ending:brotherhood_hint")
                => new EndingOption(ending, false, $"wymaga reputacji Bractwa ≥ {BrotherhoodReputationRequired} albo obietnicy danej Neferet"),
            EndingKind.Destroy when !hero.HasFlag("map:nomads")
                => new EndingOption(ending, false, "wymaga mapy koczowników"),
            _ => new EndingOption(ending, true, null),
        };
    }

    /// <summary>Zapisuje wybór w bohaterze. Rzuca, gdy zakończenie nie jest dostępne.</summary>
    public static EndingDefinition Apply(Hero hero, EndingKind kind)
    {
        ArgumentNullException.ThrowIfNull(hero);
        EndingDefinition ending = EndingCatalog.Get(kind);
        if (!Check(hero, ending).Available)
        {
            throw new InvalidOperationException("To zakończenie nie jest dostępne.");
        }

        hero.Ending = kind;
        hero.Completed = true;
        hero.SetFlag($"ending:{kind}");
        switch (kind)
        {
            case EndingKind.TakeGrail:
                hero.Stats.RecordChoice(-1);
                break;
            case EndingKind.GiveToBrotherhood:
                hero.Stats.RecordChoice(1);
                hero.AdjustReputation(Faction.Brotherhood, 30);
                break;
            default:
                hero.Stats.RecordChoice(1);
                hero.AdjustReputation(Faction.Town, 20);
                break;
        }

        return ending;
    }
}
