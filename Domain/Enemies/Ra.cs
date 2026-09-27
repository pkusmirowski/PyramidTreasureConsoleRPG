namespace PyramidTreasureConsoleRPG;

public sealed class Ra : Enemy
{
    public Ra()
        : base("Bóg Ra", EnemyKind.Divine, maxHp: 1000, minDmg: 38, maxDmg: 60, armor: 30, agility: 12, exp: 0, gold: 20000)
    {
    }

    public override bool IsBoss => true;
}
