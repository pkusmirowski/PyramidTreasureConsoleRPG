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

public sealed record FightStartedEvent(Enemy Enemy, bool HeroActsFirst) : CombatEvent;

public sealed record StrikeEvent(Enemy Enemy, Strike Strike) : CombatEvent;

public sealed record EnemyAttackEvent(Enemy Enemy, EnemyAttackResult Result) : CombatEvent;

public sealed record PotionDrunkEvent(PotionKind Kind, int Healed) : CombatEvent;

public sealed record FleeAttemptEvent(Enemy Enemy, bool Success) : CombatEvent;

public sealed record EnemyDefeatedEvent(Enemy Enemy, int Gold, int Exp, int LevelsGained, int NewLevel) : CombatEvent;

public sealed record HeroDefeatedEvent(Enemy Enemy) : CombatEvent;

public sealed record VictoryEvent : CombatEvent;

/// <summary>
/// Stan i reguły jednej walki (bohater kontra kolejka wrogów). Nie zna konsoli: każda akcja
/// bohatera zwraca listę zdarzeń do wyświetlenia. Kolejność tur wynika z inicjatywy ustalanej
/// osobno dla każdego wroga.
/// </summary>
public sealed class CombatEngine
{
    private readonly Queue<Enemy> pending;
    private readonly IRandomSource rng;

    public CombatEngine(Hero hero, IEnumerable<Enemy> enemies, IRandomSource rng)
    {
        Hero = hero ?? throw new ArgumentNullException(nameof(hero));
        ArgumentNullException.ThrowIfNull(enemies);
        this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        pending = new Queue<Enemy>(enemies);
        if (pending.Count == 0)
        {
            throw new ArgumentException("Walka wymaga przynajmniej jednego wroga.", nameof(enemies));
        }

        CurrentEnemy = pending.Dequeue();
    }

    public Hero Hero { get; }

    public Enemy CurrentEnemy { get; private set; }

    public CombatStatus Status { get; private set; } = CombatStatus.InProgress;

    public bool HeroActsFirst { get; private set; }

    public bool CanFlee => !CurrentEnemy.IsBoss;

    /// <summary>Rozpoczyna walkę z pierwszym wrogiem. Jeśli wróg jest szybszy, od razu atakuje.</summary>
    public IReadOnlyList<CombatEvent> Begin()
    {
        var events = new List<CombatEvent>();
        StartEncounter(events);
        return events;
    }

    public IReadOnlyList<CombatEvent> HeroAttack(AttackKind kind)
    {
        EnsureInProgress();
        var events = new List<CombatEvent>();
        AttackResult result = Hero.Attack(kind, CurrentEnemy, rng);
        foreach (Strike strike in result.Strikes)
        {
            events.Add(new StrikeEvent(CurrentEnemy, strike));
        }

        FinishHeroTurn(events);
        return events;
    }

    /// <summary>Wypicie mikstury zużywa turę. Wywołujący sprawdza wcześniej, że bohater ją ma.</summary>
    public IReadOnlyList<CombatEvent> HeroDrinkPotion(PotionKind kind)
    {
        EnsureInProgress();
        int? healed = Hero.DrinkPotion(kind);
        if (healed is null)
        {
            throw new InvalidOperationException("Bohater nie ma takiej mikstury.");
        }

        var events = new List<CombatEvent> { new PotionDrunkEvent(kind, healed.Value) };
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

        var events = new List<CombatEvent>();
        bool success = rng.Chance(Hero.FleeChance);
        events.Add(new FleeAttemptEvent(CurrentEnemy, success));
        if (success)
        {
            Status = CombatStatus.Fled;
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

    private void StartEncounter(List<CombatEvent> events)
    {
        HeroActsFirst = rng.Chance(Math.Clamp(50 + Hero.Dex - CurrentEnemy.Agility, 20, 80));
        events.Add(new FightStartedEvent(CurrentEnemy, HeroActsFirst));
        if (!HeroActsFirst)
        {
            EnemyTurn(events);
        }
    }

    /// <summary>Po akcji bohatera: nagroda za pokonanego wroga albo kontratak.</summary>
    private void FinishHeroTurn(List<CombatEvent> events)
    {
        if (!CurrentEnemy.IsAlive)
        {
            Reward(events);
            return;
        }

        EnemyTurn(events);
    }

    private void EnemyTurn(List<CombatEvent> events)
    {
        EnemyAttackResult result = CurrentEnemy.Attack(Hero, rng);
        events.Add(new EnemyAttackEvent(CurrentEnemy, result));
        if (!Hero.IsAlive)
        {
            Status = CombatStatus.Defeat;
            events.Add(new HeroDefeatedEvent(CurrentEnemy));
        }
    }

    private void Reward(List<CombatEvent> events)
    {
        Enemy enemy = CurrentEnemy;
        Hero.Gold += enemy.Gold;
        int before = Hero.Level;
        int gained = enemy.Exp > 0 ? Hero.AddExp(enemy.Exp) : 0;
        events.Add(new EnemyDefeatedEvent(enemy, enemy.Gold, enemy.Exp, gained, before + gained));

        if (pending.Count == 0)
        {
            Status = CombatStatus.Victory;
            events.Add(new VictoryEvent());
            return;
        }

        CurrentEnemy = pending.Dequeue();
        StartEncounter(events);
    }
}
