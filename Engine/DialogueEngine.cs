namespace PyramidTreasureConsoleRPG.Engine;

public sealed record DialogueResult(string? Text, IReadOnlyList<string> Notes, DialogueNode? Next, EnemyDefinition[]? Fight);

/// <summary>Rozmowy z NPC: wybór węzła wejściowego po flagach, opcje z wymaganiami, skutki jak w zdarzeniach.</summary>
public static class DialogueEngine
{
    public static DialogueNode Start(Hero hero, NpcDefinition npc)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(npc);
        foreach ((string? requires, string? forbids, string nodeId) in npc.Entry)
        {
            if ((requires is null || hero.HasFlag(requires)) && (forbids is null || !hero.HasFlag(forbids)))
            {
                return npc.Node(nodeId);
            }
        }

        return npc.Node(npc.Entry[^1].NodeId);
    }

    public static ChoiceAvailability Availability(Hero hero, DialogueOption option)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(option);
        if (option.RequiresFlag is not null && !hero.HasFlag(option.RequiresFlag))
        {
            return new ChoiceAvailability(false, "jeszcze nie");
        }

        if (option.GoldCost > 0 && hero.Gold < option.GoldCost)
        {
            return new ChoiceAvailability(false, $"brak złota ({option.GoldCost} g)");
        }

        if (option.RequiresFaction is Faction faction && hero.GetReputation(faction) < option.RequiredReputation)
        {
            return new ChoiceAvailability(false, $"wymaga reputacji {RegionCatalog.FactionName(faction)} ≥ {option.RequiredReputation}");
        }

        return new ChoiceAvailability(true, null);
    }

    public static DialogueResult Choose(Hero hero, NpcDefinition npc, DialogueNode node, int optionIndex)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(npc);
        ArgumentNullException.ThrowIfNull(node);
        DialogueOption option = node.Options[optionIndex];
        if (!Availability(hero, option).Available)
        {
            throw new InvalidOperationException("Ta opcja nie jest dostępna.");
        }

        var notes = new List<string>();
        if (option.GoldCost > 0)
        {
            hero.Gold -= option.GoldCost;
            notes.Add($"−{option.GoldCost} złota");
        }

        EnemyDefinition[]? fight = EventEngine.ApplyAll(hero, option.Effects, notes);
        DialogueNode? next = option.Next is null ? null : npc.Node(option.Next);
        return new DialogueResult(option.ResultText, notes, next, fight);
    }
}
