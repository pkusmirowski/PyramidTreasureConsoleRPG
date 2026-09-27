namespace PyramidTreasureConsoleRPG.Tests;

public class StoryTests
{
    [Fact]
    public void FreshHero_CannotTravelUntilBarmanTalk()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        Assert.False(Story.CanTravel(hero));
        Assert.True(Story.BarmanHasNews(hero));
        Assert.Equal(StoryStage.BanditsCalmed, Story.AdvanceByBarman(hero));
        Assert.True(Story.CanTravel(hero));
        Assert.False(Story.BarmanHasNews(hero));
        Assert.Null(Story.AdvanceByBarman(hero));
    }

    [Theory]
    [InlineData(1, StoryStage.BanditsCalmed)]
    [InlineData(4, StoryStage.BanditsCalmed)]
    [InlineData(5, StoryStage.WolvesCleared)]
    [InlineData(10, StoryStage.CaravanAnnounced)]
    [InlineData(19, StoryStage.CaravanAnnounced)]
    [InlineData(20, StoryStage.CaravanReady)]
    public void RequiredStage_FollowsLevelBrackets(int level, StoryStage expected)
    {
        Assert.Equal(expected, Story.RequiredStageForTravel(level));
    }

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
    public void Encounters_CoverEveryLevelWithAtLeastOneEnemy()
    {
        var rng = new SeededRandomSource(1);
        for (int level = 1; level <= CombatMath.MaxLevel; level++)
        {
            var group = Encounters.ForLevel(level, rng);
            Assert.NotEmpty(group);
            Assert.All(group, e => Assert.True(e.IsAlive));
        }
    }
}
