namespace PyramidTreasureConsoleRPG.Tests;

public class TravelEngineTests
{
    [Fact]
    public void Travel_IsGatedByStageGoldAndLevel()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        Assert.False(TravelEngine.CanTravel(hero, RegionCatalog.Port).Allowed);
        Assert.False(TravelEngine.CanTravel(hero, RegionCatalog.OldTown).Allowed);
        hero.Stage = StoryStage.BanditsCalmed;
        Assert.True(TravelEngine.CanTravel(hero, RegionCatalog.OldTown).Allowed);
        hero.Gold = 0;
        Assert.False(TravelEngine.CanTravel(hero, RegionCatalog.Delta).Allowed);
        hero.Gold = 100;
        hero.Stage = StoryStage.CaravanReady;
        Assert.False(TravelEngine.CanTravel(hero, RegionCatalog.Pyramid).Allowed);
        hero.AddExp(1_000_000);
        Assert.True(TravelEngine.CanTravel(hero, RegionCatalog.Pyramid).Allowed);
    }

    [Fact]
    public void Depart_ChargesGold_AndArriveMovesHeroAndAddsDays()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Stage = StoryStage.BanditsCalmed;
        hero.Gold = 50;
        TravelPlan plan = TravelEngine.Depart(hero, RegionCatalog.Delta, new SeededRandomSource(3));
        Assert.Equal(40, hero.Gold);
        Assert.Equal(RegionCatalog.Delta.TravelDays - 1, plan.Steps.Count);
        Assert.All(plan.Steps, s => Assert.True(s.Kind != TravelStepKind.Fight || s.Enemies!.Count > 0));
        TravelEngine.Arrive(hero, plan.Target);
        Assert.Equal(RegionId.Delta, hero.CurrentRegion);
        Assert.Equal(1 + RegionCatalog.Delta.TravelDays, hero.Day);
        Assert.True(hero.HasFlag(QuestEngine.ReachedFlag(RegionId.Delta)));
    }

    [Fact]
    public void Explore_ProducesFightsEventsAndQuietDays()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        var kinds = new HashSet<TravelStepKind>();
        var rng = new SeededRandomSource(11);
        for (int i = 0; i < 100; i++)
        {
            kinds.Add(TravelEngine.Explore(hero, RegionCatalog.Port, rng, out _).Kind);
        }

        Assert.Equal(101, hero.Day);
        Assert.Contains(TravelStepKind.Fight, kinds);
        Assert.Contains(TravelStepKind.Event, kinds);
    }
}
