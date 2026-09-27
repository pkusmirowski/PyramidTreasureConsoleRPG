namespace PyramidTreasureConsoleRPG.Tests;

public class CombatEngineTests
{
    public static IEnumerable<object[]> Classes => new[]
    {
        new object[] { HeroClass.Warrior },
        [HeroClass.Archer],
        [HeroClass.Assassin],
    };

    private static Hero MaxLevelHero(HeroClass heroClass)
    {
        Hero hero = Hero.Create(heroClass, "Test");
        hero.AddExp(1_000_000);
        return hero;
    }

    [Theory]
    [MemberData(nameof(Classes))]
    public void FinalBossFight_EndsWithinBoundedTurns_ForEveryClass(HeroClass heroClass)
    {
        int wins = 0;
        for (int seed = 0; seed < 20; seed++)
        {
            Hero hero = MaxLevelHero(heroClass);
            for (int i = 0; i < 5; i++)
            {
                hero.AddPotion(Potion.Large);
            }

            var engine = new CombatEngine(hero, Encounters.FinalBoss(), new SeededRandomSource(seed));
            engine.Begin();
            int turns = 0;
            while (engine.Status == CombatStatus.InProgress)
            {
                if (hero.Hp < hero.MaxHp / 3 && hero.CountPotions(PotionKind.Large) > 0)
                {
                    engine.HeroDrinkPotion(PotionKind.Large);
                }
                else
                {
                    engine.HeroAttack(AttackKind.Strong);
                }

                Assert.True(++turns < 500, "Walka z Ra nie kończy się.");
            }

            Assert.NotEqual(CombatStatus.Fled, engine.Status);
            if (engine.Status == CombatStatus.Victory)
            {
                wins++;
            }
        }

        Assert.True(wins >= 10, $"{heroClass} wygrywa z Ra tylko {wins}/20 razy – balans końcówki jest za ostry.");
    }

    [Fact]
    public void Potion_HealsAndPassesTurnToEnemy()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.AddPotion(Potion.Small);
        hero.TakeDamage(60);
        // heroFirst: 50 + Dex(2) - Agility(5) = 47 -> roll 10 < 47 => hero acts first
        var engine = new CombatEngine(hero, new[] { EnemyCatalog.Thief.Spawn() }, new ScriptedRandomSource(10, 0, 5));
        engine.Begin();
        Assert.True(engine.HeroActsFirst);

        int hpBefore = hero.Hp;
        IReadOnlyList<CombatEvent> events = engine.HeroDrinkPotion(PotionKind.Small);
        Assert.IsType<PotionDrunkEvent>(events[0]);
        Assert.IsType<EnemyAttackEvent>(events[1]);
        Assert.Empty(hero.Inventory);
        Assert.True(hero.Hp > hpBefore - 60 && hero.Hp <= hpBefore + 50);
    }

    [Fact]
    public void Flee_IsForbiddenAgainstBoss()
    {
        Hero hero = MaxLevelHero(HeroClass.Archer);
        var engine = new CombatEngine(hero, Encounters.PyramidGuards(), new SeededRandomSource(3));
        engine.Begin();
        Assert.False(engine.CanFlee);
        Assert.Throws<InvalidOperationException>(() => engine.HeroFlee());
    }

    [Fact]
    public void Flee_SuccessEndsFightWithoutReward()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        int gold = hero.Gold;
        // initiative roll 10 (hero first), flee roll 0 (< FleeChance)
        var engine = new CombatEngine(hero, new[] { EnemyCatalog.Wolf.Spawn(), EnemyCatalog.Wolf.Spawn() }, new ScriptedRandomSource(10, 0));
        engine.Begin();
        IReadOnlyList<CombatEvent> events = engine.HeroFlee();
        Assert.Equal(CombatStatus.Fled, engine.Status);
        Assert.True(((FleeAttemptEvent)events[0]).Success);
        Assert.Equal(gold, hero.Gold);
    }

    [Fact]
    public void KillingEveryEnemy_GivesRewardsAndVictory()
    {
        Hero hero = MaxLevelHero(HeroClass.Warrior);
        int gold = hero.Gold;
        var engine = new CombatEngine(hero, new[] { EnemyCatalog.Thief.Spawn(), EnemyCatalog.Thief.Spawn() }, new SeededRandomSource(5));
        engine.Begin();
        var all = new List<CombatEvent>();
        while (engine.Status == CombatStatus.InProgress)
        {
            all.AddRange(engine.HeroAttack(AttackKind.Normal));
        }

        Assert.Equal(CombatStatus.Victory, engine.Status);
        Assert.Equal(2, all.OfType<EnemyDefeatedEvent>().Count());
        Assert.Single(all.OfType<VictoryEvent>());
        Assert.Equal(gold + (2 * EnemyCatalog.Thief.Spawn().Gold), hero.Gold);
        Assert.Throws<InvalidOperationException>(() => engine.HeroAttack(AttackKind.Normal));
    }

    [Fact]
    public void EnemyActingFirst_AttacksBeforeHeroCanMove()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        // initiative roll 99 -> enemy first, enemy attack: evasion roll 99 (no dodge), damage roll
        var engine = new CombatEngine(hero, new[] { EnemyCatalog.Thief.Spawn() }, new ScriptedRandomSource(99, 99, 5));
        IReadOnlyList<CombatEvent> events = engine.Begin();
        Assert.False(engine.HeroActsFirst);
        Assert.IsType<FightStartedEvent>(events[0]);
        Assert.IsType<EnemyAttackEvent>(events[1]);
        Assert.True(hero.Hp < hero.MaxHp);
    }
}
