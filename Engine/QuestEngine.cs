namespace PyramidTreasureConsoleRPG.Engine;

public sealed record QuestUpdate(QuestDefinition Quest, int Progress, int Target, bool JustCompleted);

public sealed record QuestTurnInResult(QuestDefinition Quest, QuestReward Reward, int LevelsGained, StoryStage? NewStage);

/// <summary>Zadania: dostępność, przyjmowanie, śledzenie postępu i oddawanie.</summary>
public static class QuestEngine
{
    public static string ReachedFlag(RegionId region) => $"reached:{region}";

    public static IReadOnlyList<QuestDefinition> Available(Hero hero, QuestGiverId giver)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return QuestCatalog.ByGiver(giver).Where(q => IsAvailable(hero, q)).ToList();
    }

    public static bool IsAvailable(Hero hero, QuestDefinition quest)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(quest);
        if (hero.GetQuest(quest.Id) is not null || hero.Stage < quest.RequiredStage)
        {
            return false;
        }

        if (quest.RequiresCompleted is QuestId required && hero.GetQuest(required)?.Status != QuestStatus.Completed)
        {
            return false;
        }

        return quest.RequiresFaction is not Faction faction || hero.GetReputation(faction) >= quest.MinReputation;
    }

    public static IReadOnlyList<QuestDefinition> Active(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return hero.Quests.Where(kv => kv.Value.Status == QuestStatus.Active).Select(kv => QuestCatalog.Get(kv.Key)).ToList();
    }

    public static IReadOnlyList<QuestDefinition> ReadyToTurnIn(Hero hero, QuestGiverId giver)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return Active(hero).Where(q => q.Giver == giver && IsObjectiveMet(hero, q)).ToList();
    }

    public static bool HasNews(Hero hero, QuestGiverId giver) => Available(hero, giver).Count > 0 || ReadyToTurnIn(hero, giver).Count > 0;

    public static void Accept(Hero hero, QuestId id)
    {
        ArgumentNullException.ThrowIfNull(hero);
        QuestDefinition quest = QuestCatalog.Get(id);
        if (!IsAvailable(hero, quest))
        {
            throw new InvalidOperationException("Zadanie nie jest dostępne.");
        }

        hero.StartQuest(id);
    }

    public static (int Progress, int Target) ProgressOf(Hero hero, QuestDefinition quest)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(quest);
        QuestProgress? progress = hero.GetQuest(quest.Id);
        return quest.Objective switch
        {
            KillObjective kill => (Math.Min(progress?.Progress ?? 0, kill.Count), kill.Count),
            ReachObjective reach => (hero.HasFlag(ReachedFlag(reach.Region)) ? 1 : 0, 1),
            FlagObjective flag => (hero.HasFlag(flag.Flag) ? 1 : 0, 1),
            _ => (0, 1),
        };
    }

    public static bool IsObjectiveMet(Hero hero, QuestDefinition quest)
    {
        (int progress, int target) = ProgressOf(hero, quest);
        return progress >= target;
    }

    /// <summary>Po zabiciu wroga: zwiększa liczniki aktywnych zadań na ten typ wroga.</summary>
    public static IReadOnlyList<QuestUpdate> OnEnemyKilled(Hero hero, EnemyDefinition enemy)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(enemy);
        var updates = new List<QuestUpdate>();
        foreach (QuestDefinition quest in Active(hero))
        {
            if (quest.Objective is not KillObjective kill || kill.Enemy != enemy)
            {
                continue;
            }

            QuestProgress progress = hero.GetQuest(quest.Id)!;
            if (progress.Progress >= kill.Count)
            {
                continue;
            }

            progress.Progress++;
            updates.Add(new QuestUpdate(quest, progress.Progress, kill.Count, progress.Progress >= kill.Count));
        }

        return updates;
    }

    /// <summary>Po dotarciu do regionu: ustawia flagę dojścia (cele typu „dotrzyj do”).</summary>
    public static void OnArrived(Hero hero, RegionId region)
    {
        ArgumentNullException.ThrowIfNull(hero);
        hero.SetFlag(ReachedFlag(region));
    }

    public static QuestTurnInResult TurnIn(Hero hero, QuestId id)
    {
        ArgumentNullException.ThrowIfNull(hero);
        QuestDefinition quest = QuestCatalog.Get(id);
        QuestProgress progress = hero.GetQuest(id) ?? throw new InvalidOperationException("Zadanie nie jest aktywne.");
        if (progress.Status != QuestStatus.Active || !IsObjectiveMet(hero, quest))
        {
            throw new InvalidOperationException("Zadanie nie jest gotowe do oddania.");
        }

        progress.Status = QuestStatus.Completed;
        hero.SetFlag($"quest:{quest.Id}");
        QuestReward reward = quest.Reward;
        hero.Gold += reward.Gold;
        int levels = hero.AddExp(reward.Exp);
        if (reward.Faction is Faction faction)
        {
            hero.AdjustReputation(faction, reward.Reputation);
        }

        StoryStage? newStage = null;
        if (reward.UnlocksStage is StoryStage stage && stage > hero.Stage)
        {
            hero.Stage = stage;
            newStage = stage;
        }

        if (reward.SetsFlag is not null)
        {
            hero.SetFlag(reward.SetsFlag);
        }

        return new QuestTurnInResult(quest, reward, levels, newStage);
    }
}
