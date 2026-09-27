namespace PyramidTreasureConsoleRPG.Tests;

public class HeroTests
{
    public static IEnumerable<object[]> Classes => new[]
    {
        new object[] { HeroClass.Warrior },
        [HeroClass.Archer],
        [HeroClass.Assassin],
    };

    [Theory]
    [MemberData(nameof(Classes))]
    public void NewHero_StartsAtLevelOneWithFullHp(HeroClass heroClass)
    {
        Hero hero = Hero.Create(heroClass, "Test");
        Assert.Equal(1, hero.Level);
        Assert.Equal(hero.MaxHp, hero.Hp);
        Assert.True(hero.MinDmg >= 1);
        Assert.True(hero.MaxDmg >= hero.MinDmg);
    }

    [Theory]
    [MemberData(nameof(Classes))]
    public void AddExp_LevelsUpMultipleTimes_AndCapsAtMaxLevel(HeroClass heroClass)
    {
        Hero hero = Hero.Create(heroClass, "Test");
        int gained = hero.AddExp(1_000_000);
        Assert.Equal(CombatMath.MaxLevel, hero.Level);
        Assert.Equal(CombatMath.MaxLevel - 1, gained);
        Assert.Equal(0, hero.Exp);
        Assert.Equal(0, hero.AddExp(500));
    }

    [Theory]
    [MemberData(nameof(Classes))]
    public void LevelUp_HealsOnlyByMaxHpIncrease(HeroClass heroClass)
    {
        Hero hero = Hero.Create(heroClass, "Test");
        hero.TakeDamage(hero.MaxHp - 1);
        int oldMax = hero.MaxHp;
        hero.AddExp(hero.ExpToNextLevel);
        Assert.Equal(2, hero.Level);
        Assert.Equal(1 + (hero.MaxHp - oldMax), hero.Hp);
    }

    [Theory]
    [MemberData(nameof(Classes))]
    public void NoClassIsImmuneToTheFinalBoss(HeroClass heroClass)
    {
        Hero hero = Hero.Create(heroClass, "Test");
        hero.AddExp(1_000_000);
        var ra = EnemyCatalog.Ra.Spawn();
        int minimumHit = CombatMath.ReduceByArmor(ra.MinDmg, hero.Armor);
        Assert.True(minimumHit >= 10, $"{heroClass} na 20. poziomie dostaje od Ra tylko {minimumHit} obrażeń – pancerz {hero.Armor} jest za wysoki.");
        Assert.True(hero.Evasion <= 40);
        Assert.True(hero.HitChance <= 95);
    }

    [Fact]
    public void Hp_IsClampedToMaxHp()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.Heal(10_000);
        Assert.Equal(hero.MaxHp, hero.Hp);
        hero.TakeDamage(10_000);
        Assert.Equal(0, hero.Hp);
        Assert.False(hero.IsAlive);
    }

    [Fact]
    public void DrinkPotion_HealsAndRemovesPotion_ButNotAboveMax()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        hero.AddPotion(Potion.Small);
        hero.TakeDamage(20);
        int? healed = hero.DrinkPotion(PotionKind.Small);
        Assert.Equal(20, healed);
        Assert.Equal(hero.MaxHp, hero.Hp);
        Assert.Empty(hero.Inventory);
        Assert.Null(hero.DrinkPotion(PotionKind.Small));
    }

    [Fact]
    public void Attack_NeverHealsEnemy()
    {
        var rng = new SeededRandomSource(7);
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        for (int i = 0; i < 500; i++)
        {
            var enemy = EnemyCatalog.Thief.Spawn();
            int before = enemy.Hp;
            AttackResult result = hero.Attack((AttackKind)(1 + (i % 3)), enemy, rng);
            Assert.True(enemy.Hp <= before);
            Assert.All(result.Strikes, s => Assert.True(s.Damage >= 0));
            Assert.All(result.Strikes.Where(s => !s.Hit), s => Assert.Equal(0, s.Damage));
        }
    }

    [Fact]
    public void SpecialDrink_IncreasesChosenStatOnly()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        int str = hero.Str;
        int dex = hero.Dex;
        hero.IncreaseStat(StatKind.Strength, 5);
        Assert.Equal(str + 5, hero.Str);
        Assert.Equal(dex, hero.Dex);
    }
}
