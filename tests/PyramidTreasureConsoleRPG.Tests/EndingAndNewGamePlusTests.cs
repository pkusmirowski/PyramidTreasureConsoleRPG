namespace PyramidTreasureConsoleRPG.Tests;

public sealed class EndingAndNewGamePlusTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "PyramidTests_" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Endings_GateOnReputationAndFlags()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        IReadOnlyList<EndingOption> options = EndingEngine.Options(hero);
        Assert.Equal(3, options.Count);
        Assert.True(options.Single(o => o.Ending.Kind == EndingKind.TakeGrail).Available);
        Assert.False(options.Single(o => o.Ending.Kind == EndingKind.GiveToBrotherhood).Available);
        Assert.False(options.Single(o => o.Ending.Kind == EndingKind.Destroy).Available);
        Assert.Throws<InvalidOperationException>(() => EndingEngine.Apply(hero, EndingKind.Destroy));

        hero.AdjustReputation(Faction.Brotherhood, EndingEngine.BrotherhoodReputationRequired);
        Assert.True(EndingEngine.Check(hero, EndingCatalog.GiveToBrotherhood).Available);

        Hero vowed = Hero.Create(HeroClass.Archer, "Test");
        vowed.SetFlag("neferet:vow");
        Assert.True(EndingEngine.Check(vowed, EndingCatalog.GiveToBrotherhood).Available);

        hero.SetFlag("map:nomads");
        Assert.True(EndingEngine.Check(hero, EndingCatalog.Destroy).Available);

        EndingDefinition applied = EndingEngine.Apply(hero, EndingKind.Destroy);
        Assert.Equal(EndingKind.Destroy, hero.Ending);
        Assert.True(hero.Completed);
        Assert.True(hero.HasFlag("ending:Destroy"));
        Assert.Equal(1, hero.Stats.LightChoices);
        Assert.Contains("KONIEC", applied.Text[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void EndingCatalog_IsConsistent()
    {
        Assert.Equal(Enum.GetValues<EndingKind>().Length, EndingCatalog.All.Count);
        foreach (EndingDefinition ending in EndingCatalog.All)
        {
            Assert.Same(ending, EndingCatalog.Get(ending.Kind));
            Assert.True(ending.Text.Count >= 4);
            Assert.False(string.IsNullOrWhiteSpace(ending.Choice));
            Assert.False(string.IsNullOrWhiteSpace(ending.NewGamePlusBonus));
        }
    }

    [Fact]
    public void EndingScreen_ShowsChoiceNarrationAndSummary()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Test");
        hero.Stats.Kills = 42;
        // 2: Bractwo (niedostępne) -> błąd i ponowny wybór; 1: zabierz Graal
        var io = new ScriptedGameIO("2", "1");
        EndingKind chosen = new EndingScreen(io).Run(hero);

        Assert.Equal(EndingKind.TakeGrail, chosen);
        Assert.Equal(EndingKind.TakeGrail, hero.Ending);
        Assert.Contains(io.Output, line => line.Contains("nie jest dostępne", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("GRAAL JEST MÓJ", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("Podsumowanie wyprawy", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("/ 42 /", StringComparison.Ordinal));
    }

    [Fact]
    public void NewGamePlus_KeepsWeaponAndTalents_StartsAtLevelFive_ScalesEnemies()
    {
        Hero previous = Hero.Create(HeroClass.Warrior, "Weteran");
        while (previous.Level < 10)
        {
            previous.AddExp(previous.ExpToNextLevel);
        }

        foreach (int level in previous.PendingTalentLevels().ToList())
        {
            previous.ChooseTalent(TalentCatalog.ForClassAtLevel(HeroClass.Warrior, level)[0].Id);
        }

        Item sword = ItemCatalog.Get(ItemId.TemplarSword);
        previous.AddGear(sword);
        previous.Equip(sword);
        previous.Gold = 9000;
        previous.SetFlag("map:nomads");
        EndingEngine.Apply(previous, EndingKind.TakeGrail);

        Hero next = Hero.NewGamePlusFrom(previous);
        Assert.Equal(1, next.NewGamePlus);
        Assert.Equal(Hero.NewGamePlusStartLevel, next.Level);
        Assert.Equal("Weteran", next.Name);
        Assert.Equal(HeroClass.Warrior, next.HeroClass);
        Assert.Same(sword, next.Weapon);
        Assert.Equal(previous.Talents.Count, next.Talents.Count);
        Assert.Empty(next.PendingTalentLevels());
        Assert.Equal(130, next.EnemyScalePercent);
        Assert.Equal(20, next.NightmareNights);
        Assert.True(next.HasFlag("ngplus:grail"));
        Assert.False(next.HasFlag("map:nomads"));
        Assert.Equal(next.MaxHp, next.Hp);
        Assert.Equal(0, next.Stats.Kills);
        Assert.True(next.Gold < previous.Gold);

        var engine = new CombatEngine(next, [EnemyCatalog.Thief.Spawn()], new SeededRandomSource(1));
        Enemy thief = engine.Enemies[0];
        Assert.Equal(EnemyCatalog.Thief.MaxHp * 130 / 100, thief.MaxHp);
        Assert.Equal(thief.MaxHp, thief.Hp);
        Assert.Equal(EnemyCatalog.Thief.MaxDmg * 130 / 100, thief.MaxDmg);

        Hero third = Hero.NewGamePlusFrom(next);
        Assert.Equal(2, third.NewGamePlus);
        Assert.Equal(160, third.EnemyScalePercent);
    }

    [Fact]
    public void NewGamePlus_BrotherhoodAndAshBonuses()
    {
        Hero brother = Hero.Create(HeroClass.Archer, "A");
        brother.SetFlag("neferet:vow");
        EndingEngine.Apply(brother, EndingKind.GiveToBrotherhood);
        Hero next = Hero.NewGamePlusFrom(brother);
        Assert.Equal(40, next.GetReputation(Faction.Brotherhood));
        Assert.True(next.HasFlag("neferet:wine"));

        Hero ash = Hero.Create(HeroClass.Assassin, "B");
        ash.SetFlag("map:nomads");
        EndingEngine.Apply(ash, EndingKind.Destroy);
        Hero next2 = Hero.NewGamePlusFrom(ash);
        Assert.Equal(40, next2.GetReputation(Faction.Town));
        Assert.Equal(400, next2.Gold);
    }

    [Fact]
    public void Enemy_ScaleDoesNotHealWoundedEnemy()
    {
        Enemy wolf = EnemyCatalog.Wolf.Spawn();
        wolf.Hp -= 10;
        int hp = wolf.Hp;
        wolf.Scale(130);
        Assert.Equal(hp, wolf.Hp);
        Assert.Equal(EnemyCatalog.Wolf.MaxHp * 130 / 100, wolf.MaxHp);
        wolf.Scale(90);
        Assert.Equal(130, wolf.ScalePercent);
    }

    [Fact]
    public void Statistics_CountFightsKillsGoldAndChoices()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Test");
        while (hero.Level < 5)
        {
            hero.AddExp(hero.ExpToNextLevel);
        }

        var engine = new CombatEngine(hero, [EnemyCatalog.Thief.Spawn()], new SeededRandomSource(2));
        engine.Begin();
        while (engine.Status == CombatStatus.InProgress)
        {
            engine.HeroAttack(AttackKind.Normal, engine.Alive[0]);
        }

        Assert.Equal(1, hero.Stats.Fights);
        Assert.True(hero.Stats.Kills + engine.Enemies.Count(e => e.Fled) >= 1);
        Assert.Equal(hero.Gold - 15, hero.Stats.GoldEarned);

        var notes = new List<string>();
        EventEngine.ApplyAll(hero, [new ReputationEffect(Faction.Underworld, 10), new ReputationEffect(Faction.Town, 2)], notes);
        Assert.Equal(1, hero.Stats.DarkChoices);
        EventEngine.ApplyAll(hero, [new ReputationEffect(Faction.Brotherhood, 5)], notes);
        Assert.Equal(1, hero.Stats.LightChoices);
        EventEngine.ApplyAll(hero, [new GoldEffect(5)], notes);
        Assert.Equal(1, hero.Stats.LightChoices);
        Assert.Equal(1, hero.Stats.DarkChoices);

        InterrogationService.Resolve(hero, PrisonerChoice.Release, RegionId.Port, new SeededRandomSource(1));
        Assert.Equal(2, hero.Stats.LightChoices);
        InterrogationService.Resolve(hero, PrisonerChoice.Execute, RegionId.Port, new SeededRandomSource(1));
        Assert.Equal(2, hero.Stats.DarkChoices);

        hero.AddPotion(Potion.Small);
        hero.DrinkPotion(PotionKind.Small);
        Assert.Equal(1, hero.Stats.PotionsDrunk);
    }

    [Fact]
    public void Save_RoundTripsStatisticsEndingAndCycle()
    {
        Hero hero = Hero.Create(HeroClass.Archer, "Test");
        hero.Stats.Fights = 7;
        hero.Stats.Kills = 9;
        hero.Stats.CasinoLost = 120;
        hero.Stats.DarkChoices = 3;
        hero.SetFlag("map:nomads");
        EndingEngine.Apply(hero, EndingKind.Destroy);
        Hero plus = Hero.NewGamePlusFrom(hero);
        plus.Stats.Kills = 1;

        SaveData data = plus.ToSaveData();
        Assert.True(SaveData.CurrentVersion >= 6);
        Assert.Equal(SaveData.CurrentVersion, data.Version);
        Hero loaded = Hero.FromSaveData(data);
        Assert.Equal(1, loaded.NewGamePlus);
        Assert.Equal(130, loaded.EnemyScalePercent);
        Assert.Equal(1, loaded.Stats.Kills);
        Assert.Null(loaded.Ending);

        SaveData finished = hero.ToSaveData();
        Hero loadedFinished = Hero.FromSaveData(finished);
        Assert.Equal(EndingKind.Destroy, loadedFinished.Ending);
        Assert.Equal(7, loadedFinished.Stats.Fights);
        Assert.Equal(120, loadedFinished.Stats.CasinoLost);

        // Zapis bez statystyk (starsza wersja) wczytuje się z zerami.
        Hero old = Hero.FromSaveData(new SaveData { Name = "Stary", Class = 1, Version = 5, Vit = 10 });
        Assert.Equal(0, old.Stats.Fights);
        Assert.Equal(0, old.NewGamePlus);
    }

    [Fact]
    public void SaveStore_HasThreeSlotsAndAutosave_WithLegacyFileNameForSlotOne()
    {
        var store = new JsonFileSaveStore(folder);
        Assert.False(((ISaveStore)store).AnyExists());
        Assert.EndsWith(JsonFileSaveStore.FileName, store.FilePathFor(1), StringComparison.Ordinal);
        Assert.NotEqual(store.FilePathFor(1), store.FilePathFor(2));
        Assert.NotEqual(store.FilePathFor(2), store.FilePathFor(ISaveStore.AutoSlot));
        Assert.Throws<ArgumentOutOfRangeException>(() => store.FilePathFor(ISaveStore.SlotCount + 1));

        Hero hero = Hero.Create(HeroClass.Warrior, "Slotowy");
        Assert.True(store.Save(hero.ToSaveData(), 2, out string message));
        Assert.Contains("Slot 2", message, StringComparison.Ordinal);
        Assert.True(((ISaveStore)store).AnyExists());
        Assert.False(store.Exists(1));
        Assert.True(store.Exists(2));
        Assert.Null(store.Peek(1));
        Assert.Equal("Slotowy", store.Peek(2)?.Name);

        Assert.True(store.Save(hero.ToSaveData(), ISaveStore.AutoSlot, out string auto));
        Assert.Contains("Autozapis", auto, StringComparison.Ordinal);
        Assert.Equal("Slotowy", Hero.FromSaveData(store.Load(ISaveStore.AutoSlot).Data!).Name);
        Assert.Equal("Autozapis", ISaveStore.SlotName(ISaveStore.AutoSlot));
    }
}
