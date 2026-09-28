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

    /// <summary>Dzień wyprawy (podróże i eksploracja go zwiększają).</summary>
    public int Day { get; set; } = 1;

    public RegionId CurrentRegion { get; set; } = RegionId.Port;

    private readonly Dictionary<Faction, int> reputation = new() { [Faction.Town] = 0, [Faction.Underworld] = 0, [Faction.Brotherhood] = 0 };
    private readonly HashSet<string> flags = new(StringComparer.Ordinal);
    private readonly Dictionary<QuestId, QuestProgress> quests = new();

    public IReadOnlyDictionary<Faction, int> Reputation => reputation;

    public IReadOnlySet<string> Flags => flags;

    public IReadOnlyDictionary<QuestId, QuestProgress> Quests => quests;

    public int GetReputation(Faction faction) => reputation.GetValueOrDefault(faction);

    /// <summary>Zmienia reputację (−100..100) i zwraca nową wartość.</summary>
    public int AdjustReputation(Faction faction, int delta)
    {
        reputation[faction] = Math.Clamp(GetReputation(faction) + delta, -100, 100);
        return reputation[faction];
    }

    public bool HasFlag(string flag) => flags.Contains(flag);

    /// <summary>Ustawia flagę fabularną; true, jeśli była nowa.</summary>
    public bool SetFlag(string flag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flag);
        return flags.Add(flag);
    }

    public QuestProgress? GetQuest(QuestId id) => quests.GetValueOrDefault(id);

    public QuestProgress StartQuest(QuestId id)
    {
        var progress = new QuestProgress();
        quests[id] = progress;
        return progress;
    }

    private readonly List<Potion> inventory = new();
    private readonly List<Item> gear = new();
    private readonly HashSet<TalentId> talents = new();

    public IReadOnlyList<Potion> Inventory => inventory;

    public const int GearCapacity = 8;

    public Weapon? Weapon { get; private set; }

    public Armor? EquippedArmor { get; private set; }

    public Trinket? Trinket { get; private set; }

    /// <summary>Torba na niezałożone wyposażenie.</summary>
    public IReadOnlyList<Item> Gear => gear;

    public IReadOnlySet<TalentId> Talents => talents;

    public bool HasTalent(TalentId id) => talents.Contains(id);

    /// <summary>Poziomy talentów, które bohater już osiągnął, ale jeszcze nie wybrał.</summary>
    public IReadOnlyList<int> PendingTalentLevels() =>
        TalentCatalog.Levels.Where(l => Level >= l && !talents.Any(t => TalentCatalog.Get(t).Level == l)).ToList();

    public void ChooseTalent(TalentId id)
    {
        TalentDefinition talent = TalentCatalog.Get(id);
        if (talent.Class != HeroClass || !PendingTalentLevels().Contains(talent.Level))
        {
            throw new InvalidOperationException("Ten talent nie jest dostępny.");
        }

        talents.Add(id);
        ClampHp();
    }

    public bool AddGear(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (gear.Count >= GearCapacity)
        {
            return false;
        }

        gear.Add(item);
        return true;
    }

    public bool RemoveGear(Item item) => gear.Remove(item);

    public bool CanEquip(Item item) => item is not Weapon weapon || weapon.ForClass == HeroClass;

    /// <summary>Zakłada przedmiot z torby; poprzedni z tego slotu wraca do torby. Zwraca zdjęty przedmiot.</summary>
    public Item? Equip(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!gear.Contains(item))
        {
            throw new InvalidOperationException("Przedmiotu nie ma w torbie.");
        }

        if (!CanEquip(item))
        {
            throw new InvalidOperationException("Ta broń nie pasuje do twojej klasy.");
        }

        gear.Remove(item);
        Item? previous = SetSlot(item);
        if (previous is not null)
        {
            gear.Add(previous);
        }

        ClampHp();
        return previous;
    }

    /// <summary>Zdejmuje przedmiot do torby. False, gdy torba pełna lub slot pusty.</summary>
    public bool Unequip(ItemSlot slot)
    {
        Item? current = slot switch
        {
            ItemSlot.Weapon => Weapon,
            ItemSlot.Armor => EquippedArmor,
            _ => Trinket,
        };
        if (current is null || gear.Count >= GearCapacity)
        {
            return false;
        }

        ClearSlot(slot);
        gear.Add(current);
        ClampHp();
        return true;
    }

    private Item? SetSlot(Item item)
    {
        switch (item)
        {
            case Weapon weapon:
                Item? oldWeapon = Weapon;
                Weapon = weapon;
                return oldWeapon;
            case Armor armor:
                Item? oldArmor = EquippedArmor;
                EquippedArmor = armor;
                return oldArmor;
            case Trinket trinket:
                Item? oldTrinket = Trinket;
                Trinket = trinket;
                return oldTrinket;
            default:
                throw new ArgumentOutOfRangeException(nameof(item));
        }
    }

    private void ClearSlot(ItemSlot slot)
    {
        switch (slot)
        {
            case ItemSlot.Weapon:
                Weapon = null;
                break;
            case ItemSlot.Armor:
                EquippedArmor = null;
                break;
            default:
                Trinket = null;
                break;
        }
    }

    /// <summary>Po zmianie sprzętu lub talentu maksymalne HP może zmaleć – bieżące HP nie może go przekraczać.</summary>
    private void ClampHp() => hp = Math.Clamp(hp, 0, MaxHp);

    public int TrinketPower(TrinketEffect effect) => Trinket?.Effect == effect ? Trinket.Power : 0;

    public int TalentPower(TalentEffect effect) => talents.Select(TalentCatalog.Get).Where(t => t.Effect == effect).Sum(t => t.Power);

    /// <summary>Procent więcej złota z walk (amulet + talent).</summary>
    public int GoldBonusPercent => TrinketPower(TrinketEffect.GoldFind) + TalentPower(TalentEffect.GoldPercent);

    public int ShopDiscountPercent => TrinketPower(TrinketEffect.ShopDiscount);

    /// <summary>Mnożnik obrażeń bohatera (talenty).</summary>
    public double DamageMultiplier => (100 + TalentPower(TalentEffect.DamagePercent) + (Hp * 3 < MaxHp ? TalentPower(TalentEffect.DamageWhenLow) : 0)) / 100.0;

    public int Hp
    {
        get => hp;
        set => hp = Math.Clamp(value, 0, MaxHp);
    }

    // --- statystyki pochodne ---

    public int MaxHp => ((Vit * Definition.HpPerVit) + TrinketPower(TrinketEffect.MaxHp)) * (100 + TalentPower(TalentEffect.MaxHpPercent)) / 100;

    private int Power => (Str * Definition.StrWeight) + (Dex * Definition.DexWeight);

    public int MinDmg => Definition.BaseMinDmg + (Power / 4) + (Weapon?.MinDmgBonus ?? 0);

    public int MaxDmg => Definition.BaseMaxDmg + (Power / 2) + (Weapon?.MaxDmgBonus ?? 0);

    public double HitChance => CombatMath.ClampChance(Definition.BaseHitChance + (Level * 1.5) + ExtraHitChance + (Weapon?.HitBonus ?? 0) + TalentPower(TalentEffect.HitFlat));

    public double CritChance => Math.Clamp(Definition.BaseCritChance + Level + (Weapon?.CritBonus ?? 0) + TrinketPower(TrinketEffect.Crit) + TalentPower(TalentEffect.CritFlat), 0, 75);

    public int Armor => (Dex / Definition.ArmorDexDivisor) + (Definition.ArmorAddsLevel ? Level : 0) + (EquippedArmor?.ArmorBonus ?? 0) + TalentPower(TalentEffect.ArmorFlat);

    /// <summary>Szansa (w %) na uniknięcie ataku wroga.</summary>
    public int Evasion => Math.Clamp((Dex / Definition.EvasionDexDivisor) + (EquippedArmor?.EvasionBonus ?? 0) + TrinketPower(TrinketEffect.Evasion) + TalentPower(TalentEffect.EvasionFlat), 0, 50);

    /// <summary>Szansa (w %) na ucieczkę z walki.</summary>
    public int FleeChance => Math.Clamp(35 + Dex + TrinketPower(TrinketEffect.FleeChance) + TalentPower(TalentEffect.FleeFlat), 20, 90);

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

        int raw = (int)Math.Round(rng.Range(MinDmg, MaxDmg) * multiplier * DamageMultiplier);
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

    public void AddPotion(Potion potion)
    {
        ArgumentNullException.ThrowIfNull(potion);
        inventory.Add(potion);
    }

    public int CountPotions(PotionKind kind) => inventory.Count(p => p.Kind == kind);

    /// <summary>Wypija miksturę danego rodzaju. Zwraca ilość faktycznie uleczonych HP lub null, gdy brak mikstury.</summary>
    public int? DrinkPotion(PotionKind kind)
    {
        Potion? potion = inventory.Find(p => p.Kind == kind);
        if (potion is null)
        {
            return null;
        }

        inventory.Remove(potion);
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
        Day = Day,
        Region = (int)CurrentRegion,
        Reputation = reputation.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
        Flags = flags.ToList(),
        Quests = quests.Select(kv => new QuestSaveEntry { Id = kv.Key.ToString(), Status = (int)kv.Value.Status, Progress = kv.Value.Progress }).ToList(),
        Weapon = Weapon is null ? null : (int)Weapon.Id,
        ArmorItem = EquippedArmor is null ? null : (int)EquippedArmor.Id,
        Trinket = Trinket is null ? null : (int)Trinket.Id,
        Gear = gear.Select(i => (int)i.Id).ToList(),
        Talents = talents.Select(t => (int)t).ToList(),
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
        foreach (int kind in data.Potions ?? [])
        {
            if (Enum.IsDefined((PotionKind)kind))
            {
                hero.AddPotion(Potion.Create((PotionKind)kind));
            }
        }

        hero.Day = Math.Max(1, data.Day);
        hero.CurrentRegion = Enum.IsDefined((RegionId)data.Region) ? (RegionId)data.Region : RegionId.Port;
        foreach ((string key, int value) in data.Reputation ?? [])
        {
            if (Enum.TryParse(key, out Faction faction))
            {
                hero.reputation[faction] = Math.Clamp(value, -100, 100);
            }
        }

        foreach (string flag in data.Flags ?? [])
        {
            if (!string.IsNullOrWhiteSpace(flag))
            {
                hero.flags.Add(flag);
            }
        }

        foreach (QuestSaveEntry entry in data.Quests ?? [])
        {
            if (Enum.TryParse(entry.Id, out QuestId questId) && Enum.IsDefined((QuestStatus)entry.Status))
            {
                hero.quests[questId] = new QuestProgress { Status = (QuestStatus)entry.Status, Progress = Math.Max(0, entry.Progress) };
            }
        }

        foreach (int id in data.Gear ?? [])
        {
            if (Enum.IsDefined((ItemId)id))
            {
                hero.AddGear(ItemCatalog.Get((ItemId)id));
            }
        }

        if (data.Weapon is int weaponId && Enum.IsDefined((ItemId)weaponId) && ItemCatalog.Get((ItemId)weaponId) is Weapon weapon && weapon.ForClass == hero.HeroClass)
        {
            hero.Weapon = weapon;
        }

        if (data.ArmorItem is int armorId && Enum.IsDefined((ItemId)armorId) && ItemCatalog.Get((ItemId)armorId) is Armor armor)
        {
            hero.EquippedArmor = armor;
        }

        if (data.Trinket is int trinketId && Enum.IsDefined((ItemId)trinketId) && ItemCatalog.Get((ItemId)trinketId) is Trinket trinket)
        {
            hero.Trinket = trinket;
        }

        foreach (int id in data.Talents ?? [])
        {
            if (Enum.IsDefined((TalentId)id) && TalentCatalog.Get((TalentId)id).Class == hero.HeroClass)
            {
                hero.talents.Add((TalentId)id);
            }
        }

        hero.Hp = data.Hp <= 0 ? hero.MaxHp : data.Hp;

        if (data.Version < 3)
        {
            // Zapis sprzed systemu zadań: główne zadania odblokowujące już osiągnięty etap uznajemy za wykonane.
            foreach (QuestDefinition quest in QuestCatalog.All.Where(q => q.IsMain && q.Reward.UnlocksStage is not null && q.Reward.UnlocksStage <= hero.Stage))
            {
                hero.quests[quest.Id] = new QuestProgress { Status = QuestStatus.Completed };
            }
        }

        return hero;
    }
}
