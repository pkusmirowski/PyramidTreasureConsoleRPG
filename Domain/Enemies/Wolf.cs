namespace PyramidTreasureConsoleRPG;

public sealed class Wolf : Enemy
{
    public Wolf()
        : base("Wilk", EnemyKind.Beast, maxHp: 45, minDmg: 7, maxDmg: 13, armor: 0, agility: 12, exp: 500, gold: 9)
    {
    }
}
