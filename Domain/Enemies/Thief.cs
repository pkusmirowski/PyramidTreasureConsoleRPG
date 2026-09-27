namespace PyramidTreasureConsoleRPG.Domain;

public sealed class Thief : Enemy
{
    public Thief()
        : base("Złodziej", EnemyKind.Human, maxHp: 22, minDmg: 3, maxDmg: 9, armor: 0, agility: 5, exp: 200, gold: 6)
    {
    }
}
