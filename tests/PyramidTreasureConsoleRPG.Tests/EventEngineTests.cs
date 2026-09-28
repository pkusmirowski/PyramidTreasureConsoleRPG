namespace PyramidTreasureConsoleRPG.Tests;

public class EventEngineTests
{
    private static GameEvent Find(string id) => EventCatalog.All.First(e => e.Id == id);

    [Fact]
    public void SkillCheck_UsesStatPlusD20_AndFailureEffects()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test"); // Str 4
        GameEvent brawl = Find("port_brawl");
        // choice 0: Str check 13; d20 roll 5 -> 9 < 13 -> failure -> -12% HP
        EventResult failed = EventEngine.Resolve(hero, brawl, 0, new ScriptedRandomSource(5));
        Assert.False(failed.Success);
        Assert.Equal(9, failed.Roll);
        Assert.True(hero.Hp < hero.MaxHp);
        Assert.Equal(0, hero.GetReputation(Faction.Town));

        hero.FullHeal();
        EventResult passed = EventEngine.Resolve(hero, brawl, 0, new ScriptedRandomSource(20));
        Assert.True(passed.Success);
        Assert.Equal(10, hero.GetReputation(Faction.Town));
        Assert.Contains(passed.Notes, n => n.Contains("sukces", StringComparison.Ordinal));
    }

    [Fact]
    public void GoldCost_IsCheckedAndDeducted()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 10;
        GameEvent customs = Find("port_customs");
        Assert.False(EventEngine.Availability(hero, customs.Choices[0]).Available);
        Assert.Throws<InvalidOperationException>(() => EventEngine.Resolve(hero, customs, 0, new SeededRandomSource(1)));

        hero.Gold = 30;
        EventResult result = EventEngine.Resolve(hero, customs, 0, new SeededRandomSource(1));
        Assert.Equal(10, hero.Gold);
        Assert.Null(result.Fight);
        Assert.Contains("−20 złota", result.Notes);
    }

    [Fact]
    public void FightEffect_IsReturnedNotResolved()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        EventResult result = EventEngine.Resolve(hero, Find("port_customs"), 1, new SeededRandomSource(1));
        Assert.NotNull(result.Fight);
        Assert.Equal(2, result.Fight.Length);
        Assert.Equal(hero.MaxHp, hero.Hp);
    }

    [Fact]
    public void OncePerGame_EventIsNotPickedTwice()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        GameEvent nomads = Find("desert_nomads");
        Assert.True(EventEngine.IsEligible(hero, nomads));
        EventEngine.Resolve(hero, nomads, 2, new SeededRandomSource(1));
        Assert.False(EventEngine.IsEligible(hero, nomads));
        for (int i = 0; i < 50; i++)
        {
            GameEvent? picked = EventEngine.Pick(hero, RegionId.Desert, new SeededRandomSource(i));
            Assert.NotNull(picked);
            Assert.NotEqual("desert_nomads", picked.Id);
        }
    }

    [Fact]
    public void RequiresFlag_GatesEvent()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        GameEvent fence = Find("oldtown_fence");
        Assert.False(EventEngine.IsEligible(hero, fence));
        hero.SetFlag("rumor:oldtown");
        Assert.True(EventEngine.IsEligible(hero, fence));
    }

    [Fact]
    public void HpDamage_NeverKills()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        hero.TakeDamage(hero.MaxHp - 2);
        // desert_mirage choice 0: -10% HP
        EventEngine.Resolve(hero, Find("desert_mirage"), 0, new SeededRandomSource(1));
        Assert.Equal(1, hero.Hp);
        Assert.True(hero.IsAlive);
    }

    [Fact]
    public void ReputationRequirement_BlocksChoice()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        GameEvent patrol = Find("desert_templar_patrol");
        Assert.False(EventEngine.Availability(hero, patrol.Choices[2]).Available);
        hero.AdjustReputation(Faction.Brotherhood, 20);
        Assert.True(EventEngine.Availability(hero, patrol.Choices[2]).Available);
    }
}
