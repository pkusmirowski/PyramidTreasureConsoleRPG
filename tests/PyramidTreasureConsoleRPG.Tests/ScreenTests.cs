namespace PyramidTreasureConsoleRPG.Tests;

public class ScreenTests
{
    [Fact]
    public void ShopScreen_WithoutGold_ShowsInsultAndSellsNothing()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 0;
        var io = new ScriptedGameIO("3", "4");
        new ShopScreen(io, new SeededRandomSource(1)).Run(hero);

        Assert.Empty(hero.Inventory);
        Assert.Equal(0, hero.Gold);
        Assert.Contains(io.Output, line => line.Contains("złota", StringComparison.Ordinal));
        Assert.Contains("Wychodzisz ze sklepu...", io.Output);
    }

    [Fact]
    public void ShopScreen_WithGold_SellsPotion()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 100;
        var io = new ScriptedGameIO("1", "4");
        new ShopScreen(io, new SeededRandomSource(1)).Run(hero);

        Assert.Equal(1, hero.CountPotions(PotionKind.Small));
        Assert.Equal(80, hero.Gold);
    }

    [Fact]
    public void CombatScreen_PlaysWholeFightThroughEvents()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.AddExp(1_000_000);
        var answers = Enumerable.Repeat("1", 50).ToArray();
        var io = new ScriptedGameIO(answers);
        CombatStatus status = new CombatScreen(io, new SeededRandomSource(9)).Run(hero, new[] { new Thief() });

        Assert.Equal(CombatStatus.Victory, status);
        Assert.Contains(io.Output, line => line.StartsWith("Pokonałeś:", StringComparison.Ordinal));
        Assert.Contains("Twoja tura:", io.MenusShown);
    }

    [Fact]
    public void BarScreen_TalkingToBarman_AdvancesStory()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        var io = new ScriptedGameIO("1", "1", "4");
        new BarScreen(io, new SeededRandomSource(1)).Run(hero);

        Assert.Equal(StoryStage.BanditsCalmed, hero.Stage);
        Assert.Contains(Dialogues.NoNews, io.Output);
    }
}
