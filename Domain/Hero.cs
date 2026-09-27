namespace PyramidTreasureConsoleRPG.Domain;

public enum AttackKind
{
    Normal = 1,
    Strong = 2,
    Special = 3,
}

public enum StatKind
{
    Strength = 1,
    Dexterity = 2,
    Vitality = 3,
}

/// <summary>Opis jednego wariantu ataku pokazywany w menu walki.</summary>
public sealed record AttackOption(AttackKind Kind, string Name, string Description);

/// <summary>Wynik pojedynczego uderzenia.</summary>
public sealed record Strike(bool Hit, bool Critical, int Damage, string Label);

/// <summary>Wynik całego ataku (atak specjalny może mieć kilka uderzeń).</summary>
public sealed class AttackResult
{
    public List<Strike> Strikes { get; } = new();

    public int TotalDamage => Strikes.Sum(s => s.Damage);

    public bool AnyHit => Strikes.Any(s => s.Hit);
}

/// <summary>Statyczne wzory walki – czyste funkcje, łatwe do przetestowania.</summary>
public static class CombatMath
{
    public const int MaxLevel = 20;

    /// <summary>Pancerz działa procentowo: 50 pancerza = 33 % redukcji, 100 = 50 %. Nigdy nie daje odporności.</summary>
    public static int ReduceByArmor(int damage, int armor)
    {
        if (damage <= 0)
        {
            return 0;
        }

        armor = Math.Max(0, armor);
        return Math.Max(1, damage * 100 / (100 + armor));
    }

    /// <summary>Doświadczenie potrzebne, aby przejść z podanego poziomu na następny.</summary>
    public static int ExpToNextLevel(int level)
    {
        level = Math.Clamp(level, 1, MaxLevel);
        return (int)(400 * Math.Pow(level, 1.5));
    }

    public static double ClampChance(double chance) => Math.Clamp(chance, 0, 95);
}

/// <summary>
/// Bohater. Klasa postaci to dane (<see cref="HeroClassDefinition"/>), a wszystkie statystyki
/// pochodne są liczone z Vit/Str/Dex, więc nie da się ich "napompować" wielokrotnymi wywołaniami.
/// </summary>
public sealed class Hero
{
    private int hp;

    private Hero(HeroClassDefinition definition, string name)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Name = name;
        Level = 1;
        Vit = definition.StartVit;
        Str = definition.StartStr;
        Dex = definition.StartDex;
        Gold = 15;
        hp = MaxHp;
    }

    public HeroClassDefinition Definition { get; }

    public HeroClass HeroClass => Definition.Kind;

    public string ClassName => Definition.Name;

    public string Name { get; set; }

    public int Level { get; private set; }

    public int Exp { get; private set; }

    public int Vit { get; private set; }

    public int Str { get; private set; }

    public int Dex { get; private set; }

    public int Gold { get; set; }

    public StoryStage Stage { get; set; } = StoryStage.Start;

    public bool SpecialDrinkUsed { get; set; }

    public bool Completed { get; set; }

    public List<Potion> Inventory { get; } = new();

    public int Hp
    {
        get => hp;
        set => hp = Math.Clamp(value, 0, MaxHp);
    }

    // --- statystyki pochodne ---

    public int MaxHp => Vit * Definition.HpPerVit;

    private int Power => (Str * Definition.StrWeight) + (Dex * Definition.DexWeight);

    public int MinDmg => Definition.BaseMinDmg + (Power / 4);

    public int MaxDmg => Definition.BaseMaxDmg + (Power / 2);

    public double HitChance => CombatMath.ClampChance(Definition.BaseHitChance + (Level * 1.5) + ExtraHitChance);

    public double CritChance => Math.Clamp(Definition.BaseCritChance + Level, 0, 75);

    public int Armor => (Dex / Definition.ArmorDexDivisor) + (Definition.ArmorAddsLevel ? Level : 0);

    /// <summary>Szansa (w %) na uniknięcie ataku wroga.</summary>
    public int Evasion => Math.Clamp(Dex / Definition.EvasionDexDivisor, 0, 40);

    /// <summary>Szansa (w %) na ucieczkę z walki.</summary>
    public int FleeChance => Math.Clamp(35 + Dex, 20, 80);

    public int ExpToNextLevel => CombatMath.ExpToNextLevel(Level);

    public bool IsMaxLevel => Level >= CombatMath.MaxLevel;

    public bool IsAlive => Hp > 0;

    private double ExtraHitChance => Definition.HitChanceDexDivisor == 0 ? 0 : Dex / (double)Definition.HitChanceDexDivisor;

    // --- akcje ---

    public IReadOnlyList<AttackOption> GetAttackOptions() => new[]
    {
        new AttackOption(AttackKind.Normal, Definition.NormalAttackName, $"{MinDmg}-{MaxDmg} obrażeń, trafienie {HitChance + 20:0}%, krytyk {CritChance:0}%"),
        new AttackOption(AttackKind.Strong, Definition.StrongAttackName, $"{MinDmg * 3 / 2}-{MaxDmg * 3 / 2} obrażeń, trafienie {HitChance:0}%, krytyk {Math.Min(75, CritChance * 2):0}%"),
        new AttackOption(AttackKind.Special, Definition.SpecialAttackName, SpecialAttackDescription()),
    };

    public AttackResult Attack(AttackKind kind, Enemy enemy, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(rng);
        var result = new AttackResult();
        switch (kind)
        {
            case AttackKind.Strong:
                result.Strikes.Add(RollStrike(enemy, HitChance, 1.5, Math.Min(75, CritChance * 2), Definition.StrongAttackName, rng));
                break;
            case AttackKind.Special:
                AddSpecialStrikes(result, enemy, rng);
                break;
            default:
                result.Strikes.Add(RollStrike(enemy, HitChance + 20, 1.0, CritChance, Definition.NormalAttackName, rng));
                break;
        }

        foreach (Strike strike in result.Strikes)
        {
            enemy.Hp -= strike.Damage;
        }

        return result;
    }

    private string SpecialAttackDescription() => Definition.SpecialAttack switch
    {
        SpecialAttackKind.TripleCut => $"3 cięcia po {MinDmg * 6 / 10}-{MaxDmg * 6 / 10} obrażeń, każde z trafieniem {CombatMath.ClampChance(HitChance - 15):0}%",
        SpecialAttackKind.DoubleShot => $"2 strzały po {MinDmg * 8 / 10}-{MaxDmg * 8 / 10} obrażeń, każdy z trafieniem {HitChance:0}%",
        _ => $"{MinDmg * 12 / 10}-{MaxDmg * 12 / 10} obrażeń, trafienie {CombatMath.ClampChance(HitChance - 10):0}%, krytyk {Math.Min(75, CritChance * 3):0}%",
    };

    private void AddSpecialStrikes(AttackResult result, Enemy enemy, IRandomSource rng)
    {
        switch (Definition.SpecialAttack)
        {
            case SpecialAttackKind.TripleCut:
                for (int i = 1; i <= 3; i++)
                {
                    result.Strikes.Add(RollStrike(enemy, HitChance - 15, 0.6, CritChance, $"Cięcie {i}", rng));
                }

                break;
            case SpecialAttackKind.DoubleShot:
                result.Strikes.Add(RollStrike(enemy, HitChance, 0.8, CritChance, "Pierwsza strzała", rng));
                result.Strikes.Add(RollStrike(enemy, HitChance, 0.8, CritChance, "Druga strzała", rng));
                break;
            default:
                result.Strikes.Add(RollStrike(enemy, HitChance - 10, 1.2, Math.Min(75, CritChance * 3), Definition.SpecialAttackName, rng));
                break;
        }
    }

    /// <summary>Jedno uderzenie: rzut na trafienie, rzut na krytyk, obrażenia po pancerzu wroga.</summary>
    private Strike RollStrike(Enemy enemy, double hitChance, double multiplier, double critChance, string label, IRandomSource rng)
    {
        if (!rng.Chance(CombatMath.ClampChance(hitChance)))
        {
            return new Strike(false, false, 0, label);
        }

        int raw = (int)Math.Round(rng.Range(MinDmg, MaxDmg) * multiplier);
        bool critical = rng.Chance(critChance);
        if (critical)
        {
            raw *= 2;
        }

        return new Strike(true, critical, CombatMath.ReduceByArmor(raw, enemy.Armor), label);
    }

    public void TakeDamage(int damage) => Hp -= Math.Max(0, damage);

    public void Heal(int amount) => Hp += Math.Max(0, amount);

    public void FullHeal() => Hp = MaxHp;

    /// <summary>Dodaje doświadczenie i zwraca liczbę zdobytych poziomów (może być kilka naraz).</summary>
    public int AddExp(int amount)
    {
        if (amount <= 0 || IsMaxLevel)
        {
            return 0;
        }

        Exp += amount;
        int gained = 0;
        while (!IsMaxLevel && Exp >= ExpToNextLevel)
        {
            Exp -= ExpToNextLevel;
            LevelUp();
            gained++;
        }

        if (IsMaxLevel)
        {
            Exp = 0;
        }

        return gained;
    }

    private void LevelUp()
    {
        int oldMax = MaxHp;
        Level++;
        Vit += Definition.VitPerLevel;
        Str += Definition.StrPerLevel;
        Dex += Definition.DexPerLevel;

        // Awans leczy o przyrost maksymalnego HP, nie do pełna.
        Hp += MaxHp - oldMax;
    }

    /// <summary>Trwałe zwiększenie statystyki (np. nagroda fabularna).</summary>
    public void IncreaseStat(StatKind stat, int amount)
    {
        switch (stat)
        {
            case StatKind.Strength:
                Str += amount;
                break;
            case StatKind.Dexterity:
                Dex += amount;
                break;
            case StatKind.Vitality:
                int oldMax = MaxHp;
                Vit += amount;
                Hp += MaxHp - oldMax;
                break;
        }
    }

    public int CountPotions(PotionKind kind) => Inventory.Count(p => p.Kind == kind);

    /// <summary>Wypija miksturę danego rodzaju. Zwraca ilość faktycznie uleczonych HP lub null, gdy brak mikstury.</summary>
    public int? DrinkPotion(PotionKind kind)
    {
        Potion? potion = Inventory.Find(p => p.Kind == kind);
        if (potion is null)
        {
            return null;
        }

        Inventory.Remove(potion);
        int before = Hp;
        Heal(potion.RestoreHp);
        return Hp - before;
    }

    // --- zapis / odczyt ---

    public SaveData ToSaveData() => new()
    {
        Version = SaveData.CurrentVersion,
        Name = Name,
        Class = (int)HeroClass,
        Level = Level,
        Exp = Exp,
        Vit = Vit,
        Str = Str,
        Dex = Dex,
        Hp = Hp,
        Gold = Gold,
        Stage = (int)Stage,
        SpecialDrinkUsed = SpecialDrinkUsed,
        Completed = Completed,
        Potions = Inventory.Select(p => (int)p.Kind).ToList(),
    };

    public static Hero Create(HeroClass heroClass, string name) => new(HeroClasses.Get(heroClass), name);

    public static Hero FromSaveData(SaveData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (!Enum.IsDefined((HeroClass)data.Class))
        {
            throw new InvalidDataException($"Nieznana klasa postaci: {data.Class}.");
        }

        Hero hero = Create((HeroClass)data.Class, string.IsNullOrWhiteSpace(data.Name) ? "Bezimienny" : data.Name);
        hero.Level = Math.Clamp(data.Level, 1, CombatMath.MaxLevel);
        hero.Vit = Math.Max(1, data.Vit);
        hero.Str = Math.Max(0, data.Str);
        hero.Dex = Math.Max(0, data.Dex);
        hero.Exp = hero.IsMaxLevel ? 0 : Math.Clamp(data.Exp, 0, hero.ExpToNextLevel - 1);
        hero.Gold = Math.Max(0, data.Gold);
        hero.Stage = Enum.IsDefined((StoryStage)data.Stage) ? (StoryStage)data.Stage : StoryStage.Start;
        hero.SpecialDrinkUsed = data.SpecialDrinkUsed;
        hero.Completed = data.Completed;
        hero.Hp = data.Hp <= 0 ? hero.MaxHp : data.Hp;
        foreach (int kind in data.Potions)
        {
            if (Enum.IsDefined((PotionKind)kind))
            {
                hero.Inventory.Add(Potion.Create((PotionKind)kind));
            }
        }

        return hero;
    }
}
