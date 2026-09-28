namespace PyramidTreasureConsoleRPG.Tests;

public class ScreenTests
{
    [Fact]
    public void ShopScreen_WithoutGold_ShowsInsultAndSellsNothing()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Gold = 0;
        // 1: mikstury, 3: duża (brak złota), 5: wróć (Port ma 4 pozycje), 4: wyjdź
        var io = new ScriptedGameIO("1", "3", "5", "4");
        new ShopScreen(io, new SeededRandomSource(1), new GameSettings()).Run(hero);

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
        // 1: mikstury, 1: mała, 5: wróć, 2: wyposażenie, 1: topór, 7: wróć (stock Portu ma 6 pozycji), 4: wyjdź
        var io = new ScriptedGameIO("1", "1", "5", "2", "1", "7", "4");
        new ShopScreen(io, new SeededRandomSource(1), new GameSettings()).Run(hero);

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
        new BarScreen(io, rng, new QuestGiverScreen(io, rng, new GameSettings()), new GameSettings()).Run(hero);

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
        answers.AddRange(Enumerable.Repeat("1", 80));
        var io = new ScriptedGameIO([.. answers]);
        var rng = new SeededRandomSource(4);
        CombatStatus? status = new EventScreen(io, rng, new CombatScreen(io, rng)).Run(hero, customs);

        Assert.Equal(CombatStatus.Victory, status);
        Assert.Contains("Celnik", io.Output);
    }

    [Fact]
    public void ShopScreen_SellsGearFromBag()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        Item axe = ItemCatalog.Get(ItemId.MercenaryAxe);
        hero.AddGear(axe);
        int gold = hero.Gold;
        // 3: sprzedaj, 1: topór, (torba pusta -> powrót), 4: wyjdź
        var io = new ScriptedGameIO("3", "1", "4");
        new ShopScreen(io, new SeededRandomSource(1), new GameSettings()).Run(hero);

        Assert.Empty(hero.Gear);
        Assert.Equal(gold + axe.SellPrice, hero.Gold);
        Assert.Contains(io.Output, line => line.StartsWith("Sprzedano: Topór najemnika", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("Torba jest pusta", StringComparison.Ordinal));
    }

    [Fact]
    public void InventoryScreen_DrinksWhiskyAtFullHealth_AndLotusAddicts()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        hero.AddPotion(Potion.Small);
        hero.AddPotion(Potion.Whisky);
        hero.AddPotion(Potion.Lotus);
        // Przy pełnym HP lista to tylko używki: 1 whisky, 2 lotos. 1,1: whisky; 1,1: lotos (już pierwszy); 3: wyjdź
        var io = new ScriptedGameIO("1", "1", "1", "1", "3");
        new InventoryScreen(io).Run(hero);

        Assert.Contains(io.Output, line => line.StartsWith("Whisky pali", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.StartsWith("Dym lotosu", StringComparison.Ordinal));
        Assert.Equal(PotionKind.Lotus, hero.NextFightBuff);
        Assert.Equal(1, hero.Addiction);
        Assert.Equal(1, hero.CountPotions(PotionKind.Small));
        Assert.Equal(0, hero.CountPotions(PotionKind.Whisky));
    }

    [Fact]
    public void EndingScreen_BrotherhoodAndAshEndings()
    {
        Hero vowed = Hero.Create(HeroClass.Archer, "Test");
        vowed.SetFlag("neferet:vow");
        var io = new ScriptedGameIO("2");
        Assert.Equal(EndingKind.GiveToBrotherhood, new EndingScreen(io).Run(vowed));
        Assert.Contains(io.Output, line => line.Contains("OFIARA BRACTWA", StringComparison.Ordinal));
        Assert.True(vowed.HasFlag("ending:GiveToBrotherhood"));

        Hero mapped = Hero.Create(HeroClass.Warrior, "Test");
        mapped.SetFlag("map:nomads");
        var io2 = new ScriptedGameIO("3");
        Assert.Equal(EndingKind.Destroy, new EndingScreen(io2).Run(mapped));
        Assert.Contains(io2.Output, line => line.Contains("POPIÓŁ POD TRONEM", StringComparison.Ordinal));
        Assert.Contains(io2.Output, line => line.Contains("Podsumowanie wyprawy: Popiół pod tronem", StringComparison.Ordinal));
    }
}
