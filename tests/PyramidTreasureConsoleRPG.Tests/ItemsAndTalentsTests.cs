namespace PyramidTreasureConsoleRPG.Tests;

public class ItemsAndTalentsTests
{
    [Fact]
    public void Catalogs_AreConsistent()
    {
        Assert.Equal(ItemCatalog.All.Count, ItemCatalog.All.Select(i => i.Id).Distinct().Count());
        Assert.Equal(Enum.GetValues<ItemId>().Length, ItemCatalog.All.Count);
        Assert.All(ItemCatalog.All, i => Assert.True(i.Price > 0 && i.SellPrice < i.Price, i.Name));
        foreach (HeroClass heroClass in Enum.GetValues<HeroClass>())
        {
            Assert.Equal(3, ItemCatalog.All.OfType<Weapon>().Count(w => w.ForClass == heroClass));
            foreach (int level in TalentCatalog.Levels)
            {
                Assert.Equal(2, TalentCatalog.ForClassAtLevel(heroClass, level).Count);
            }
        }

        Assert.All(RegionCatalog.All.Where(r => r.HasShop), r => Assert.NotEmpty(r.Stock));
        Assert.All(EnemyCatalog.All.SelectMany(e => e.LootTable), l => Assert.True(l.ChancePercent is > 0 and <= 100 && (l.Item.HasValue ^ l.Potion.HasValue)));
    }

    [Fact]
    public void Equip_ChangesDerivedStats_AndSwapsPreviousItemToBag()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        int baseMin = hero.MinDmg;
        int baseArmor = hero.Armor;
        var axe = (Weapon)ItemCatalog.Get(ItemId.MercenaryAxe);
        var sword = (Weapon)ItemCatalog.Get(ItemId.TemplarSword);
        var mail = (Armor)ItemCatalog.Get(ItemId.ChainMail);
        Assert.True(hero.AddGear(axe));
        Assert.True(hero.AddGear(sword));
        Assert.True(hero.AddGear(mail));

        Assert.Null(hero.Equip(axe));
        Assert.Equal(baseMin + axe.MinDmgBonus, hero.MinDmg);
        Assert.Same(axe, hero.Equip(sword));
        Assert.Contains(axe, hero.Gear);
        Assert.DoesNotContain(sword, hero.Gear);
        Assert.Equal(baseMin + sword.MinDmgBonus, hero.MinDmg);

        hero.Equip(mail);
        Assert.Equal(baseArmor + mail.ArmorBonus, hero.Armor);
        Assert.True(hero.Unequip(ItemSlot.Armor));
        Assert.Equal(baseArmor, hero.Armor);
        Assert.Contains(mail, hero.Gear);
    }

    [Fact]
    public void Equip_RejectsOtherClassWeapon_AndFullBag()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        var axe = ItemCatalog.Get(ItemId.MercenaryAxe);
        hero.AddGear(axe);
        Assert.False(hero.CanEquip(axe));
        Assert.Throws<InvalidOperationException>(() => hero.Equip(axe));
        Assert.Throws<InvalidOperationException>(() => hero.Equip(ItemCatalog.Get(ItemId.HuntingBow)));

        for (int i = hero.Gear.Count; i < Hero.GearCapacity; i++)
        {
            Assert.True(hero.AddGear(ItemCatalog.Get(ItemId.WolfBone)));
        }

        Assert.False(hero.AddGear(ItemCatalog.Get(ItemId.Scarab)));
    }

    [Fact]
    public void Scarab_RaisesMaxHp_AndUnequipClampsHp()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        int baseMax = hero.MaxHp;
        var scarab = ItemCatalog.Get(ItemId.Scarab);
        hero.AddGear(scarab);
        hero.Equip(scarab);
        Assert.Equal(baseMax + 60, hero.MaxHp);
        hero.FullHeal();
        Assert.Equal(baseMax + 60, hero.Hp);
        hero.Unequip(ItemSlot.Trinket);
        Assert.Equal(baseMax, hero.Hp);
    }

    [Fact]
    public void Talents_AreOfferedAtLevels_AndApplyEffects()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        Assert.Empty(hero.PendingTalentLevels());
        while (hero.Level < 10)
        {
            hero.AddExp(hero.ExpToNextLevel);
        }

        Assert.Equal([5, 10], hero.PendingTalentLevels());
        int armor = hero.Armor;
        hero.ChooseTalent(TalentId.WarriorWall);
        Assert.Equal(armor + 12, hero.Armor);
        Assert.Equal([10], hero.PendingTalentLevels());
        Assert.Throws<InvalidOperationException>(() => hero.ChooseTalent(TalentId.WarriorRage));
        Assert.Throws<InvalidOperationException>(() => hero.ChooseTalent(TalentId.ArcherShadow));
        hero.ChooseTalent(TalentId.WarriorVeteran);
        Assert.Empty(hero.PendingTalentLevels());
    }

    [Fact]
    public void Rage_OnlyBoostsDamageWhenLow()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        while (hero.Level < 5)
        {
            hero.AddExp(hero.ExpToNextLevel);
        }

        hero.ChooseTalent(TalentId.WarriorRage);
        Assert.Equal(1.0, hero.DamageMultiplier);
        hero.TakeDamage(hero.MaxHp * 3 / 4);
        Assert.Equal(1.25, hero.DamageMultiplier);
    }

    [Fact]
    public void Shop_PricesFollowReputationAndDiscount_AndSellingGivesForty()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        RegionDefinition port = RegionCatalog.Port;
        Assert.Equal(100, ShopService.PricePercent(hero, port));
        hero.AdjustReputation(Faction.Town, 40);
        Assert.Equal(90, ShopService.PricePercent(hero, port));
        hero.AdjustReputation(Faction.Town, -100);
        Assert.Equal(125, ShopService.PricePercent(hero, port));

        var ring = ItemCatalog.Get(ItemId.HasanRing);
        hero.AddGear(ring);
        hero.Equip(ring);
        Assert.Equal(115, ShopService.PricePercent(hero, port));

        hero.Gold = 1000;
        PurchaseResult bought = ShopService.BuyItem(hero, port, ItemCatalog.Get(ItemId.LeatherJerkin));
        Assert.Equal(PurchaseOutcome.Bought, bought.Outcome);
        Assert.Equal(115, bought.Price);
        Assert.Equal(885, hero.Gold);
        Assert.Equal(40, ShopService.Sell(hero, ItemCatalog.Get(ItemId.LeatherJerkin)));
        Assert.Equal(925, hero.Gold);
        Assert.Equal(0, ShopService.Sell(hero, ItemCatalog.Get(ItemId.LeatherJerkin)));

        hero.Gold = 0;
        Assert.Equal(PurchaseOutcome.NotEnoughGold, ShopService.BuyPotion(hero, port, PotionKind.Small).Outcome);
    }

    [Fact]
    public void Loot_DropsWithRolls_AndGoldBonusApplies()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        hero.AddExp(1_000_000);
        var amulet = ItemCatalog.Get(ItemId.GreedAmulet);
        hero.AddGear(amulet);
        hero.Equip(amulet);
        int gold = hero.Gold;
        // hero acts first (roll 0), hits (0), no crit (99), damage roll, kill; loot rolls 0 => everything drops
        var rng = new ScriptedRandomSource(0, 0, 99, 20, 0, 0, 0, 0, 0);
        var engine = new CombatEngine(hero, [EnemyCatalog.Thief.Spawn()], rng);
        engine.Begin();
        var events = engine.HeroAttack(AttackKind.Normal, engine.Alive[0]);
        Assert.Equal(CombatStatus.Victory, engine.Status);
        Assert.Equal(gold + (EnemyCatalog.Thief.Gold * 120 / 100), hero.Gold);
        Assert.Equal(2, events.OfType<LootEvent>().Count());
        Assert.Equal(1, hero.CountPotions(PotionKind.Small));
        Assert.Contains(hero.Gear, g => g.Id == ItemId.LeatherJerkin);
    }

    [Fact]
    public void SaveAndLoad_KeepsEquipmentAndTalents()
    {
        var folder = Path.Combine(Path.GetTempPath(), "PyramidItems_" + Guid.NewGuid().ToString("N"));
        var store = new JsonFileSaveStore(folder);
        try
        {
            Hero hero = Hero.Create(HeroClass.Archer, "Test");
            while (hero.Level < 5)
            {
                hero.AddExp(hero.ExpToNextLevel);
            }

            hero.ChooseTalent(TalentId.ArcherShadow);
            var bow = ItemCatalog.Get(ItemId.RecurveBow);
            hero.AddGear(bow);
            hero.Equip(bow);
            hero.AddGear(ItemCatalog.Get(ItemId.ChainMail));
            Assert.True(store.Save(hero.ToSaveData(), out _));

            Hero loaded = Hero.FromSaveData(store.Load().Data!);
            Assert.Equal(ItemId.RecurveBow, loaded.Weapon?.Id);
            Assert.Single(loaded.Gear);
            Assert.Contains(TalentId.ArcherShadow, loaded.Talents);
            Assert.Equal(hero.MinDmg, loaded.MinDmg);
            Assert.Equal(hero.Evasion, loaded.Evasion);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
