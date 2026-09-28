namespace PyramidTreasureConsoleRPG.Tests;

public class ScreenTests
{
    [Fact]
    public void ShopScreen_WithoutGold_ShowsInsultAndSellsNothing()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 0;
        // 1: mikstury, 3: duża (brak złota), 4: wróć, 4: wyjdź
        var io = new ScriptedGameIO("1", "3", "4", "4");
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
        hero.Gold = 300;
        // 1: mikstury, 1: mała, 4: wróć, 2: wyposażenie, 1: topór, 4+: wróć... stock Portu ma 6 pozycji, więc "Wróć" = 7; 4: wyjdź
        var io = new ScriptedGameIO("1", "1", "4", "2", "1", "7", "4");
        new ShopScreen(io, new SeededRandomSource(1)).Run(hero);

        Assert.Equal(1, hero.CountPotions(PotionKind.Small));
        Assert.Single(hero.Gear, g => g.Id == ItemId.MercenaryAxe);
        Assert.Equal(300 - 20 - 120, hero.Gold);
    }

    [Fact]
    public void CombatScreen_PlaysWholeFightThroughEvents()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.AddExp(1_000_000);
        var answers = Enumerable.Repeat("1", 50).ToArray();
        var io = new ScriptedGameIO(answers);
        CombatStatus status = new CombatScreen(io, new SeededRandomSource(9)).Run(hero, new[] { EnemyCatalog.Thief.Spawn() });

        Assert.Equal(CombatStatus.Victory, status);
        Assert.Contains(io.Output, line => line.StartsWith("Pokonałeś:", StringComparison.Ordinal));
        Assert.Contains("Twoja tura:", io.MenusShown);
    }

    [Fact]
    public void BarScreen_TalkingToBarman_OffersAndAcceptsMainQuest()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        var rng = new SeededRandomSource(1);
        // 1: pogadaj -> zleceniodawca; 1: przyjmij Bandyci; 1: tak; 2: odejdź; 4: odejdź od baru
        var io = new ScriptedGameIO("1", "1", "1", "2", "4");
        new BarScreen(io, rng, new QuestGiverScreen(io, rng)).Run(hero);

        Assert.Equal(QuestStatus.Active, hero.GetQuest(QuestId.Bandits)?.Status);
        Assert.Contains(io.Output, line => line.StartsWith("Przyjęto zadanie", StringComparison.Ordinal));
    }

    [Fact]
    public void EventScreen_ResolvesChoice_AndRunsResultingFight()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.AddExp(1_000_000);
        GameEvent customs = EventCatalog.All.First(e => e.Id == "port_customs");
        var answers = new List<string> { "2" };
        answers.AddRange(Enumerable.Repeat("1", 40));
        var io = new ScriptedGameIO([.. answers]);
        var rng = new SeededRandomSource(4);
        CombatStatus? status = new EventScreen(io, rng, new CombatScreen(io, rng)).Run(hero, customs);

        Assert.Equal(CombatStatus.Victory, status);
        Assert.Contains("Celnik", io.Output);
    }
}
