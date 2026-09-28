namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Mapa regionów i podróż. Kroki podróży (walki, zdarzenia) wykonuje przez ekran regionu.</summary>
public sealed class MapScreen(IGameIO io, IRandomSource rng, CombatScreen combat, EventScreen events)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly CombatScreen combat = combat ?? throw new ArgumentNullException(nameof(combat));
    private readonly EventScreen events = events ?? throw new ArgumentNullException(nameof(events));

    public SessionEnd? Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        io.Header("Mapa");
        var regions = RegionCatalog.All;
        var labels = regions.Select(r => Describe(hero, r)).Append("Zostań tutaj").ToArray();
        int choice = io.Menu($"Jesteś w: {RegionCatalog.Get(hero.CurrentRegion).Name}. Dokąd?", labels);
        io.Clear();
        if (choice > regions.Count)
        {
            return null;
        }

        RegionDefinition target = regions[choice - 1];
        TravelCheck check = TravelEngine.CanTravel(hero, target);
        if (!check.Allowed)
        {
            io.ShowError($"Nie możesz tam teraz jechać: {check.Reason}.");
            return null;
        }

        TravelPlan plan = TravelEngine.Depart(hero, target, rng);
        io.ShowInfo($"Ruszasz do: {target.Name}. Droga zajmie {plan.Days} dni" + (plan.Cost > 0 ? $" i kosztowała {plan.Cost} złota." : "."));
        int day = 0;
        foreach (TravelStep step in plan.Steps)
        {
            day++;
            io.WriteLine($"Dzień {hero.Day + day} w drodze.", ConsoleColor.DarkGray);
            SessionEnd? end = RunStep(hero, step);
            if (end.HasValue)
            {
                return end.Value;
            }
        }

        ArrivalReport arrival = TravelEngine.Arrive(hero, target);
        io.Clear();
        io.Narrate(target.Arrival);
        foreach (string note in arrival.DayNotes)
        {
            io.WriteLine(note, ConsoleColor.Magenta);
        }

        return null;
    }

    private SessionEnd? RunStep(Hero hero, TravelStep step)
    {
        switch (step.Kind)
        {
            case TravelStepKind.Fight:
                io.ShowInfo(step.Text);
                return combat.Run(hero, step.Enemies!) == CombatStatus.Defeat ? SessionEnd.Died : null;
            case TravelStepKind.Event:
                return events.Run(hero, step.Event!) == CombatStatus.Defeat ? SessionEnd.Died : null;
            default:
                io.ShowInfo(step.Text);
                io.Pause(600);
                return null;
        }
    }

    private static string Describe(Hero hero, RegionDefinition region)
    {
        if (region.Id == hero.CurrentRegion)
        {
            return $"{region.Name} – tu jesteś";
        }

        TravelCheck check = TravelEngine.CanTravel(hero, region);
        string cost = region.TravelCost > 0 ? $"{region.TravelDays} dni, {region.TravelCost} g, poziom {region.RecommendedLevel}+" : $"{region.TravelDays} dni, poziom {region.RecommendedLevel}+";
        return check.Allowed ? $"{region.Name} ({cost})" : $"{region.Name} ({cost}) – {check.Reason}";
    }
}
