namespace PyramidTreasureConsoleRPG.Tests;

public class StatusAndAbilityTests
{
    private static Hero MaxHero(HeroClass heroClass)
    {
        Hero hero = Hero.Create(heroClass, "Test");
        hero.AddExp(1_000_000);
        return hero;
    }

    [Fact]
    public void StatusList_TicksDamage_AndExpires()
    {
        var statuses = new StatusList();
        Assert.True(statuses.Add(StatusKind.Bleed, 2, 7));
        Assert.False(statuses.Add(StatusKind.Bleed, 1, 3));
        Assert.Equal(7, statuses.Get(StatusKind.Bleed)!.Power);
        var first = statuses.Tick();
        Assert.Single(first);
        Assert.Equal(7, first[0].Damage);
        Assert.False(first[0].Expired);
        var second = statuses.Tick();
        Assert.True(second[0].Expired);
        Assert.False(statuses.Has(StatusKind.Bleed));
        Assert.Empty(statuses.Tick());
    }

    [Fact]
    public void Guard_HalvesIncomingDamage_AndGrantsCrit()
    {
        Hero hero = MaxHero(HeroClass.Warrior);
        // hero first (0); enemy attack: evasion roll 99 (no dodge), hit-penalty roll 99, damage roll max
        var engine = new CombatEngine(hero, [EnemyCatalog.WildBoar.Spawn()], new ScriptedRandomSource(0, 99, 99, 16));
        engine.Begin();
        IReadOnlyList<CombatEvent> events = engine.HeroGuard();
        Assert.Contains(events, e => e is GuardEvent);
        EnemyAttackEvent attack = events.OfType<EnemyAttackEvent>().Single();
        int expectedFull = CombatMath.ReduceByArmor(16, hero.Armor);
        Assert.Equal((int)Math.Round(expectedFull * 0.5), attack.Result.Damage);
        Assert.False(hero.Statuses.Has(StatusKind.Guard));
        Assert.True(hero.GuaranteedCrit);

        // next strike: hit roll 0, crit roll 99 would normally miss the crit, but it is guaranteed
        var strike = engine.HeroAttack(AttackKind.Normal, engine.Alive[0]).OfType<StrikeEvent>().First();
        Assert.True(strike.Strike.Critical);
        Assert.False(hero.GuaranteedCrit);
    }

    [Fact]
    public void PoisonedBlade_AppliesPoison_ThatTicksOnEnemyTurn()
    {
        Hero hero = MaxHero(HeroClass.Assassin);
        // hero first (0); special hit (0); crit roll 99; damage roll; poison chance roll 0 -> applied
        var engine = new CombatEngine(hero, [EnemyCatalog.Templar.Spawn()], new ScriptedRandomSource(0, 0, 99, 0, 0, 99, 99, 0, 99));
        engine.Begin();
        Enemy templar = engine.Alive[0];
        IReadOnlyList<CombatEvent> events = engine.HeroAttack(AttackKind.Special, templar);
        Assert.Contains(events, e => e is StatusAppliedEvent { Kind: StatusKind.Poison, OnHero: false });
        Assert.Contains(events, e => e is StatusTickEvent { Kind: StatusKind.Poison, OnHero: false, Damage: > 0 });
    }

    [Fact]
    public void Thief_FleesWhenLow_KeepsGold_GivesHalfExp()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        Enemy thief = EnemyCatalog.Thief.Spawn();
        thief.Hp = 5;
        int gold = hero.Gold;
        // enemy first (99); flee roll 0 (< 50%): thief at 5/22 HP flees immediately
        var engine = new CombatEngine(hero, [thief], new ScriptedRandomSource(99, 0));
        IReadOnlyList<CombatEvent> events = engine.Begin();
        EnemyFledEvent fled = Assert.Single(events.OfType<EnemyFledEvent>());
        Assert.Equal(EnemyCatalog.Thief.Exp / 2, fled.Exp);
        Assert.Equal(CombatStatus.Victory, engine.Status);
        Assert.Empty(engine.Killed);
        Assert.Equal(gold, hero.Gold);
        Assert.Equal(EnemyCatalog.Thief.Exp / 2, hero.Exp);
    }

    [Fact]
    public void Thief_MayStandAndFight_WhenFleeRollFails()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        Enemy thief = EnemyCatalog.Thief.Spawn();
        thief.Hp = 5;
        // enemy first (99); flee roll 99 (>= 50%): thief attacks instead (evasion 99, penalty 99, damage roll)
        var engine = new CombatEngine(hero, [thief], new ScriptedRandomSource(99, 99, 99, 99, 5));
        IReadOnlyList<CombatEvent> events = engine.Begin();
        Assert.DoesNotContain(events, e => e is EnemyFledEvent);
        Assert.Contains(events, e => e is EnemyAttackEvent);
        Assert.False(thief.Fled);
    }

    [Fact]
    public void Anubis_ResurrectsOnce_WhileTwinLives()
    {
        Hero hero = MaxHero(HeroClass.Warrior);
        Enemy first = EnemyCatalog.Anubis.Spawn();
        Enemy second = EnemyCatalog.Anubis.Spawn();
        first.Hp = 1;
        // hero first (0), hit (0), no crit (99), damage roll
        var engine = new CombatEngine(hero, [first, second], new ScriptedRandomSource(0, 0, 99, 10, 99, 99, 99, 99, 99, 99));
        engine.Begin();
        IReadOnlyList<CombatEvent> events = engine.HeroAttack(AttackKind.Normal, first);
        Assert.Contains(events, e => e is ResurrectEvent);
        Assert.True(first.IsAlive);
        Assert.Equal(first.MaxHp * 3 / 10, first.Hp);
        Assert.Empty(engine.Killed);
    }

    [Fact]
    public void Ra_SummonsShadowsAtHalfHealth_AndTelegraphsSunEye()
    {
        Hero hero = MaxHero(HeroClass.Warrior);
        Enemy ra = EnemyCatalog.Ra.Spawn();
        ra.Hp = ra.MaxHp / 2;
        // enemy first (99): Ra summons instead of attacking
        var engine = new CombatEngine(hero, [ra], new ScriptedRandomSource(99));
        IReadOnlyList<CombatEvent> events = engine.Begin();
        Assert.Contains(events, e => e is SummonEvent { Summoned.Count: 2 });
        Assert.Equal(3, engine.Alive.Count);
        Assert.False(engine.CanFlee);

        ra.Hp = ra.MaxHp / 6;
        IReadOnlyList<CombatEvent> guard = engine.HeroGuard();
        Assert.Contains(guard, e => e is EnemyAbilityEvent a && a.Text.Contains("Oko Słońca", StringComparison.Ordinal));
        Assert.True(ra.Charging);
    }

    [Fact]
    public void Stun_MakesHeroLoseTurn()
    {
        Hero hero = MaxHero(HeroClass.Warrior);
        hero.Statuses.Add(StatusKind.Stun, 1);
        // hero first (0): hero turn starts stunned -> enemies act, then hero turn again
        var engine = new CombatEngine(hero, [EnemyCatalog.WildBoar.Spawn()], new ScriptedRandomSource(0, 99, 99, 10));
        hero.Statuses.Add(StatusKind.Stun, 1);
        IReadOnlyList<CombatEvent> events = engine.Begin();
        Assert.Contains(events, e => e is StunnedEvent { OnHero: true });
        Assert.Contains(events, e => e is EnemyAttackEvent);
        Assert.Equal(CombatStatus.InProgress, engine.Status);
        Assert.False(hero.Statuses.Has(StatusKind.Stun));
    }

    [Fact]
    public void Wail_FearsHeroAndHeals_EveryThirdTurn()
    {
        Hero hero = MaxHero(HeroClass.Archer);
        Enemy monk = EnemyCatalog.CryingMonk.Spawn();
        monk.Hp = 100;
        monk.Turn = 2;
        // enemy first (99): monk's turn 3 -> wail
        var engine = new CombatEngine(hero, [monk], new ScriptedRandomSource(99));
        IReadOnlyList<CombatEvent> events = engine.Begin();
        Assert.Contains(events, e => e is StatusAppliedEvent { Kind: StatusKind.Fear, OnHero: true });
        Assert.Equal(100 + (monk.MaxHp / 10), monk.Hp);
        Assert.True(hero.HitChance < CombatMath.ClampChance(95) || hero.Statuses.Has(StatusKind.Fear));
    }

    [Fact]
    public void ShieldBash_CannotStunTwiceInARow()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        while (hero.Level < 12)
        {
            hero.AddExp(hero.ExpToNextLevel);
        }

        // Trzech templariuszy, kostka zawsze 0: każdy blok tarczą (20%) wchodzi, każdy cios trafia.
        var engine = new CombatEngine(hero, [EnemyCatalog.Templar.Spawn(), EnemyCatalog.Templar.Spawn(), EnemyCatalog.Templar.Spawn()], new ScriptedRandomSource(Enumerable.Repeat(0, 400).ToArray()));
        engine.Begin();
        for (int turn = 0; turn < 3 && engine.Status == CombatStatus.InProgress; turn++)
        {
            IReadOnlyList<CombatEvent> events = engine.HeroGuard();
            int stunsLost = events.Count(e => e is StunnedEvent { OnHero: true });
            Assert.True(stunsLost <= 1, "bohater nie może stracić dwóch tur z rzędu przez ogłuszenie");
        }
    }

    [Fact]
    public void EnemyAttackEvent_CarriesHpAfterEachHit()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        var engine = new CombatEngine(hero, [EnemyCatalog.Thief.Spawn(), EnemyCatalog.Thief.Spawn()], new ScriptedRandomSource(99, 99, 99, 99, 99, 99, 99, 99));
        IReadOnlyList<CombatEvent> events = engine.HeroGuard();
        var attacks = events.OfType<EnemyAttackEvent>().Where(a => a.Result.Hit).ToList();
        Assert.True(attacks.Count >= 2);
        Assert.True(attacks[0].HeroHp > attacks[^1].HeroHp);
        Assert.Equal(hero.Hp, attacks[^1].HeroHp);
    }
}
