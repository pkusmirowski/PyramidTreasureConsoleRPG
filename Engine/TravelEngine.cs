namespace PyramidTreasureConsoleRPG.Engine;

public enum TravelStepKind
{
    Quiet,
    Fight,
    Event,
}

public sealed record TravelStep(TravelStepKind Kind, string Text, List<Enemy>? Enemies, GameEvent? Event);

public sealed record TravelCheck(bool Allowed, string? Reason);

public sealed record TravelPlan(RegionDefinition Target, int Days, int Cost, IReadOnlyList<TravelStep> Steps);

public sealed record ArrivalReport(IReadOnlyList<string> DayNotes);

/// <summary>Podróż między regionami i eksploracja regionu. Każdy krok to walka, zdarzenie albo spokojny dzień.</summary>
public static class TravelEngine
{
    private static readonly string[] QuietRoad =
    [
        "Dzień mija na marszu. Nic nie wychodzi z piasku, nic nie wychodzi z trzcin.",
        "Nocujesz przy drodze. Gwiazdy są tu większe niż w porcie.",
        "Mijasz karawanę idącą w przeciwną stronę. Nikt nie macha.",
        "Upał, kurz, pęcherze. Zwykły dzień w drodze.",
    ];

    private static readonly string[] QuietExplore =
    [
        "Krążysz do zmierzchu. Ślady są, ale stare.",
        "Cisza. Zbyt duża cisza, ale nic z niej nie wynika.",
        "Znajdujesz tylko kości i wypalone ognisko.",
    ];

    public static TravelCheck CanTravel(Hero hero, RegionDefinition target)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(target);
        if (target.Id == hero.CurrentRegion)
        {
            return new TravelCheck(false, "już tu jesteś");
        }

        if (hero.Stage < target.RequiredStage)
        {
            return new TravelCheck(false, "droga jeszcze zamknięta – wykonaj zadania barmana");
        }

        if (target.IsFinal && !hero.IsMaxLevel)
        {
            return new TravelCheck(false, $"karawana nie zabierze słabeusza – potrzebny poziom {CombatMath.MaxLevel}");
        }

        return hero.Gold < target.TravelCost ? new TravelCheck(false, $"brak złota na drogę ({target.TravelCost} g)") : new TravelCheck(true, null);
    }

    /// <summary>Pobiera koszt i losuje kroki podróży. Kroki wykonuje ekran; na końcu wywołuje Arrive.</summary>
    public static TravelPlan Depart(Hero hero, RegionDefinition target, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(rng);
        TravelCheck check = CanTravel(hero, target);
        if (!check.Allowed)
        {
            throw new InvalidOperationException(check.Reason);
        }

        hero.Gold -= target.TravelCost;
        var steps = new List<TravelStep>();
        int eventful = target.IsFinal ? 0 : Math.Max(0, target.TravelDays - 1);
        for (int day = 0; day < eventful; day++)
        {
            steps.Add(RollStep(hero, target, rng, QuietRoad, fightChance: 40, eventChance: 30));
        }

        return new TravelPlan(target, target.TravelDays, target.TravelCost, steps);
    }

    public static ArrivalReport Arrive(Hero hero, RegionDefinition target)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(target);
        DayReport report = DayService.AdvanceDays(hero, target.TravelDays);
        hero.CurrentRegion = target.Id;
        QuestEngine.OnArrived(hero, target.Id);
        return new ArrivalReport(report.Notes);
    }

    /// <summary>Eksploracja bieżącego regionu: jeden dzień, walka albo zdarzenie albo nic. Komunikaty dnia w DayNotes.</summary>
    public static TravelStep Explore(Hero hero, RegionDefinition region, IRandomSource rng, out IReadOnlyList<string> dayNotes)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(rng);
        dayNotes = DayService.AdvanceDays(hero, 1).Notes;
        if (hero.HasFlag($"intel:{region.Id}"))
        {
            // Informacje z przesłuchania: zamiast zasadzki trafiasz na zdarzenie.
            hero.ClearFlag($"intel:{region.Id}");
            GameEvent? intel = EventEngine.Pick(hero, region.Id, rng);
            if (intel is not null)
            {
                return new TravelStep(TravelStepKind.Event, intel.Title, null, intel);
            }
        }

        return RollStep(hero, region, rng, QuietExplore, region.FightChancePercent, region.EventChancePercent);
    }

    private static TravelStep RollStep(Hero hero, RegionDefinition region, IRandomSource rng, string[] quietLines, int fightChance, int eventChance)
    {
        int roll = rng.Range(1, 100);
        if (roll <= fightChance && region.Encounters.Length > 0)
        {
            return new TravelStep(TravelStepKind.Fight, "Ktoś zastępuje ci drogę.", Encounters.InRegion(region, rng, hero.Level), null);
        }

        if (roll <= fightChance + eventChance)
        {
            GameEvent? ev = EventEngine.Pick(hero, region.Id, rng);
            if (ev is not null)
            {
                return new TravelStep(TravelStepKind.Event, ev.Title, null, ev);
            }
        }

        return new TravelStep(TravelStepKind.Quiet, rng.Pick(quietLines), null, null);
    }
}
