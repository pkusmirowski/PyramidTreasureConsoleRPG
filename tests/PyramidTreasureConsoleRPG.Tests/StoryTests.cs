namespace PyramidTreasureConsoleRPG.Tests;

public class StoryTests
{
    [Fact]
    public void Pyramid_RequiresMaxLevelAndCaravan()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        hero.AddExp(1_000_000);
        hero.Stage = StoryStage.CaravanAnnounced;
        Assert.False(Story.CanEnterPyramid(hero));
        hero.Stage = StoryStage.CaravanReady;
        Assert.True(Story.CanEnterPyramid(hero));
    }

    [Fact]
    public void Encounters_EveryFightingRegionSpawnsLivingEnemies()
    {
        var rng = new SeededRandomSource(1);
        foreach (RegionDefinition region in RegionCatalog.All.Where(r => !r.IsFinal))
        {
            for (int i = 0; i < 10; i++)
            {
                var group = Encounters.InRegion(region, rng, heroLevel: 20);
                Assert.NotEmpty(group);
                Assert.All(group, e => Assert.True(e.IsAlive));
            }
        }

        Assert.Empty(Encounters.InRegion(RegionCatalog.Pyramid, rng, heroLevel: 20));
    }

    [Fact]
    public void Encounters_UnlockLargerGroupsWithLevel()
    {
        RegionDefinition delta = RegionCatalog.Delta;
        Assert.Equal(1, Encounters.UnlockedGroups(delta, delta.RecommendedLevel - 2));
        Assert.Equal(1, Encounters.UnlockedGroups(delta, delta.RecommendedLevel));
        Assert.Equal(2, Encounters.UnlockedGroups(delta, delta.RecommendedLevel + 1));
        Assert.Equal(3, Encounters.UnlockedGroups(delta, delta.RecommendedLevel + 3));

        var rng = new SeededRandomSource(7);
        for (int i = 0; i < 30; i++)
        {
            List<Enemy> group = Encounters.InRegion(delta, rng, delta.RecommendedLevel);
            Assert.Equal(delta.Encounters[0].Length, group.Count);
            Assert.All(group, e => Assert.Contains(e.Definition, delta.Encounters[0]));
        }

        int largest = delta.Encounters[^1].Length;
        Assert.Contains(largest, Enumerable.Range(0, 60).Select(_ => Encounters.InRegion(delta, rng, 20).Count));
    }

    [Fact]
    public void Reputation_IsClampedAndNamed()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        Assert.Equal(100, hero.AdjustReputation(Faction.Town, 500));
        Assert.Equal(-100, hero.AdjustReputation(Faction.Underworld, -500));
        Assert.Equal("bohater", Story.ReputationName(100));
        Assert.Equal("wróg", Story.ReputationName(-100));
        Assert.Equal("obcy", Story.ReputationName(0));
    }
}
