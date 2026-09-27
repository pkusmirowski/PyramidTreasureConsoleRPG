namespace PyramidTreasureConsoleRPG.Tests;

public class CombatMathTests
{
    [Theory]
    [InlineData(100, 0, 100)]
    [InlineData(100, 50, 66)]
    [InlineData(100, 100, 50)]
    [InlineData(10, 1000, 1)]
    [InlineData(0, 10, 0)]
    public void ReduceByArmor_IsPercentageBased_AndNeverBelowOne(int damage, int armor, int expected)
    {
        Assert.Equal(expected, CombatMath.ReduceByArmor(damage, armor));
    }

    [Fact]
    public void ExpToNextLevel_GrowsMonotonically()
    {
        int previous = 0;
        for (int level = 1; level <= CombatMath.MaxLevel; level++)
        {
            int current = CombatMath.ExpToNextLevel(level);
            Assert.True(current > previous, $"Próg dla poziomu {level} ({current}) nie jest większy od poprzedniego ({previous}).");
            previous = current;
        }
    }

    [Fact]
    public void ClampChance_NeverExceeds95()
    {
        Assert.Equal(95, CombatMath.ClampChance(500));
        Assert.Equal(0, CombatMath.ClampChance(-5));
    }
}
