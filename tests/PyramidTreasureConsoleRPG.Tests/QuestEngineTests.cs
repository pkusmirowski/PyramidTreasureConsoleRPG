namespace PyramidTreasureConsoleRPG.Tests;

public class QuestEngineTests
{
    [Fact]
    public void MainLine_IsAvailableInOrder_AndUnlocksStages()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        Assert.Equal([QuestId.Bandits], QuestEngine.Available(hero, QuestGiverId.Barman).Select(q => q.Id));
        Assert.True(QuestEngine.HasNews(hero, QuestGiverId.Barman));

        QuestEngine.Accept(hero, QuestId.Bandits);
        Assert.Empty(QuestEngine.Available(hero, QuestGiverId.Barman));
        Assert.False(QuestEngine.IsObjectiveMet(hero, QuestCatalog.Get(QuestId.Bandits)));

        for (int i = 0; i < 6; i++)
        {
            var updates = QuestEngine.OnEnemyKilled(hero, EnemyCatalog.Thief);
            Assert.Single(updates);
            Assert.Equal(i == 5, updates[0].JustCompleted);
        }

        Assert.Empty(QuestEngine.OnEnemyKilled(hero, EnemyCatalog.Thief));
        Assert.Single(QuestEngine.ReadyToTurnIn(hero, QuestGiverId.Barman));

        int gold = hero.Gold;
        QuestTurnInResult result = QuestEngine.TurnIn(hero, QuestId.Bandits);
        Assert.Equal(StoryStage.BanditsCalmed, result.NewStage);
        Assert.Equal(StoryStage.BanditsCalmed, hero.Stage);
        Assert.Equal(gold + 40, hero.Gold);
        Assert.Equal(10, hero.GetReputation(Faction.Town));
        Assert.Equal(QuestStatus.Completed, hero.GetQuest(QuestId.Bandits)!.Status);
        Assert.Equal([QuestId.Wolves], QuestEngine.Available(hero, QuestGiverId.Barman).Select(q => q.Id));
        Assert.Throws<InvalidOperationException>(() => QuestEngine.TurnIn(hero, QuestId.Bandits));
    }

    [Fact]
    public void MainLine_ReachesCaravanReady()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        foreach (QuestDefinition quest in QuestCatalog.All.Where(q => q.IsMain))
        {
            Assert.True(QuestEngine.IsAvailable(hero, quest), quest.Title);
            QuestEngine.Accept(hero, quest.Id);
            switch (quest.Objective)
            {
                case KillObjective kill:
                    for (int i = 0; i < kill.Count; i++)
                    {
                        QuestEngine.OnEnemyKilled(hero, kill.Enemy);
                    }

                    break;
                case ReachObjective reach:
                    QuestEngine.OnArrived(hero, reach.Region);
                    break;
                case FlagObjective flag:
                    hero.SetFlag(flag.Flag);
                    break;
            }

            QuestEngine.TurnIn(hero, quest.Id);
        }

        Assert.Equal(StoryStage.CaravanReady, hero.Stage);
    }

    [Fact]
    public void ReachObjective_CompletesOnArrival()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        hero.Stage = StoryStage.BanditsCalmed;
        Assert.Contains(QuestEngine.Available(hero, QuestGiverId.Smuggler), q => q.Id == QuestId.SmugglerCargo);
        QuestEngine.Accept(hero, QuestId.SmugglerCargo);
        Assert.False(QuestEngine.IsObjectiveMet(hero, QuestCatalog.Get(QuestId.SmugglerCargo)));
        TravelEngine.Arrive(hero, RegionCatalog.Delta);
        Assert.True(QuestEngine.IsObjectiveMet(hero, QuestCatalog.Get(QuestId.SmugglerCargo)));
        Assert.Equal(RegionId.Delta, hero.CurrentRegion);
    }

    [Fact]
    public void FactionRequirement_HidesQuest()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        hero.Stage = StoryStage.BanditsCalmed;
        hero.AdjustReputation(Faction.Underworld, -50);
        Assert.DoesNotContain(QuestEngine.Available(hero, QuestGiverId.Smuggler), q => q.Id == QuestId.SmugglerCargo);
    }
}
