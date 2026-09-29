namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Rozmowa ze zleceniodawcą: oddawanie ukończonych zadań, przyjmowanie nowych, stan aktywnych.</summary>
public sealed class QuestGiverScreen(IGameIO io, IRandomSource rng, GameSettings settings)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly GameSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));

    public void Run(Hero hero, QuestGiverId giver)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.Header(RegionCatalog.GiverName(giver));
            IReadOnlyList<QuestDefinition> ready = QuestEngine.ReadyToTurnIn(hero, giver);
            IReadOnlyList<QuestDefinition> available = QuestEngine.Available(hero, giver);
            IReadOnlyList<QuestDefinition> active = [.. QuestEngine.Active(hero).Where(q => q.Giver == giver && !ready.Contains(q))];

            var options = new List<(string Label, Action Action)>();
            foreach (QuestDefinition quest in ready)
            {
                options.Add(($"Oddaj: {quest.Title} (nagroda {quest.Reward.Gold} g, {quest.Reward.Exp} dośw.)", () => TurnIn(hero, quest)));
            }

            foreach (QuestDefinition quest in available)
            {
                options.Add(($"Przyjmij: {quest.Title}", () => Accept(hero, quest)));
            }

            foreach (QuestDefinition quest in active)
            {
                (int progress, int target) = QuestEngine.ProgressOf(hero, quest);
                options.Add(($"W toku: {quest.Title} ({progress}/{target}) – {quest.Objective.Describe()}", () => io.ShowInfo("\"Wracaj, jak skończysz.\"")));
            }

            if (options.Count == 0)
            {
                io.ShowInfo(giver == QuestGiverId.Barman ? Dialogues.BarmanSmallTalk(hero, rng, settings.ProfanityEnabled) : "\"Nie mam dla ciebie nic więcej. Na razie.\"");
                return;
            }

            options.Add(("Odejdź", () => { }));
            int choice = io.Menu("Rozmowa:", [.. options.Select(o => o.Label)]);
            io.Clear();
            if (choice == options.Count)
            {
                return;
            }

            options[choice - 1].Action();
        }
    }

    private void Accept(Hero hero, QuestDefinition quest)
    {
        io.Narrate(quest.Intro);
        io.ShowInfo($"Cel: {quest.Objective.Describe()}");
        if (io.Menu("Przyjąć zadanie?", "Tak", "Nie") == 1)
        {
            QuestEngine.Accept(hero, quest.Id);
            io.ShowSuccess($"Przyjęto zadanie: {quest.Title}.");
        }

        io.Clear();
    }

    private void TurnIn(Hero hero, QuestDefinition quest)
    {
        QuestTurnInResult result = QuestEngine.TurnIn(hero, quest.Id);
        io.Narrate(quest.Completion);
        io.ShowSuccess($"Zadanie ukończone: {quest.Title}. +{result.Reward.Gold} złota, +{result.Reward.Exp} doświadczenia"
            + (result.Reward.Faction is Faction faction ? $", reputacja {RegionCatalog.FactionName(faction)} {result.Reward.Reputation:+0;-0}" : "") + ".");
        if (result.LevelsGained > 0)
        {
            io.WriteLine($"Awans! Masz teraz poziom {hero.Level}.", ConsoleColor.DarkYellow);
        }

        if (result.NewStage is StoryStage stage)
        {
            io.WriteLine($"Nowy etap wyprawy: {Story.StageName(stage)}. Sprawdź mapę.", ConsoleColor.Magenta);
        }

        io.PressAnyKey();
        io.Clear();
    }
}
