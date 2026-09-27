namespace PyramidTreasureConsoleRPG.Tests;

public class CatalogTests
{
    [Fact]
    public void EveryEnemyDefinition_IsSane_AndNamesAreUnique()
    {
        Assert.All(EnemyCatalog.All, e =>
        {
            Assert.True(e.MaxHp > 0, e.Name);
            Assert.True(e.MinDmg > 0 && e.MaxDmg >= e.MinDmg, e.Name);
            Assert.True(e.Armor >= 0 && e.Gold >= 0 && e.Exp >= 0, e.Name);
            Assert.Equal(e.MaxHp, e.Spawn().Hp);
        });
        Assert.Equal(EnemyCatalog.All.Count, EnemyCatalog.All.Select(e => e.Name).Distinct().Count());
        Assert.True(EnemyCatalog.Ra.IsBoss);
        Assert.False(EnemyCatalog.Thief.IsBoss);
    }

    [Fact]
    public void EveryHeroClass_HasDefinition_AndCreatesMatchingHero()
    {
        foreach (HeroClass kind in Enum.GetValues<HeroClass>())
        {
            HeroClassDefinition def = HeroClasses.Get(kind);
            Assert.Equal(kind, def.Kind);
            Hero hero = Hero.Create(kind, "X");
            Assert.Same(def, hero.Definition);
            Assert.Equal(def.Name, hero.ClassName);
            Assert.True(def.ArmorDexDivisor > 0 && def.EvasionDexDivisor > 0);
        }

        Assert.Equal(3, HeroClasses.All.Count);
    }

    [Fact]
    public void Potions_AreOrderedByPriceAndPower()
    {
        Assert.Equal(new[] { PotionKind.Small, PotionKind.Medium, PotionKind.Large }, Potion.AllKinds);
        Assert.True(Potion.Small.Price < Potion.Medium.Price && Potion.Medium.Price < Potion.Large.Price);
        Assert.True(Potion.Small.RestoreHp < Potion.Medium.RestoreHp && Potion.Medium.RestoreHp < Potion.Large.RestoreHp);
        Assert.Same(Potion.Large, Potion.Create(PotionKind.Large));
    }
}
