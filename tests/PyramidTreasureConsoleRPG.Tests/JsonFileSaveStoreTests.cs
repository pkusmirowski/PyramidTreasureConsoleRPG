namespace PyramidTreasureConsoleRPG.Tests;

public sealed class JsonFileSaveStoreTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "PyramidTests_" + Guid.NewGuid().ToString("N"));
    private readonly JsonFileSaveStore store;

    public JsonFileSaveStoreTests()
    {
        store = new JsonFileSaveStore(folder);
    }

    public void Dispose()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Load_WithoutSaveFile_ReturnsNullWithMessage()
    {
        Assert.False(store.Exists());
        SaveLoadResult result = store.Load();
        Assert.Null(result.Data);
        Assert.Contains("Brak", result.Message, StringComparison.Ordinal);
        Assert.Null(store.Peek());
    }

    [Fact]
    public void SaveAndLoad_RoundTripsEverything()
    {
        Hero hero = Hero.Create(HeroClass.Assassin, "Zoja");
        hero.AddExp(7000);
        hero.TakeDamage(17);
        hero.Gold = 321;
        hero.Stage = StoryStage.CaravanAnnounced;
        hero.SpecialDrinkUsed = true;
        hero.AddPotion(Potion.Small);
        hero.AddPotion(Potion.Large);

        Assert.True(store.Save(hero.ToSaveData(), out _));
        SaveLoadResult result = store.Load();
        Assert.NotNull(result.Data);
        Hero loaded = Hero.FromSaveData(result.Data);

        Assert.Equal(hero.Level, loaded.Level);
        Assert.Equal(hero.Exp, loaded.Exp);
        Assert.Equal(hero.ExpToNextLevel, loaded.ExpToNextLevel);
        Assert.Equal(hero.Hp, loaded.Hp);
        Assert.Equal(hero.MaxHp, loaded.MaxHp);
        Assert.Equal(hero.MinDmg, loaded.MinDmg);
        Assert.Equal(hero.Armor, loaded.Armor);
        Assert.Equal(321, loaded.Gold);
        Assert.Equal(StoryStage.CaravanAnnounced, loaded.Stage);
        Assert.True(loaded.SpecialDrinkUsed);
        Assert.Equal(1, loaded.CountPotions(PotionKind.Small));
        Assert.Equal(1, loaded.CountPotions(PotionKind.Large));
        Assert.Equal(HeroClass.Assassin, loaded.HeroClass);

        SaveInfo? info = store.Peek();
        Assert.NotNull(info);
        Assert.Equal("Zoja", info.Name);
    }

    [Fact]
    public void Load_CorruptedJson_ReturnsNullInsteadOfThrowing()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(store.FilePath, "{ to nie jest json");
        SaveLoadResult result = store.Load();
        Assert.Null(result.Data);
        Assert.False(string.IsNullOrEmpty(result.Message));
    }

    [Fact]
    public void Load_UnknownClass_IsRejectedByHero()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(store.FilePath, "{\"Version\":2,\"Name\":\"X\",\"Class\":9,\"Level\":3}");
        SaveLoadResult result = store.Load();
        Assert.NotNull(result.Data);
        Assert.Throws<InvalidDataException>(() => Hero.FromSaveData(result.Data));
        Assert.Null(store.Peek());
    }

    [Fact]
    public void Load_OldVersion_IsRejected()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(store.FilePath, "{\"Version\":1,\"Name\":\"X\",\"Class\":1}");
        SaveLoadResult result = store.Load();
        Assert.Null(result.Data);
        Assert.Contains("wersji", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_Version2_MigratesQuestsFromStage()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(store.FilePath, "{\"Version\":2,\"Name\":\"Stary\",\"Class\":1,\"Level\":9,\"Vit\":31,\"Str\":28,\"Dex\":10,\"Hp\":100,\"Gold\":77,\"Stage\":2,\"Potions\":[1]}");
        SaveLoadResult result = store.Load();
        Assert.NotNull(result.Data);
        Hero hero = Hero.FromSaveData(result.Data);
        Assert.Equal(StoryStage.WolvesCleared, hero.Stage);
        Assert.Equal(QuestStatus.Completed, hero.GetQuest(QuestId.Bandits)?.Status);
        Assert.Equal(QuestStatus.Completed, hero.GetQuest(QuestId.Wolves)?.Status);
        Assert.Null(hero.GetQuest(QuestId.CaravanTrail));
        Assert.Equal(RegionId.Port, hero.CurrentRegion);
        Assert.Equal(1, hero.Day);
        Assert.Contains(QuestEngine.Available(hero, QuestGiverId.Barman), q => q.Id == QuestId.CaravanTrail);
    }

    [Fact]
    public void SaveAndLoad_RoundTripsWorldState()
    {
        Hero hero = Hero.Create(HeroClass.Warrior, "Wędrowiec");
        hero.Day = 17;
        hero.CurrentRegion = RegionId.Desert;
        hero.AdjustReputation(Faction.Brotherhood, 35);
        hero.SetFlag("map:nomads");
        QuestEngine.Accept(hero, QuestId.Bandits);
        QuestEngine.OnEnemyKilled(hero, EnemyCatalog.Thief);

        Assert.True(store.Save(hero.ToSaveData(), out _));
        Hero loaded = Hero.FromSaveData(store.Load().Data!);
        Assert.Equal(17, loaded.Day);
        Assert.Equal(RegionId.Desert, loaded.CurrentRegion);
        Assert.Equal(35, loaded.GetReputation(Faction.Brotherhood));
        Assert.True(loaded.HasFlag("map:nomads"));
        Assert.Equal(1, loaded.GetQuest(QuestId.Bandits)?.Progress);
        Assert.Equal(QuestStatus.Active, loaded.GetQuest(QuestId.Bandits)?.Status);
    }
}
