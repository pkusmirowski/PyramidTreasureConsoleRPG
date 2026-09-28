namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Zdarzenie z wyborem: tekst, opcje z wymaganiami, wynik, ewentualna walka.</summary>
public sealed class EventScreen(IGameIO io, IRandomSource rng, CombatScreen combat)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly CombatScreen combat = combat ?? throw new ArgumentNullException(nameof(combat));

    /// <summary>Zwraca wynik walki, jeśli zdarzenie ją wywołało; inaczej null.</summary>
    public CombatStatus? Run(Hero hero, GameEvent ev)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(ev);
        io.Header(ev.Title);
        io.Narrate(ev.Text, ConsoleColor.White);

        var labels = new List<string>();
        foreach (EventChoice choice in ev.Choices)
        {
            ChoiceAvailability availability = EventEngine.Availability(hero, choice);
            string label = choice.Text;
            if (choice.Check is SkillCheck check)
            {
                label += $" [test {EventEngine.StatName(check.Stat)} {check.Difficulty}]";
            }

            if (choice.OnSuccess.OfType<FightEffect>().FirstOrDefault() is FightEffect fight)
            {
                label += $" (walka: {DescribeGroup(fight.Enemies)})";
            }
            else if (choice.OnFailure?.OfType<FightEffect>().FirstOrDefault() is FightEffect failFight)
            {
                label += $" (porażka = walka: {DescribeGroup(failFight.Enemies)})";
            }

            labels.Add(availability.Available ? label : $"{label} – niedostępne: {availability.Reason}");
        }

        int index;
        while (true)
        {
            index = io.Menu("Co robisz?", [.. labels]) - 1;
            if (EventEngine.Availability(hero, ev.Choices[index]).Available)
            {
                break;
            }

            io.ShowError("Ta opcja jest niedostępna.");
        }

        io.Clear();
        EventResult result = EventEngine.Resolve(hero, ev, index, rng);
        io.Narrate([result.Text], result.Success ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
        foreach (string note in result.Notes)
        {
            io.WriteLine(note, ConsoleColor.Cyan);
        }

        if (result.Fight is null)
        {
            io.PressAnyKey();
            return null;
        }

        io.Pause(900);
        return combat.Run(hero, result.Fight.Select(d => d.Spawn()));
    }

    /// <summary>„Wilk ×3, Dzik” – skład grupy wrogów w etykiecie wyboru.</summary>
    public static string DescribeGroup(IEnumerable<EnemyDefinition> enemies) =>
        string.Join(", ", enemies.GroupBy(e => e.Name).Select(g => g.Count() > 1 ? $"{g.Key} ×{g.Count()}" : g.Key));
}
