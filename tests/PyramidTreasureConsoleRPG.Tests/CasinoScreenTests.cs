namespace PyramidTreasureConsoleRPG.Tests;

public class CasinoScreenTests
{
    private static Hero RichHero()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Gracz");
        hero.Gold = 100;
        return hero;
    }

    private static void AssertBooksBalance(Hero hero, int startGold)
    {
        Assert.Equal(hero.Stats.CasinoWon - hero.Stats.CasinoLost, hero.Gold - startGold);
        Assert.True(hero.Stats.CasinoWon + hero.Stats.CasinoLost > 0, "stawka musi trafić do statystyk");
    }

    [Fact]
    public void Roulette_OneRoundOnBlack_UpdatesGoldAndStats()
    {
        Hero hero = RichHero();
        // 1: ruletka, 1: stawka, 1: czarne, 2: nie graj dalej, 6: wyjdź
        var io = new ScriptedGameIO("1", "1", "1", "2", "6");
        new CasinoScreen(io, new SeededRandomSource(3)).Run(hero);

        Assert.Contains(io.Output, line => line.StartsWith("Kulka zatrzymuje się", StringComparison.Ordinal));
        AssertBooksBalance(hero, 100);
    }

    [Fact]
    public void Roulette_NumberBet_AsksForNumber()
    {
        Hero hero = RichHero();
        // 1: ruletka, 5: stawka, 3: numer, 17: numer, 2: nie, 6: wyjdź
        var io = new ScriptedGameIO("1", "5", "3", "17", "2", "6");
        new CasinoScreen(io, new SeededRandomSource(5)).Run(hero);

        Assert.Contains("Na co stawiasz?", io.MenusShown);
        AssertBooksBalance(hero, 100);
        Assert.True(hero.Gold == 95 || hero.Gold == 95 + (5 * 36));
    }

    [Fact]
    public void SlotMachine_OneSpin_ShowsReels()
    {
        Hero hero = RichHero();
        var io = new ScriptedGameIO("2", "2", "2", "6");
        new CasinoScreen(io, new SeededRandomSource(8)).Run(hero);

        Assert.Contains(io.Output, line => line.StartsWith("+-----+", StringComparison.Ordinal));
        AssertBooksBalance(hero, 100);
    }

    [Fact]
    public void Blackjack_StandImmediately_ResolvesHand()
    {
        Hero hero = RichHero();
        const int seed = 11;
        // Ta sama kostka co w ekranie: jeśli rozdanie kończy się od razu (blackjack), menu "Co robisz?" się nie pojawi.
        var preview = new BlackjackGame(1, new SeededRandomSource(seed));
        string[] answers = preview.IsFinished ? ["3", "1", "2", "6"] : ["3", "1", "2", "2", "6"];
        var io = new ScriptedGameIO(answers);
        new CasinoScreen(io, new SeededRandomSource(seed)).Run(hero);

        Assert.Contains(io.Output, line => line.StartsWith("Twoje karty:", StringComparison.Ordinal));
        AssertBooksBalance(hero, 100);
    }

    [Fact]
    public void Craps_OneRoll_ResolvesBet()
    {
        Hero hero = RichHero();
        var io = new ScriptedGameIO("4", "3", "2", "6");
        new CasinoScreen(io, new SeededRandomSource(2)).Run(hero);

        Assert.Contains(io.Output, line => line.StartsWith("Wyrzucono:", StringComparison.Ordinal));
        AssertBooksBalance(hero, 100);
    }

    [Fact]
    public void LoanShark_BorrowThenRepay_ThroughTheScreen()
    {
        Hero hero = RichHero();
        // 5: lichwiarz, 150: pożyczka, 5: lichwiarz, 150: spłata, 6: wyjdź
        var io = new ScriptedGameIO("5", "150", "5", "150", "6");
        new CasinoScreen(io, new SeededRandomSource(1)).Run(hero);

        Assert.Contains(io.Output, line => line.Contains("Dostajesz 150 g", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("Dług spłacony", StringComparison.Ordinal));
        Assert.Equal(0, hero.Debt);
        Assert.Equal(100, hero.Gold);
    }

    [Fact]
    public void LoanShark_PartialRepayment_LeavesDebtAndCancelWithZero()
    {
        Hero hero = RichHero();
        // 5: pożyczka 200; 5: spłać 50 (zostaje 150); 5: 0 = rezygnacja; 6
        var io = new ScriptedGameIO("5", "200", "5", "50", "5", "0", "6");
        new CasinoScreen(io, new SeededRandomSource(1)).Run(hero);

        Assert.Equal(150, hero.Debt);
        Assert.Equal(250, hero.Gold);
        Assert.Contains(io.Output, line => line.Contains("Zostało 150 g", StringComparison.Ordinal));
    }

    [Fact]
    public void NoGold_CannotBet_AndCannotRepay()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Biedak");
        hero.Gold = 0;
        DebtService.Borrow(hero, 50);
        hero.Gold = 0;
        // 1: ruletka -> brak złota, wraca; 5: lichwiarz -> nie masz czym spłacać; 6
        var io = new ScriptedGameIO("1", "5", "6");
        new CasinoScreen(io, new SeededRandomSource(1)).Run(hero);

        Assert.Contains(io.Output, line => line.Contains("Nie masz złota", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("Nie masz czym spłacać", StringComparison.Ordinal));
        Assert.Equal(50, hero.Debt);
    }
}
