namespace PyramidTreasureConsoleRPG;

public sealed class Templar : Enemy
{
    public Templar()
        : base("Templariusz", EnemyKind.Human, maxHp: 180, minDmg: 20, maxDmg: 31, armor: 25, agility: 10, exp: 2300, gold: 45)
    {
    }
}
