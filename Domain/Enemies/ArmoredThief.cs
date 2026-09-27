namespace PyramidTreasureConsoleRPG;

public sealed class ArmoredThief : Enemy
{
    public ArmoredThief()
        : base("Opancerzony złodziej", EnemyKind.Human, maxHp: 100, minDmg: 12, maxDmg: 20, armor: 15, agility: 8, exp: 1100, gold: 22)
    {
    }
}
