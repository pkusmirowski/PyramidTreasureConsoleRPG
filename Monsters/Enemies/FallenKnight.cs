namespace PyramidTreasureConsoleRPG;

public sealed class FallenKnight : Enemy
{
    public FallenKnight()
        : base("Upadły rycerz", EnemyKind.Undead, maxHp: 150, minDmg: 17, maxDmg: 27, armor: 20, agility: 8, exp: 1800, gold: 35)
    {
    }
}
