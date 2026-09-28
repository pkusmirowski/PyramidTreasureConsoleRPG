namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Po Ra: wybór zakończenia, narracja i podsumowanie wyprawy.</summary>
public sealed class EndingScreen(IGameIO io)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));

    public EndingKind Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        io.Narrate(Dialogues.Ending);
        io.WriteLine();

        IReadOnlyList<EndingOption> options = EndingEngine.Options(hero);
        string[] labels = options
            .Select(o => o.Available ? o.Ending.Choice : $"{o.Ending.Choice} – niedostępne: {o.Reason}")
            .ToArray();
        int index;
        while (true)
        {
            index = io.Menu("Co robisz z Graalem?", labels) - 1;
            if (options[index].Available)
            {
                break;
            }

            io.ShowError("To zakończenie nie jest dostępne w tej wyprawie.");
        }

        io.Clear();
        EndingDefinition ending = EndingEngine.Apply(hero, options[index].Ending.Kind);
        io.Narrate(ending.Text);
        io.WriteLine();
        ShowSummary(hero, ending);
        io.PressAnyKey();
        return ending.Kind;
    }

    public void ShowSummary(Hero hero, EndingDefinition ending)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(ending);
        io.Header($"Podsumowanie wyprawy: {ending.Title}");
        foreach ((string label, string value) in HeroStats.SummaryRows(hero))
        {
            io.WriteLine($"{label}: {value}");
        }

        io.WriteLine();
        io.ShowInfo(ending.NewGamePlusBonus);
    }
}
