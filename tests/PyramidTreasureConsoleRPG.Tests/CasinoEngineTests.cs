namespace PyramidTreasureConsoleRPG.Tests;

public class CasinoEngineTests
{
    [Fact]
    public void RouletteColorBet_HasNegativeExpectedValue()
    {
        var rng = new SeededRandomSource(42);
        long net = 0;
        const int spins = 20_000;
        for (int i = 0; i < spins; i++)
        {
            net += CasinoEngine.PlayRoulette(10, RouletteBet.Red, -1, rng).Payout - 10;
        }

        Assert.True(net < 0, $"Ruletka na kolor wyszła na plus ({net}) – kasyno przegrywa.");
    }

    [Fact]
    public void RouletteNumberBet_PaysThirtySixTimes()
    {
        // NextInt(0, 37) -> 17
        RouletteResult result = CasinoEngine.PlayRoulette(5, RouletteBet.Number, 17, new ScriptedRandomSource(17));
        Assert.True(result.Won);
        Assert.Equal(180, result.Payout);
        Assert.Equal(RouletteColor.Black, result.Color);
    }

    [Fact]
    public void Blackjack_NaturalTwentyOne_PaysFiveToTwo()
    {
        // player: A, K ; dealer: 5, 9
        var game = new BlackjackGame(10, new ScriptedRandomSource(1, 13, 5, 9));
        Assert.Equal(BlackjackOutcome.PlayerBlackjack, game.Outcome);
        Assert.Equal(25, game.Payout);
        Assert.Throws<InvalidOperationException>(() => game.Hit());
    }

    [Fact]
    public void Blackjack_DealerDrawsToSeventeen_AndBustLoses()
    {
        // player: 10, 8 ; dealer: 6, 10 ; dealer draws 9 -> 25 bust
        var game = new BlackjackGame(10, new ScriptedRandomSource(10, 8, 6, 10, 9));
        game.Stand();
        Assert.Equal(BlackjackOutcome.PlayerWins, game.Outcome);
        Assert.Equal(20, game.Payout);
        Assert.Equal(3, game.DealerCards.Count);
    }

    [Fact]
    public void Blackjack_PlayerBust_LosesStake()
    {
        var game = new BlackjackGame(10, new ScriptedRandomSource(10, 8, 6, 10, 9));
        game.Hit();
        Assert.Equal(BlackjackOutcome.PlayerBust, game.Outcome);
        Assert.Equal(0, game.Payout);
    }

    [Fact]
    public void Craps_SevenOnFirstRoll_WinsDouble()
    {
        CrapsResult result = CasinoEngine.PlayCraps(7, new ScriptedRandomSource(3, 4));
        Assert.True(result.Won);
        Assert.Null(result.Point);
        Assert.Equal(14, result.Payout);
    }

    [Fact]
    public void Craps_PointThenSeven_Loses()
    {
        // 4+4 = 8 point, then 3+4 = 7
        CrapsResult result = CasinoEngine.PlayCraps(7, new ScriptedRandomSource(4, 4, 3, 4));
        Assert.False(result.Won);
        Assert.Equal(8, result.Point);
        Assert.Equal(0, result.Payout);
        Assert.Equal(2, result.Rolls.Count);
    }

    [Fact]
    public void Slots_TripleSeven_IsJackpot()
    {
        SlotResult result = CasinoEngine.SpinSlots(2, new ScriptedRandomSource(7, 7, 7));
        Assert.True(result.Jackpot);
        Assert.Equal(100, result.Payout);
    }
}
