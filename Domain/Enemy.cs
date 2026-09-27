namespace PyramidTreasureConsoleRPG.Domain;

public enum EnemyKind
{
    Human,
    Beast,
    Undead,
    Divine,
}

public sealed record EnemyAttackResult(bool Hit, int Damage);

/// <summary>
/// Wspólna klasa wroga. Konkretne potwory to tylko zestawy liczb w konstruktorach podklas.
/// </summary>
public abstract class Enemy
{
    private int hp;

    protected Enemy(string name, EnemyKind kind, int maxHp, int minDmg, int maxDmg, int armor, int agility, int exp, int gold)
    {
        Name = name;
        Kind = kind;
        MaxHp = maxHp;
        hp = maxHp;
        MinDmg = minDmg;
        MaxDmg = maxDmg;
        Armor = armor;
        Agility = agility;
        Exp = exp;
        Gold = gold;
    }

    public string Name { get; }

    public EnemyKind Kind { get; }

    public int MaxHp { get; }

    public int Hp
    {
        get => hp;
        set => hp = Math.Clamp(value, 0, MaxHp);
    }

    public int MinDmg { get; }

    public int MaxDmg { get; }

    public int Armor { get; }

    /// <summary>Zwinność – decyduje o inicjatywie w starciu z bohaterem.</summary>
    public int Agility { get; }

    public int Exp { get; }

    public int Gold { get; }

    /// <summary>Bossowie: nie można od nich uciec.</summary>
    public virtual bool IsBoss => false;

    public bool IsAlive => Hp > 0;

    public EnemyAttackResult Attack(Hero hero, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(rng);
        if (rng.Chance(hero.Evasion))
        {
            return new EnemyAttackResult(false, 0);
        }

        int damage = CombatMath.ReduceByArmor(rng.Range(MinDmg, MaxDmg), hero.Armor);
        hero.TakeDamage(damage);
        return new EnemyAttackResult(true, damage);
    }
}
