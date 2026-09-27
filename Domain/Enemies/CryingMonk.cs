namespace PyramidTreasureConsoleRPG.Domain;

public sealed class CryingMonk : Enemy
{
    public CryingMonk()
        : base("Płaczący Mnich", EnemyKind.Undead, maxHp: 280, minDmg: 26, maxDmg: 40, armor: 15, agility: 14, exp: 3600, gold: 70)
    {
    }
}
