namespace PyramidTreasureConsoleRPG.Domain;

public enum QuestId
{
    Bandits,
    Wolves,
    CaravanTrail,
    Oasis,
    CaptainSmugglers,
    SmugglerCargo,
    PriestessMonks,
}

public enum QuestStatus
{
    Active,
    Completed,
}

public abstract record QuestObjective
{
    public abstract string Describe();
}

public sealed record KillObjective(EnemyDefinition Enemy, int Count) : QuestObjective
{
    public override string Describe() => $"Pokonaj: {Enemy.Name} x{Count}";
}

public sealed record ReachObjective(RegionId Region) : QuestObjective
{
    public override string Describe() => $"Dotrzyj do: {RegionCatalog.Get(Region).Name}";
}

public sealed record FlagObjective(string Flag, string Description) : QuestObjective
{
    public override string Describe() => Description;
}

public sealed record QuestReward(
    int Gold,
    int Exp,
    Faction? Faction = null,
    int Reputation = 0,
    StoryStage? UnlocksStage = null,
    string? SetsFlag = null);

public sealed record QuestDefinition(
    QuestId Id,
    QuestGiverId Giver,
    string Title,
    IReadOnlyList<string> Intro,
    QuestObjective Objective,
    QuestReward Reward,
    IReadOnlyList<string> Completion,
    StoryStage RequiredStage = StoryStage.Start,
    QuestId? RequiresCompleted = null,
    Faction? RequiresFaction = null,
    int MinReputation = -100,
    bool IsMain = false);

/// <summary>Postęp zadania u bohatera (zapisywany).</summary>
public sealed class QuestProgress
{
    public QuestStatus Status { get; set; } = QuestStatus.Active;

    public int Progress { get; set; }
}
