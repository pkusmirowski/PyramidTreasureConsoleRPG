namespace PyramidTreasureConsoleRPG.Tests;

public class CasinoTests
{
    [Fact]
    public void ColorOf_ZeroIsGreen_And18BlackAnd18Red()
    {
        Assert.Equal(RouletteColor.Green, CasinoEngine.ColorOf(0));
        int black = Enumerable.Range(1, 36).Count(n => CasinoEngine.ColorOf(n) == RouletteColor.Black);
        int red = Enumerable.Range(1, 36).Count(n => CasinoEngine.ColorOf(n) == RouletteColor.Red);
        Assert.Equal(18, black);
        Assert.Equal(18, red);
    }

    [Theory]
    [InlineData(7, 7, 7, 10, 500)]
    [InlineData(3, 3, 3, 10, 100)]
    [InlineData(3, 3, 5, 10, 15)]
    [InlineData(1, 4, 4, 10, 15)]
    [InlineData(1, 2, 3, 10, 0)]
    public void SlotPayout_MatchesTable(int a, int b, int c, int bet, int expected)
    {
        Assert.Equal(expected, CasinoEngine.SlotPayout(a, b, c, bet));
    }

    [Fact]
    public void SlotMachine_HasHouseEdge()
    {
        long totalPayout = 0;
        int spins = 0;
        for (int a = 1; a <= 7; a++)
        {
            for (int b = 1; b <= 7; b++)
            {
                for (int c = 1; c <= 7; c++)
                {
                    totalPayout += CasinoEngine.SlotPayout(a, b, c, 100);
                    spins++;
                }
            }
        }

        double returnRate = totalPayout / (spins * 100.0);
        Assert.InRange(returnRate, 0.75, 0.99);
    }

    [Theory]
    [InlineData(new[] { 1, 13 }, 21)]
    [InlineData(new[] { 1, 1 }, 12)]
    [InlineData(new[] { 1, 9, 5 }, 15)]
    [InlineData(new[] { 11, 12, 13 }, 30)]
    public void BlackjackScore_HandlesAcesAndFaces(int[] cards, int expected)
    {
        Assert.Equal(expected, CasinoEngine.BlackjackScore(cards));
    }
}
