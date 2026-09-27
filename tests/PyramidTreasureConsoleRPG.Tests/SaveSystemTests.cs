using Xunit;

namespace PyramidTreasureConsoleRPG.Tests;

public sealed class SaveSystemTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "PyramidTests_" + Guid.NewGuid().ToString("N"));

    public SaveSystemTests()
    {
        SaveSystem.SaveFolder = folder;
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
        Assert.False(SaveSystem.SaveExists());
        Hero? hero = SaveSystem.Load(out string message);
        Assert.Null(hero);
        Assert.Contains("Brak", message);
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
        hero.Inventory.Add(new SmallPotion());
        hero.Inventory.Add(new LargePotion());

        Assert.True(SaveSystem.Save(hero, out _));
        Hero? loaded = SaveSystem.Load(out string message);

        Assert.NotNull(loaded);
        Assert.Contains("Zoja", message);
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
    }

    [Fact]
    public void Load_CorruptedJson_ReturnsNullInsteadOfThrowing()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(SaveSystem.SaveFilePath, "{ to nie jest json");
        Hero? hero = SaveSystem.Load(out string message);
        Assert.Null(hero);
        Assert.False(string.IsNullOrEmpty(message));
    }

    [Fact]
    public void Load_UnknownClass_ReturnsNull()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(SaveSystem.SaveFilePath, "{\"Version\":2,\"Name\":\"X\",\"Class\":9,\"Level\":3}");
        Hero? hero = SaveSystem.Load(out string message);
        Assert.Null(hero);
        Assert.Contains("uszkodzony", message);
    }

    [Fact]
    public void Load_OldVersion_IsRejected()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(SaveSystem.SaveFilePath, "{\"Version\":1,\"Name\":\"X\",\"Class\":1}");
        Assert.Null(SaveSystem.Load(out string message));
        Assert.Contains("wersji", message);
    }
}
