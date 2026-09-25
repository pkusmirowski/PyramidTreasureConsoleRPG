namespace PyramidTreasureConsoleRPG;

public sealed class WildBoar : Enemy
{
    public WildBoar()
        : base("Dzik", EnemyKind.Beast, maxHp: 70, minDmg: 9, maxDmg: 16, armor: 5, agility: 6, exp: 650, gold: 12)
    {
    }
}
