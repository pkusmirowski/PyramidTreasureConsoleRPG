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
        Assert.Equal([PotionKind.Small, PotionKind.Medium, PotionKind.Large], Potion.Healing.Select(p => p.Kind));
        Assert.Equal(Enum.GetValues<PotionKind>().Length, Potion.AllKinds.Count);
        Assert.All(Potion.All, p => Assert.Same(p, Potion.Create(p.Kind)));
        Assert.All(Potion.All.Where(p => p.Effect != PotionUse.Heal), p => Assert.Equal(0, p.RestoreHp));
        Assert.True(Potion.Small.Price < Potion.Medium.Price && Potion.Medium.Price < Potion.Large.Price);
        Assert.True(Potion.Small.RestoreHp < Potion.Medium.RestoreHp && Potion.Medium.RestoreHp < Potion.Large.RestoreHp);
        Assert.Same(Potion.Large, Potion.Create(PotionKind.Large));
    }

    [Fact]
    public void EveryRegion_IsConsistent()
    {
        Assert.Equal(6, RegionCatalog.All.Count);
        Assert.Equal(1, RegionCatalog.Port.RecommendedLevel);
        Assert.Equal(RegionCatalog.All.Max(r => r.RecommendedLevel), RegionCatalog.Pyramid.RecommendedLevel);
        Assert.All(RegionCatalog.All, r => Assert.InRange(r.RecommendedLevel, 1, 20));
        Assert.All(RegionCatalog.All, r =>
        {
            Assert.Same(r, RegionCatalog.Get(r.Id));
            Assert.NotEmpty(r.Arrival);
            Assert.True(r.TravelDays >= 1 && r.TravelCost >= 0, r.Name);
            Assert.True(r.IsFinal || r.Encounters.Length > 0, r.Name);
            Assert.All(r.Encounters, g => Assert.NotEmpty(g));
        });
        Assert.True(RegionCatalog.Port.IsHome && RegionCatalog.Port.HasTavern);
    }

    [Fact]
    public void EveryEvent_HasChoicesAndUniqueId()
    {
        Assert.True(EventCatalog.All.Count >= 20);
        Assert.Equal(EventCatalog.All.Count, EventCatalog.All.Select(e => e.Id).Distinct().Count());
        Assert.All(EventCatalog.All, e =>
        {
            Assert.NotEmpty(e.Text);
            Assert.True(e.Choices.Count >= 2, e.Id);
            Assert.All(e.Choices, c =>
            {
                Assert.False(string.IsNullOrWhiteSpace(c.SuccessText), e.Id);
                if (c.Check is not null)
                {
                    Assert.NotNull(c.FailureText);
                }
            });
        });
        Assert.All(RegionCatalog.All.Where(r => !r.IsFinal), r => Assert.NotEmpty(EventCatalog.InRegion(r.Id)));
    }

    [Fact]
    public void EveryQuest_IsConsistent()
    {
        Assert.Equal(QuestCatalog.All.Count, QuestCatalog.All.Select(q => q.Id).Distinct().Count());
        Assert.All(QuestCatalog.All, q =>
        {
            Assert.NotEmpty(q.Intro);
            Assert.NotEmpty(q.Completion);
            Assert.True(q.Reward.Gold > 0 && q.Reward.Exp > 0, q.Title);
            Assert.Contains(q.Giver, RegionCatalog.All.SelectMany(r => r.QuestGivers));
        });
        var mainStages = QuestCatalog.All.Where(q => q.IsMain).Select(q => q.Reward.UnlocksStage).ToList();
        Assert.Equal([StoryStage.BanditsCalmed, StoryStage.WolvesCleared, StoryStage.CaravanAnnounced, StoryStage.CaravanReady], mainStages);
    }
}
