namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Rozmowa z postacią z wątkiem. Sceny intymne za zamkniętymi drzwiami; liczą się konsekwencje.</summary>
public sealed class NpcScreen(IGameIO io, CombatScreen combat)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly CombatScreen combat = combat ?? throw new ArgumentNullException(nameof(combat));

    /// <summary>Zwraca status walki, jeśli rozmowa do niej doprowadziła.</summary>
    public CombatStatus? Talk(Hero hero, NpcDefinition npc)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(npc);
        DialogueNode? node = DialogueEngine.Start(hero, npc);
        io.Header(npc.Name);
        io.WriteLine(npc.Description, ConsoleColor.DarkGray);
        while (node is not null)
        {
            io.Narrate(node.Text, ConsoleColor.White);
            var labels = node.Options.Select(o =>
            {
                ChoiceAvailability availability = DialogueEngine.Availability(hero, o);
                return availability.Available ? o.Text : $"{o.Text} – niedostępne: {availability.Reason}";
            }).ToArray();

            int index;
            while (true)
            {
                index = io.Menu("Co mówisz?", labels) - 1;
                if (DialogueEngine.Availability(hero, node.Options[index]).Available)
                {
                    break;
                }

                io.ShowError("Ta opcja jest niedostępna.");
            }

            io.Clear();
            DialogueResult result = DialogueEngine.Choose(hero, npc, node, index);
            if (result.Text is not null)
            {
                io.Narrate([result.Text], ConsoleColor.Magenta);
            }

            foreach (string note in result.Notes)
            {
                io.WriteLine(note, ConsoleColor.Cyan);
            }

            if (result.Fight is not null)
            {
                io.Pause(900);
                return combat.Run(hero, result.Fight.Select(d => d.Spawn()));
            }

            node = result.Next;
        }

        io.PressAnyKey();
        return null;
    }
}
