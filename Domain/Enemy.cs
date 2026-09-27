namespace PyramidTreasureConsoleRPG.Domain;

public enum EnemyKind
{
    Human,
    Beast,
    Undead,
    Divine,
}

public sealed record EnemyAttackResult(bool Hit, int Damage);

/// <summary>Definicja przeciwnika: same liczby. Nowy potwór to nowy wpis w <see cref="EnemyCatalog"/>.</summary>
public sealed record EnemyDefinition(
    string Name,
    EnemyKind Kind,
    int MaxHp,
    int MinDmg,
    int MaxDmg,
    int Armor,
    int Agility,
    int Exp,
    int Gold,
    bool IsBoss = false)
{
    /// <summary>Tworzy żywego przeciwnika z tej definicji.</summary>
    public Enemy Spawn() => new(this);
}

public static class EnemyCatalog
{
    public static EnemyDefinition Thief { get; } = new("Złodziej", EnemyKind.Human, MaxHp: 22, MinDmg: 3, MaxDmg: 9, Armor: 0, Agility: 5, Exp: 200, Gold: 6);

    public static EnemyDefinition Wolf { get; } = new("Wilk", EnemyKind.Beast, MaxHp: 45, MinDmg: 7, MaxDmg: 13, Armor: 0, Agility: 12, Exp: 500, Gold: 9);

    public static EnemyDefinition WildBoar { get; } = new("Dzik", EnemyKind.Beast, MaxHp: 70, MinDmg: 9, MaxDmg: 16, Armor: 5, Agility: 6, Exp: 650, Gold: 12);

    public static EnemyDefinition ArmoredThief { get; } = new("Opancerzony złodziej", EnemyKind.Human, MaxHp: 100, MinDmg: 12, MaxDmg: 20, Armor: 15, Agility: 8, Exp: 1100, Gold: 22);

    public static EnemyDefinition FallenKnight { get; } = new("Upadły rycerz", EnemyKind.Undead, MaxHp: 150, MinDmg: 17, MaxDmg: 27, Armor: 20, Agility: 8, Exp: 1800, Gold: 35);

    public static EnemyDefinition Templar { get; } = new("Templariusz", EnemyKind.Human, MaxHp: 180, MinDmg: 20, MaxDmg: 31, Armor: 25, Agility: 10, Exp: 2300, Gold: 45);

    public static EnemyDefinition CryingMonk { get; } = new("Płaczący Mnich", EnemyKind.Undead, MaxHp: 280, MinDmg: 26, MaxDmg: 40, Armor: 15, Agility: 14, Exp: 3600, Gold: 70);

    public static EnemyDefinition Anubis { get; } = new("Anubis", EnemyKind.Divine, MaxHp: 500, MinDmg: 32, MaxDmg: 50, Armor: 30, Agility: 16, Exp: 0, Gold: 2500, IsBoss: true);

    public static EnemyDefinition Ra { get; } = new("Bóg Ra", EnemyKind.Divine, MaxHp: 1000, MinDmg: 38, MaxDmg: 60, Armor: 30, Agility: 12, Exp: 0, Gold: 20000, IsBoss: true);

    public static IReadOnlyList<EnemyDefinition> All { get; } = [Thief, Wolf, WildBoar, ArmoredThief, FallenKnight, Templar, CryingMonk, Anubis, Ra];
}

/// <summary>Żywy przeciwnik w walce: definicja plus aktualne HP.</summary>
public sealed class Enemy
{
    private int hp;

    public Enemy(EnemyDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        hp = definition.MaxHp;
    }

    public EnemyDefinition Definition { get; }

    public string Name => Definition.Name;

    public EnemyKind Kind => Definition.Kind;

    public int MaxHp => Definition.MaxHp;

    public int MinDmg => Definition.MinDmg;

    public int MaxDmg => Definition.MaxDmg;

    public int Armor => Definition.Armor;

    /// <summary>Zwinność – decyduje o inicjatywie w starciu z bohaterem.</summary>
    public int Agility => Definition.Agility;

    public int Exp => Definition.Exp;

    public int Gold => Definition.Gold;

    /// <summary>Bossowie: nie można od nich uciec.</summary>
    public bool IsBoss => Definition.IsBoss;

    public int Hp
    {
        get => hp;
        set => hp = Math.Clamp(value, 0, MaxHp);
    }

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
