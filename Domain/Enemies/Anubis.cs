namespace PyramidTreasureConsoleRPG.Domain;

public sealed class Anubis : Enemy
{
    public Anubis()
        : base("Anubis", EnemyKind.Divine, maxHp: 500, minDmg: 32, maxDmg: 50, armor: 30, agility: 16, exp: 0, gold: 2500)
    {
    }

    public override bool IsBoss => true;
}
