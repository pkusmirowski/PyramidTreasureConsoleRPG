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
                var group = Encounters.InRegion(region, rng);
                Assert.NotEmpty(group);
                Assert.All(group, e => Assert.True(e.IsAlive));
            }
        }

        Assert.Empty(Encounters.InRegion(RegionCatalog.Pyramid, rng));
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
