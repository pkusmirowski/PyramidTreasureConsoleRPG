namespace PyramidTreasureConsoleRPG.Engine;

public enum CombatStatus
{
    InProgress,
    Victory,
    Defeat,
    Fled,
}

/// <summary>Zdarzenia walki. Silnik je produkuje, ekran je wyświetla.</summary>
public abstract record CombatEvent;

public sealed record FightStartedEvent(IReadOnlyList<Enemy> Enemies, bool HeroActsFirst) : CombatEvent;

public sealed record StrikeEvent(Enemy Enemy, Strike Strike) : CombatEvent;

/// <summary>Atak wroga; HeroHp to zdrowie bohatera tuż po tym ciosie (ekran renderuje zdarzenia po całej turze).</summary>
public sealed record EnemyAttackEvent(Enemy Enemy, EnemyAttackResult Result, int HeroHp) : CombatEvent;

public sealed record PotionDrunkEvent(PotionKind Kind, int Healed, int HeroHp) : CombatEvent;

public sealed record FleeAttemptEvent(bool Success) : CombatEvent;

public sealed record EnemyDefeatedEvent(Enemy Enemy, int Gold, int Exp, int LevelsGained, int NewLevel) : CombatEvent;

/// <summary>Łup z wroga. Kept = false, gdy torba była pełna i przedmiot przepadł.</summary>
public sealed record LootEvent(Enemy Enemy, Item? Item, Potion? Potion, bool Kept) : CombatEvent;

public sealed record HeroDefeatedEvent(Enemy Enemy) : CombatEvent;

public sealed record VictoryEvent : CombatEvent;

public sealed record StatusAppliedEvent(string TargetName, bool OnHero, StatusKind Kind, int Turns) : CombatEvent;

public sealed record StatusTickEvent(string TargetName, bool OnHero, StatusKind Kind, int Damage, bool Expired) : CombatEvent;

public sealed record StunnedEvent(string TargetName, bool OnHero) : CombatEvent;

public sealed record GuardEvent : CombatEvent;

public sealed record EnemyAbilityEvent(Enemy Enemy, string Text) : CombatEvent;

/// <summary>Wróg uciekł; łup przepada, ale połowa doświadczenia zostaje (pogoń też uczy).</summary>
public sealed record EnemyFledEvent(Enemy Enemy, int Exp, int LevelsGained, int NewLevel) : CombatEvent;

public sealed record SummonEvent(Enemy Summoner, IReadOnlyList<Enemy> Summoned) : CombatEvent;

public sealed record ResurrectEvent(Enemy Enemy) : CombatEvent;

/// <summary>
/// Walka grupowa: bohater kontra wszyscy żywi wrogowie naraz. Tura bohatera (atak na wybrany cel,
/// obrona, mikstura, ucieczka), potem tura każdego wroga z jego umiejętnością. Statusy tykają na
/// początku tury właściciela. Silnik nie zna konsoli – zwraca listy zdarzeń.
/// </summary>
public sealed class CombatEngine
{
    private readonly List<Enemy> enemies;
    private readonly IRandomSource rng;
    private readonly List<EnemyDefinition> killed = [];

    public CombatEngine(Hero hero, IEnumerable<Enemy> enemies, IRandomSource rng)
    {
        Hero = hero ?? throw new ArgumentNullException(nameof(hero));
        ArgumentNullException.ThrowIfNull(enemies);
        this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        this.enemies = enemies.ToList();
        if (this.enemies.Count == 0)
        {
            throw new ArgumentException("Walka wymaga przynajmniej jednego wroga.", nameof(enemies));
        }

        foreach (Enemy enemy in this.enemies)
        {
            enemy.Scale(Hero.EnemyScalePercent);
        }

        Hero.ClearCombatState();
        Hero.ApplyNextFightBuff();
        Hero.Stats.Fights++;
    }

    public Hero Hero { get; }

    /// <summary>Po straconej turze bohater nie da się ogłuszyć ponownie, dopóki sam nie zadziała (bez pętli ogłuszeń).</summary>
    private bool heroStunImmune;

    /// <summary>Wszyscy wrogowie, także pokonani i zbiegli.</summary>
    public IReadOnlyList<Enemy> Enemies => enemies;

    public IReadOnlyList<Enemy> Alive => enemies.Where(e => e.IsAlive && !e.Fled).ToList();

    public CombatStatus Status { get; private set; } = CombatStatus.InProgress;

    public bool HeroActsFirst { get; private set; }

    public bool CanFlee => !Alive.Any(e => e.IsBoss);

    /// <summary>Pokonani wrogowie (do zadań i statystyk).</summary>
    public IReadOnlyList<EnemyDefinition> Killed => killed;

    public IReadOnlyList<CombatEvent> Begin()
    {
        List<CombatEvent> events = [];
        int agility = (int)enemies.Average(e => e.Agility);
        HeroActsFirst = rng.Chance(Math.Clamp(50 + Hero.Dex - agility, 20, 80));
        events.Add(new FightStartedEvent(Alive, HeroActsFirst));
        if (!HeroActsFirst)
        {
            EnemiesTurn(events);
        }

        if (Status == CombatStatus.InProgress && !CheckVictory(events))
        {
            StartHeroTurn(events);
        }

        return events;
    }

    public IReadOnlyList<CombatEvent> HeroAttack(AttackKind kind, Enemy target)
    {
        EnsureInProgress();
        ArgumentNullException.ThrowIfNull(target);
        if (!target.IsAlive || target.Fled || !enemies.Contains(target))
        {
            throw new InvalidOperationException("Ten cel nie stoi już na polu walki.");
        }

        List<CombatEvent> events = [];
        AttackResult result = Hero.Attack(kind, target, rng);
        foreach (Strike strike in result.Strikes)
        {
            events.Add(new StrikeEvent(target, strike));
            if (strike.Hit && target.IsAlive && Hero.StatusOnHit(kind) is (StatusKind status, int turns, int power, int chance) && rng.Chance(chance))
            {
                if (target.Statuses.Add(status, turns, power))
                {
                    events.Add(new StatusAppliedEvent(target.Name, false, status, turns));
                }
            }
        }

        if (!target.IsAlive)
        {
            HandleEnemyDeath(target, events);
        }

        FinishHeroTurn(events);
        return events;
    }

    /// <summary>Obrona: połowa obrażeń do następnej tury, a następny cios bohatera jest krytyczny.</summary>
    public IReadOnlyList<CombatEvent> HeroGuard()
    {
        EnsureInProgress();
        List<CombatEvent> events = [];
        Hero.Statuses.Add(StatusKind.Guard, 1);
        Hero.GuaranteedCrit = true;
        events.Add(new GuardEvent());
        FinishHeroTurn(events);
        return events;
    }

    /// <summary>Wypicie mikstury zużywa turę. Wywołujący sprawdza wcześniej, że bohater ją ma.</summary>
    public IReadOnlyList<CombatEvent> HeroDrinkPotion(PotionKind kind)
    {
        EnsureInProgress();
        int? healed = Hero.DrinkPotion(kind) ?? throw new InvalidOperationException("Bohater nie ma takiej mikstury.");
        List<CombatEvent> events = [new PotionDrunkEvent(kind, healed.Value, Hero.Hp)];
        FinishHeroTurn(events);
        return events;
    }

    public IReadOnlyList<CombatEvent> HeroFlee()
    {
        EnsureInProgress();
        if (!CanFlee)
        {
            throw new InvalidOperationException("Od bossa nie można uciec.");
        }

        List<CombatEvent> events = [];
        bool success = rng.Chance(Hero.FleeChance);
        events.Add(new FleeAttemptEvent(success));
        if (success)
        {
            Finish(CombatStatus.Fled);
            return events;
        }

        FinishHeroTurn(events);
        return events;
    }

    private void EnsureInProgress()
    {
        if (Status != CombatStatus.InProgress)
        {
            throw new InvalidOperationException("Walka jest już zakończona.");
        }
    }

    private void Finish(CombatStatus status)
    {
        Status = status;
        Hero.ClearCombatState();
    }

    /// <summary>Po akcji bohatera: zwycięstwo albo tury wrogów, a potem początek kolejnej tury bohatera.</summary>
    private void FinishHeroTurn(List<CombatEvent> events)
    {
        heroStunImmune = false;
        if (CheckVictory(events))
        {
            return;
        }

        EnemiesTurn(events);
        if (Status == CombatStatus.InProgress && !CheckVictory(events))
        {
            StartHeroTurn(events);
        }
    }

    private bool CheckVictory(List<CombatEvent> events)
    {
        if (Alive.Count > 0)
        {
            return false;
        }

        events.Add(new VictoryEvent());
        Finish(CombatStatus.Victory);
        return true;
    }

    /// <summary>Statusy bohatera tykają; ogłuszony bohater traci turę, a wrogowie atakują ponownie.</summary>
    private void StartHeroTurn(List<CombatEvent> events)
    {
        bool stunned = Hero.Statuses.Has(StatusKind.Stun);
        foreach (StatusTick tick in Hero.Statuses.Tick())
        {
            if (tick.Damage > 0)
            {
                Hero.Hp -= tick.Damage;
            }

            events.Add(new StatusTickEvent(Hero.Name, true, tick.Kind, tick.Damage, tick.Expired));
        }

        if (!Hero.IsAlive)
        {
            IReadOnlyList<Enemy> alive = Alive;
            events.Add(new HeroDefeatedEvent(alive.Count > 0 ? alive[0] : enemies[0]));
            Finish(CombatStatus.Defeat);
            return;
        }

        if (stunned)
        {
            events.Add(new StunnedEvent(Hero.Name, true));
            heroStunImmune = true;
            EnemiesTurn(events);
            if (Status == CombatStatus.InProgress && !CheckVictory(events))
            {
                StartHeroTurn(events);
            }
        }
    }

    private void EnemiesTurn(List<CombatEvent> events)
    {
        foreach (Enemy enemy in enemies.ToList())
        {
            if (!enemy.IsAlive || enemy.Fled || Status != CombatStatus.InProgress)
            {
                continue;
            }

            EnemyTurn(enemy, events);
        }
    }

    private void EnemyTurn(Enemy enemy, List<CombatEvent> events)
    {
        enemy.Turn++;
        bool stunned = enemy.Statuses.Has(StatusKind.Stun);
        foreach (StatusTick tick in enemy.Statuses.Tick())
        {
            if (tick.Damage > 0)
            {
                enemy.Hp -= tick.Damage;
            }

            events.Add(new StatusTickEvent(enemy.Name, false, tick.Kind, tick.Damage, tick.Expired));
        }

        if (!enemy.IsAlive)
        {
            HandleEnemyDeath(enemy, events);
            return;
        }

        if (stunned)
        {
            events.Add(new StunnedEvent(enemy.Name, false));
            return;
        }

        if (UseAbilityInsteadOfAttack(enemy, events))
        {
            return;
        }

        double multiplier = AttackMultiplier(enemy, events);
        EnemyAttackResult result = enemy.Attack(Hero, rng, multiplier);
        events.Add(new EnemyAttackEvent(enemy, result, Hero.Hp));
        if (result.Hit)
        {
            OnEnemyHit(enemy, events);
        }

        if (!Hero.IsAlive)
        {
            events.Add(new HeroDefeatedEvent(enemy));
            Finish(CombatStatus.Defeat);
        }
    }

    /// <summary>Umiejętności, które zastępują zwykły atak w tej turze.</summary>
    private bool UseAbilityInsteadOfAttack(Enemy enemy, List<CombatEvent> events)
    {
        switch (enemy.Ability)
        {
            case EnemyAbility.Thievery when enemy.Hp * 10 < enemy.MaxHp * 3 && rng.Chance(50):
                enemy.Fled = true;
                int fleeExp = enemy.Exp / 2;
                int levelBefore = Hero.Level;
                int fleeLevels = fleeExp > 0 ? Hero.AddExp(fleeExp) : 0;
                events.Add(new EnemyFledEvent(enemy, fleeExp, fleeLevels, levelBefore + fleeLevels));
                return true;
            case EnemyAbility.Block when enemy.Turn % 2 == 1:
                enemy.Statuses.Add(StatusKind.Guard, 1);
                events.Add(new EnemyAbilityEvent(enemy, $"{enemy.Name} zasłania się tarczą – następny cios zada połowę obrażeń."));
                return false;
            case EnemyAbility.Wail when enemy.Turn % 3 == 0:
                int heal = enemy.MaxHp / 10;
                enemy.Hp += heal;
                if (Hero.Statuses.Add(StatusKind.Fear, 2))
                {
                    events.Add(new StatusAppliedEvent(Hero.Name, true, StatusKind.Fear, 2));
                }

                events.Add(new EnemyAbilityEvent(enemy, $"{enemy.Name} wyje lamentem, od którego drętwieją ręce. Leczy się o {heal}."));
                return true;
            case EnemyAbility.SunGod when enemy.AbilityUses == 0 && enemy.Hp * 2 <= enemy.MaxHp:
                enemy.AbilityUses = 1;
                var shadows = new List<Enemy> { EnemyCatalog.ShadowAnubis.Spawn(), EnemyCatalog.ShadowAnubis.Spawn() };
                enemies.AddRange(shadows);
                events.Add(new SummonEvent(enemy, shadows));
                return true;
            case EnemyAbility.SunGod when enemy.AbilityUses == 1 && enemy.Hp * 5 <= enemy.MaxHp:
                enemy.AbilityUses = 2;
                enemy.Charging = true;
                events.Add(new EnemyAbilityEvent(enemy, "Oko Słońca otwiera się. Ra zbiera moc – w następnej turze uderzy potrójnie. Broń się!"));
                return true;
            default:
                return false;
        }
    }

    private double AttackMultiplier(Enemy enemy, List<CombatEvent> events)
    {
        switch (enemy.Ability)
        {
            case EnemyAbility.Pack when PackBonus(enemy):
                return 1.2;
            case EnemyAbility.Charge when enemy.Turn % 3 == 0:
                events.Add(new EnemyAbilityEvent(enemy, $"{enemy.Name} szarżuje!"));
                return 2.0;
            case EnemyAbility.SunGod when enemy.Charging:
                enemy.Charging = false;
                events.Add(new EnemyAbilityEvent(enemy, "Oko Słońca uderza!"));
                return 3.0;
            default:
                return 1.0;
        }
    }

    private bool PackBonus(Enemy enemy) => enemy.Ability == EnemyAbility.Pack && Alive.Any(e => e != enemy && e.Definition == enemy.Definition);

    private void OnEnemyHit(Enemy enemy, List<CombatEvent> events)
    {
        switch (enemy.Ability)
        {
            case EnemyAbility.Thievery:
                int stolen = Hero.Gold / 20;
                if (stolen > 0)
                {
                    Hero.Gold -= stolen;
                    events.Add(new EnemyAbilityEvent(enemy, $"{enemy.Name} wyciąga ci z sakwy {stolen} złota."));
                }

                break;
            case EnemyAbility.Terrify when enemy.AbilityUses == 0:
                enemy.AbilityUses = 1;
                if (Hero.Statuses.Add(StatusKind.Fear, 2))
                {
                    events.Add(new StatusAppliedEvent(Hero.Name, true, StatusKind.Fear, 2));
                }

                break;
            case EnemyAbility.ShieldBash when rng.Chance(20):
                if (!heroStunImmune && Hero.Statuses.Add(StatusKind.Stun, 1))
                {
                    events.Add(new StatusAppliedEvent(Hero.Name, true, StatusKind.Stun, 1));
                }

                break;
            case EnemyAbility.Pack when PackBonus(enemy):
                events.Add(new EnemyAbilityEvent(enemy, "Stado atakuje razem – cios był mocniejszy."));
                break;
        }
    }

    private void HandleEnemyDeath(Enemy enemy, List<CombatEvent> events)
    {
        if (enemy.Ability == EnemyAbility.Resurrect && enemy.AbilityUses == 0 && Alive.Any(e => e != enemy && e.Ability == EnemyAbility.Resurrect))
        {
            enemy.AbilityUses = 1;
            enemy.Hp = enemy.MaxHp * 3 / 10;
            events.Add(new ResurrectEvent(enemy));
            return;
        }

        killed.Add(enemy.Definition);
        int gold = enemy.Gold * (100 + Hero.GoldBonusPercent) / 100;
        Hero.Gold += gold;
        Hero.Stats.Kills++;
        Hero.Stats.GoldEarned += gold;
        if (enemy.IsBoss)
        {
            Hero.Stats.BossKills++;
        }

        int before = Hero.Level;
        int gained = enemy.Exp > 0 ? Hero.AddExp(enemy.Exp) : 0;
        events.Add(new EnemyDefeatedEvent(enemy, gold, enemy.Exp, gained, before + gained));
        foreach (LootEntry entry in enemy.Definition.LootTable)
        {
            if (!rng.Chance(entry.ChancePercent))
            {
                continue;
            }

            if (entry.Potion is PotionKind kind)
            {
                Potion potion = Potion.Create(kind);
                Hero.AddPotion(potion);
                events.Add(new LootEvent(enemy, null, potion, true));
            }
            else if (entry.Item is ItemId itemId)
            {
                Item item = ItemCatalog.Get(itemId);
                events.Add(new LootEvent(enemy, item, null, Hero.AddGear(item)));
            }
        }
    }
}
