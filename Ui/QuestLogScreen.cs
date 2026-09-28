namespace PyramidTreasureConsoleRPG.Ui;

public sealed class QuestLogScreen(IGameIO io)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        io.Header("Dziennik zadań");
        IReadOnlyList<QuestDefinition> active = QuestEngine.Active(hero);
        if (active.Count == 0)
        {
            io.ShowInfo("Brak aktywnych zadań. Pogadaj z barmanem w porcie albo z ludźmi w innych regionach.");
        }

        foreach (QuestDefinition quest in active)
        {
            (int progress, int target) = QuestEngine.ProgressOf(hero, quest);
            string state = progress >= target ? "GOTOWE – oddaj u: " + RegionCatalog.GiverName(quest.Giver) : $"{progress}/{target}";
            io.WriteLine($"{(quest.IsMain ? "[główne] " : "")}{quest.Title} – {quest.Objective.Describe()} – {state}", progress >= target ? ConsoleColor.Green : ConsoleColor.Yellow);
        }

        var done = hero.Quests.Where(q => q.Value.Status == QuestStatus.Completed).Select(q => QuestCatalog.Get(q.Key).Title).ToList();
        if (done.Count > 0)
        {
            io.WriteLine("Ukończone: " + string.Join(", ", done), ConsoleColor.DarkGray);
        }

        io.WriteLine($"Etap wyprawy: {Story.StageName(hero.Stage)}.", ConsoleColor.Cyan);
        io.PressAnyKey();
        io.Clear();
    }
}
