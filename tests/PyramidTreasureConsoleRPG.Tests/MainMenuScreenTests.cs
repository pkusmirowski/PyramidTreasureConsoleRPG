namespace PyramidTreasureConsoleRPG.Tests;

public sealed class MainMenuScreenTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "PyramidTests_" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        public GameSettings? Saved { get; private set; }

        public GameSettings Load() => new();

        public string? Save(GameSettings settings)
        {
            Saved = settings;
            return null;
        }
    }

    private sealed class SilentMusic : IMusicPlayer
    {
        public bool IsAvailable => false;

        public void Play(int volumePercent)
        {
        }

        public void StopPlayback()
        {
        }

        public void SetVolume(int volumePercent)
        {
        }
    }

    private static MainMenuScreen Build(ScriptedGameIO io, ISaveStore saves, GameSettings settings, FakeSettingsStore store)
    {
        var rng = new SeededRandomSource(1);
        var combat = new CombatScreen(io, rng);
        var questGiver = new QuestGiverScreen(io, rng, settings);
        var events = new EventScreen(io, rng, combat);
        var region = new RegionScreen(
            io,
            rng,
            saves,
            combat,
            new TavernScreen(io, new BarScreen(io, rng, questGiver, settings), new CasinoScreen(io, rng), new RestScreen(io, rng, new RestService(rng), new NpcScreen(io, combat), settings)),
            new ShopScreen(io, rng, settings),
            new InventoryScreen(io),
            events,
            questGiver,
            new QuestLogScreen(io),
            new MapScreen(io, rng, combat, events),
            new TalentScreen(io),
            new EndingScreen(io));
        return new MainMenuScreen(io, saves, store, settings, new SilentMusic(), region);
    }

    [Fact]
    public void LoadGame_ListsSlots_AndLoadsTheChosenOne()
    {
        var saves = new JsonFileSaveStore(folder);
        Hero first = Hero.Create(HeroClass.Warrior, "Pierwszy");
        Hero second = Hero.Create(HeroClass.Archer, "Drugi");
        Assert.True(saves.Save(first.ToSaveData(), 1, out _));
        Assert.True(saves.Save(second.ToSaveData(), 2, out _));

        // 2: wczytaj, 2: drugi slot, 10: wróć do menu (Port ma 10 opcji), 2: tak, 4: wyjdź
        var io = new ScriptedGameIO("2", "2", "10", "2", "4");
        var settings = new GameSettings { AgeConfirmed = true };
        Build(io, saves, settings, new FakeSettingsStore()).Run();

        Assert.Contains("Który zapis wczytać?", io.MenusShown);
        Assert.Contains(io.Output, line => line.Contains("Slot 1: Pierwszy, Wojownik", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("Slot 2: Drugi, Łucznik", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.StartsWith("Wczytano: Drugi, Łucznik", StringComparison.Ordinal));
        Assert.Contains(io.Output, line => line.Contains("Do zobaczenia", StringComparison.Ordinal) || line.Contains("Port Sokoła", StringComparison.Ordinal));
    }

    [Fact]
    public void LoadGame_BackAndEmptyStore_DoNotLoad()
    {
        var saves = new JsonFileSaveStore(folder);
        var settings = new GameSettings { AgeConfirmed = true };

        // brak zapisów: 2: wczytaj -> błąd; 4: wyjdź
        var io = new ScriptedGameIO("2", "4");
        Build(io, saves, settings, new FakeSettingsStore()).Run();
        Assert.Contains(io.Output, line => line.Contains("Brak zapisanej gry", StringComparison.Ordinal));
        Assert.DoesNotContain(io.Output, line => line.StartsWith("Wczytano", StringComparison.Ordinal));

        saves.Save(Hero.Create(HeroClass.Assassin, "Ktoś").ToSaveData(), ISaveStore.AutoSlot, out _);
        // 2: wczytaj, 2: wróć (autozapis + wróć), 4: wyjdź
        var io2 = new ScriptedGameIO("2", "2", "4");
        Build(io2, saves, settings, new FakeSettingsStore()).Run();
        Assert.Contains(io2.Output, line => line.StartsWith("Autozapis: Ktoś", StringComparison.Ordinal));
        Assert.DoesNotContain(io2.Output, line => line.StartsWith("Wczytano", StringComparison.Ordinal));
    }

    [Fact]
    public void Settings_ToggleProfanity_IsPersisted()
    {
        var saves = new JsonFileSaveStore(folder);
        var settings = new GameSettings { AgeConfirmed = true, ProfanityEnabled = true };
        var store = new FakeSettingsStore();
        // 3: ustawienia, 4: wulgaryzmy, 3: prędkość tekstu, 5: rysunki, 6: wróć, 4: wyjdź
        var io = new ScriptedGameIO("3", "4", "3", "5", "6", "4");
        Build(io, saves, settings, store).Run();

        Assert.False(settings.ProfanityEnabled);
        Assert.Equal(3, settings.TextSpeed);
        Assert.False(settings.ArtEnabled);
        Assert.False(io.ArtEnabled);
        Assert.Same(settings, store.Saved);
        Assert.Contains(io.Output, line => line.Contains("Wulgarny język w dialogach: wyłączony", StringComparison.Ordinal));
    }

    [Fact]
    public void AgeGate_Refusal_EndsTheGame()
    {
        var settings = new GameSettings { AgeConfirmed = false };
        var store = new FakeSettingsStore();
        var io = new ScriptedGameIO("2");
        Build(io, new JsonFileSaveStore(folder), settings, store).Run();

        Assert.False(settings.AgeConfirmed);
        Assert.Null(store.Saved);
        Assert.DoesNotContain("Menu główne", io.MenusShown);
    }

    [Fact]
    public void NewGame_AsksForClassAndDifficulty_ShowsArt()
    {
        var settings = new GameSettings { AgeConfirmed = true };
        // 1: nowa gra, imię, 2: łucznik, 3: trudny, 10: wróć do menu, 2: tak, 4: wyjdź
        var io = new ScriptedGameIO("1", "Ela", "2", "3", "10", "2", "4");
        Build(io, new JsonFileSaveStore(folder), settings, new FakeSettingsStore()).Run();

        Assert.Contains("Poziom trudności:", io.MenusShown);
        Assert.Contains(io.Output, line => line.Contains("Ela, Łucznik (poziom trudności: Trudny)", StringComparison.Ordinal));
        Assert.Contains("Tytuł", io.ArtShown);
        Assert.Contains("Łucznik", io.ArtShown);
        Assert.Contains("Port Sokoła", io.ArtShown);
    }
}
