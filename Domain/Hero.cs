namespace PyramidTreasureConsoleRPG;

public enum HeroClass
{
    Warrior = 1,
    Archer = 2,
    Assassin = 3,
}

public enum AttackKind
{
    Normal = 1,
    Strong = 2,
    Special = 3,
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
/// Wspólna klasa bohatera. Klasy postaci (Wojownik, Łucznik, Asasyn) różnią się tylko
/// wartościami startowymi, przyrostami, wzorami na pancerz/uniki i atakiem specjalnym.
/// Wszystkie statystyki pochodne są liczone z Vit/Str/Dex, więc nie da się ich "napompować"
/// przez wielokrotne wywołania (dawny exploit z wodą w barze).
/// </summary>
public abstract class Hero
{
    private int hp;

    protected Hero(string name)
    {
        Name = name;
        Level = 1;
        Vit = StartVit;
        Str = StartStr;
        Dex = StartDex;
        Gold = 15;
        hp = MaxHp;
    }

    // --- definicja klasy (nadpisywana w podklasach) ---

    public abstract HeroClass HeroClass { get; }

    public abstract string ClassName { get; }

    protected abstract int StartVit { get; }

    protected abstract int StartStr { get; }

    protected abstract int StartDex { get; }

    protected abstract int HpPerVit { get; }

    protected abstract int BaseMinDmg { get; }

    protected abstract int BaseMaxDmg { get; }

    /// <summary>Waga siły we wzorze na obrażenia.</summary>
    protected abstract int StrWeight { get; }

    /// <summary>Waga zręczności we wzorze na obrażenia.</summary>
    protected abstract int DexWeight { get; }

    protected abstract double BaseHitChance { get; }

    protected abstract double BaseCritChance { get; }

    protected abstract int VitPerLevel { get; }

    protected abstract int StrPerLevel { get; }

    protected abstract int DexPerLevel { get; }

    public abstract string NormalAttackName { get; }

    public abstract string StrongAttackName { get; }

    public abstract string SpecialAttackName { get; }

    protected abstract string SpecialAttackDescription { get; }

    protected abstract int ArmorFormula();

    protected abstract int EvasionFormula();

    protected abstract AttackResult PerformSpecialAttack(Enemy enemy);

    // --- stan ---

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

    public int MaxHp => Vit * HpPerVit;

    private int Power => Str * StrWeight + Dex * DexWeight;

    public int MinDmg => BaseMinDmg + Power / 4;

    public int MaxDmg => BaseMaxDmg + Power / 2;

    public double HitChance => CombatMath.ClampChance(BaseHitChance + (Level * 1.5) + ExtraHitChance());

    public double CritChance => Math.Clamp(BaseCritChance + Level, 0, 75);

    public int Armor => ArmorFormula();

    /// <summary>Szansa (w %) na uniknięcie ataku wroga.</summary>
    public int Evasion => Math.Clamp(EvasionFormula(), 0, 40);

    /// <summary>Szansa (w %) na ucieczkę z walki.</summary>
    public int FleeChance => Math.Clamp(35 + Dex, 20, 80);

    public int ExpToNextLevel => CombatMath.ExpToNextLevel(Level);

    public bool IsMaxLevel => Level >= CombatMath.MaxLevel;

    public bool IsAlive => Hp > 0;

    protected virtual double ExtraHitChance() => 0;

    // --- akcje ---

    public IReadOnlyList<AttackOption> AttackOptions => new[]
    {
        new AttackOption(AttackKind.Normal, NormalAttackName, $"{MinDmg}-{MaxDmg} obrażeń, trafienie {HitChance + 20:0}%, krytyk {CritChance:0}%"),
        new AttackOption(AttackKind.Strong, StrongAttackName, $"{MinDmg * 3 / 2}-{MaxDmg * 3 / 2} obrażeń, trafienie {HitChance:0}%, krytyk {Math.Min(75, CritChance * 2):0}%"),
        new AttackOption(AttackKind.Special, SpecialAttackName, SpecialAttackDescription),
    };

    public AttackResult Attack(AttackKind kind, Enemy enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        AttackResult result = kind switch
        {
            AttackKind.Strong => SingleStrike(enemy, HitChance, 1.5, Math.Min(75, CritChance * 2), StrongAttackName),
            AttackKind.Special => PerformSpecialAttack(enemy),
            _ => SingleStrike(enemy, HitChance + 20, 1.0, CritChance, NormalAttackName),
        };

        foreach (Strike strike in result.Strikes)
        {
            enemy.Hp -= strike.Damage;
        }

        return result;
    }

    /// <summary>Jedno uderzenie: rzut na trafienie, rzut na krytyk, obrażenia po pancerzu wroga.</summary>
    protected AttackResult SingleStrike(Enemy enemy, double hitChance, double multiplier, double critChance, string label)
    {
        var result = new AttackResult();
        result.Strikes.Add(RollStrike(enemy, hitChance, multiplier, critChance, label));
        return result;
    }

    protected Strike RollStrike(Enemy enemy, double hitChance, double multiplier, double critChance, string label)
    {
        if (!Rng.Chance(CombatMath.ClampChance(hitChance)))
        {
            return new Strike(false, false, 0, label);
        }

        int raw = (int)Math.Round(Rng.Range(MinDmg, MaxDmg) * multiplier);
        bool critical = Rng.Chance(critChance);
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
        Vit += VitPerLevel;
        Str += StrPerLevel;
        Dex += DexPerLevel;

        // Awans leczy o przyrost maksymalnego HP, nie do pełna (pełne leczenie robiło z awansu darmowy nocleg).
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

    public static Hero Create(HeroClass heroClass, string name) => heroClass switch
    {
        HeroClass.Warrior => new Warrior(name),
        HeroClass.Archer => new Archer(name),
        HeroClass.Assassin => new Assassin(name),
        _ => throw new ArgumentOutOfRangeException(nameof(heroClass)),
    };

    public static Hero FromSaveData(SaveData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (!Enum.IsDefined(typeof(HeroClass), data.Class))
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
        hero.Stage = Enum.IsDefined(typeof(StoryStage), data.Stage) ? (StoryStage)data.Stage : StoryStage.Start;
        hero.SpecialDrinkUsed = data.SpecialDrinkUsed;
        hero.Completed = data.Completed;
        hero.Hp = data.Hp <= 0 ? hero.MaxHp : data.Hp;
        foreach (int kind in data.Potions ?? new List<int>())
        {
            if (Enum.IsDefined(typeof(PotionKind), kind))
            {
                hero.Inventory.Add(Potion.Create((PotionKind)kind));
            }
        }

        return hero;
    }
}

public enum StatKind
{
    Strength = 1,
    Dexterity = 2,
    Vitality = 3,
}
