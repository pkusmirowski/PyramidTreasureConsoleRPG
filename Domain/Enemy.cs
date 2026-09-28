namespace PyramidTreasureConsoleRPG.Domain;

public enum EnemyKind
{
    Human,
    Beast,
    Undead,
    Divine,
}

public sealed record EnemyAttackResult(bool Hit, int Damage);

/// <summary>Umiejętność specjalna wroga, obsługiwana przez silnik walki.</summary>
public enum EnemyAbility
{
    None,

    /// <summary>Kradnie 5% złota przy trafieniu i ucieka poniżej 30% HP.</summary>
    Thievery,

    /// <summary>+20% obrażeń, gdy żyje inny wróg tego samego rodzaju.</summary>
    Pack,

    /// <summary>Co trzeci atak zadaje podwójne obrażenia.</summary>
    Charge,

    /// <summary>Co drugą turę zasłania się tarczą (obrona).</summary>
    Block,

    /// <summary>Pierwsze trafienie nakłada strach na 2 tury.</summary>
    Terrify,

    /// <summary>20% szansy na ogłuszenie przy trafieniu.</summary>
    ShieldBash,

    /// <summary>Co trzecią turę lament: strach na 2 tury i leczenie 10%.</summary>
    Wail,

    /// <summary>Raz wstaje z 30% HP, jeśli żyje drugi taki wróg.</summary>
    Resurrect,

    /// <summary>Fazy bossa: przy 50% wzywa cienie, przy 20% szykuje potrójny cios.</summary>
    SunGod,
}

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
    bool IsBoss = false,
    IReadOnlyList<LootEntry>? Loot = null,
    EnemyAbility Ability = EnemyAbility.None)
{
    /// <summary>Tabela łupów (pusta, gdy wróg nic nie nosi).</summary>
    public IReadOnlyList<LootEntry> LootTable => Loot ?? [];

    /// <summary>Tworzy żywego przeciwnika z tej definicji.</summary>
    public Enemy Spawn() => new(this);
}

public static class EnemyCatalog
{
    public static EnemyDefinition Thief { get; } = new("Złodziej", EnemyKind.Human, MaxHp: 22, MinDmg: 3, MaxDmg: 9, Armor: 0, Agility: 5, Exp: 200, Gold: 6, Loot: [new LootEntry(12, Potion: PotionKind.Small), new LootEntry(4, ItemId.LeatherJerkin)], Ability: EnemyAbility.Thievery);

    public static EnemyDefinition Wolf { get; } = new("Wilk", EnemyKind.Beast, MaxHp: 45, MinDmg: 7, MaxDmg: 13, Armor: 0, Agility: 12, Exp: 500, Gold: 9, Loot: [new LootEntry(5, ItemId.WolfBone)], Ability: EnemyAbility.Pack);

    public static EnemyDefinition WildBoar { get; } = new("Dzik", EnemyKind.Beast, MaxHp: 70, MinDmg: 9, MaxDmg: 16, Armor: 5, Agility: 6, Exp: 650, Gold: 12, Loot: [new LootEntry(20, Potion: PotionKind.Small)], Ability: EnemyAbility.Charge);

    public static EnemyDefinition ArmoredThief { get; } = new("Opancerzony złodziej", EnemyKind.Human, MaxHp: 100, MinDmg: 12, MaxDmg: 20, Armor: 15, Agility: 8, Exp: 1100, Gold: 22, Loot: [new LootEntry(6, ItemId.ChainMail), new LootEntry(4, ItemId.DesertCloak), new LootEntry(10, Potion: PotionKind.Medium)], Ability: EnemyAbility.Block);

    public static EnemyDefinition FallenKnight { get; } = new("Upadły rycerz", EnemyKind.Undead, MaxHp: 150, MinDmg: 17, MaxDmg: 27, Armor: 20, Agility: 8, Exp: 1800, Gold: 35, Loot: [new LootEntry(8, ItemId.ChainMail), new LootEntry(4, ItemId.TemplarSword), new LootEntry(12, Potion: PotionKind.Medium)], Ability: EnemyAbility.Terrify);

    public static EnemyDefinition Templar { get; } = new("Templariusz", EnemyKind.Human, MaxHp: 180, MinDmg: 20, MaxDmg: 31, Armor: 25, Agility: 10, Exp: 2300, Gold: 45, Loot: [new LootEntry(5, ItemId.TemplarSword), new LootEntry(3, ItemId.TemplarPlate), new LootEntry(4, ItemId.RecurveBow), new LootEntry(4, ItemId.TwinKindjals), new LootEntry(10, Potion: PotionKind.Large)], Ability: EnemyAbility.ShieldBash);

    public static EnemyDefinition CryingMonk { get; } = new("Płaczący Mnich", EnemyKind.Undead, MaxHp: 280, MinDmg: 26, MaxDmg: 40, Armor: 15, Agility: 14, Exp: 3600, Gold: 70, Loot: [new LootEntry(8, ItemId.BrotherhoodRobe), new LootEntry(3, ItemId.EyeOfRa), new LootEntry(3, ItemId.Scarab), new LootEntry(12, Potion: PotionKind.Large)], Ability: EnemyAbility.Wail);

    public static EnemyDefinition Anubis { get; } = new("Anubis", EnemyKind.Divine, MaxHp: 900, MinDmg: 58, MaxDmg: 85, Armor: 40, Agility: 16, Exp: 0, Gold: 2500, IsBoss: true, Loot: [new LootEntry(50, ItemId.AnubisFangs), new LootEntry(50, ItemId.PharaohScimitar), new LootEntry(50, ItemId.FalconBow)], Ability: EnemyAbility.Resurrect);

    public static EnemyDefinition Ra { get; } = new("Bóg Ra", EnemyKind.Divine, MaxHp: 2500, MinDmg: 90, MaxDmg: 132, Armor: 45, Agility: 12, Exp: 0, Gold: 20000, IsBoss: true, Ability: EnemyAbility.SunGod);

    /// <summary>Egzekutor lichwiarza.</summary>
    public static EnemyDefinition Breaker { get; } = new("Łamacz", EnemyKind.Human, MaxHp: 220, MinDmg: 24, MaxDmg: 36, Armor: 20, Agility: 9, Exp: 900, Gold: 60, Loot: [new LootEntry(20, ItemId.ChainMail)], Ability: EnemyAbility.Block);

    /// <summary>Wzywany przez Ra w drugiej fazie.</summary>
    public static EnemyDefinition ShadowAnubis { get; } = new("Cień Anubisa", EnemyKind.Divine, MaxHp: 300, MinDmg: 24, MaxDmg: 36, Armor: 15, Agility: 16, Exp: 0, Gold: 0, IsBoss: true);

    public static IReadOnlyList<EnemyDefinition> All { get; } = [Thief, Wolf, WildBoar, ArmoredThief, FallenKnight, Templar, CryingMonk, Anubis, Ra, ShadowAnubis, Breaker];
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

    public EnemyAbility Ability => Definition.Ability;

    public StatusList Statuses { get; } = new();

    /// <summary>Licznik tur wroga (umiejętności cykliczne).</summary>
    public int Turn { get; set; }

    /// <summary>Czy umiejętność jednorazowa (wskrzeszenie, fazy) została już użyta.</summary>
    public int AbilityUses { get; set; }

    /// <summary>Wróg uciekł z walki (złodziej).</summary>
    public bool Fled { get; set; }

    /// <summary>Zapowiedziany potężny cios w następnej turze (Ra).</summary>
    public bool Charging { get; set; }

    /// <summary>Atak na bohatera z mnożnikiem obrażeń (umiejętności) i po unikach/obronie bohatera.</summary>
    public EnemyAttackResult Attack(Hero hero, IRandomSource rng, double multiplier = 1.0)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(rng);
        if (rng.Chance(hero.Evasion) || rng.Chance(Statuses.HitPenalty))
        {
            return new EnemyAttackResult(false, 0);
        }

        int raw = (int)Math.Round(rng.Range(MinDmg, MaxDmg) * multiplier * Statuses.DamageMultiplier);
        int damage = hero.TakeDamage(CombatMath.ReduceByArmor(raw, hero.Armor));
        return new EnemyAttackResult(true, damage);
    }
}
