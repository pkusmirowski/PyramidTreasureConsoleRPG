namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Menu główne: bramka wiekowa, nowa gra, wczytanie, ustawienia.</summary>
public sealed class MainMenuScreen(IGameIO io, ISaveStore saves, ISettingsStore settingsStore, GameSettings settings, IMusicPlayer music, RegionScreen region)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly ISaveStore saves = saves ?? throw new ArgumentNullException(nameof(saves));
    private readonly ISettingsStore settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
    private readonly GameSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly IMusicPlayer music = music ?? throw new ArgumentNullException(nameof(music));
    private readonly RegionScreen region = region ?? throw new ArgumentNullException(nameof(region));

    public void Run()
    {
        if (!AgeGate())
        {
            return;
        }

        io.ArtEnabled = settings.ArtEnabled;
        io.NarrationDelayMs = settings.NarrationDelayMs;
        while (true)
        {
            io.Clear();
            io.ShowArt(ArtCatalog.Title);
            io.WriteLine("PYRAMID TREASURE – konsolowe RPG", ConsoleColor.Cyan);
            string load = saves.AnyExists() ? "Wczytaj grę" : "Wczytaj grę (brak zapisu)";
            switch (io.Menu("Menu główne", "Nowa gra", load, "Ustawienia", "Wyjdź z gry"))
            {
                case 1:
                    io.Clear();
                    Play(NewHero());
                    break;
                case 2:
                    io.Clear();
                    LoadGame();
                    break;
                case 3:
                    SettingsMenu();
                    break;
                default:
                    return;
            }
        }
    }

    private bool AgeGate()
    {
        if (settings.AgeConfirmed)
        {
            return true;
        }

        io.Clear();
        io.WriteLine("UWAGA: gra zawiera treści przeznaczone dla osób dorosłych:", ConsoleColor.Red);
        io.WriteLine("brutalne opisy przemocy, alkohol, hazard i wulgarny język.");
        if (io.Menu("Czy masz ukończone 18 lat?", "Tak, mam 18 lat lub więcej", "Nie") == 2)
        {
            io.ShowInfo("Rozumiem. Wróć, gdy dorośniesz.");
            return false;
        }

        settings.AgeConfirmed = true;
        SaveSettings();
        return true;
    }

    private Hero NewHero()
    {
        io.Narrate(Dialogues.Intro);
        io.WriteLine();
        string name = io.ReadText("Podaj swoje imię: ");
        io.Clear();
        int choice = io.Menu("Wybierz klasę:", HeroClasses.All.Select(c => $"{c.Name} – {c.Description}").ToArray());
        io.Clear();
        int level = io.Menu("Poziom trudności:", DifficultyCatalog.All.Select(d => $"{d.Name} – {d.Description}").ToArray());
        io.Clear();
        Hero hero = Hero.Create(HeroClasses.All[choice - 1].Kind, name, DifficultyCatalog.All[level - 1].Kind);
        io.ShowArt(ArtCatalog.ForClass(hero.HeroClass));
        io.ShowSuccess($"{hero.Name}, {hero.ClassName} (poziom trudności: {hero.DifficultyDefinition.Name}), rusza na wyprawę po Graala. Zacznij od rozmowy z barmanem w tawernie.");
        return hero;
    }

    private void LoadGame()
    {
        var slots = new List<int>();
        var labels = new List<string>();
        for (int slot = ISaveStore.AutoSlot; slot <= ISaveStore.SlotCount; slot++)
        {
            if (saves.Peek(slot) is SaveInfo info)
            {
                slots.Add(slot);
                labels.Add($"{ISaveStore.SlotName(slot)}: {info.Name}, {HeroClasses.Get(info.HeroClass).Name}, poziom {info.Level}, {info.SavedAt:yyyy-MM-dd HH:mm}");
            }
        }

        if (slots.Count == 0)
        {
            io.ShowError("Brak zapisanej gry.");
            io.PressAnyKey();
            return;
        }

        labels.Add("Wróć");
        int choice = io.Menu("Który zapis wczytać?", [.. labels]);
        io.Clear();
        if (choice > slots.Count)
        {
            return;
        }

        SaveLoadResult result = saves.Load(slots[choice - 1]);
        if (result.Data is null)
        {
            io.ShowError(result.Message);
            io.PressAnyKey();
            return;
        }

        Hero hero;
        try
        {
            hero = Hero.FromSaveData(result.Data);
        }
        catch (InvalidDataException ex)
        {
            io.ShowError($"Plik zapisu jest uszkodzony: {ex.Message}");
            io.PressAnyKey();
            return;
        }

        io.ShowSuccess($"Wczytano: {hero.Name}, {hero.ClassName}, poziom {hero.Level}.");
        Play(hero);
    }

    private void Play(Hero hero)
    {
        while (true)
        {
            SessionEnd end = region.Run(hero);
            io.Clear();
            if (end != SessionEnd.Completed)
            {
                return;
            }

            io.ShowSuccess("Dziękujemy za grę!");
            if (io.Menu("Nowa gra+?", $"Tak – {hero.Name} rusza ponownie (start na poziomie {Hero.NewGamePlusStartLevel}, wrogowie +{30 * (hero.NewGamePlus + 1)}%)", "Nie, wróć do menu") == 2)
            {
                return;
            }

            hero = Hero.NewGamePlusFrom(hero);
            io.Clear();
            io.ShowSuccess($"Nowa gra+ (cykl {hero.NewGamePlus}): {hero.Name}, {hero.ClassName}, poziom {hero.Level}. Zachowujesz broń i talenty; wrogowie są silniejsi o {hero.EnemyScalePercent - 100}%.");
        }
    }

    private void SettingsMenu()
    {
        while (true)
        {
            io.Clear();
            string musicState = !music.IsAvailable ? "niedostępna na tym systemie" : settings.MusicEnabled ? "włączona" : "wyłączona";
            switch (io.Menu(
                "Ustawienia",
                $"Muzyka: {musicState}",
                $"Głośność muzyki: {settings.MusicVolume}%",
                $"Prędkość tekstu: {GameSettings.TextSpeedName(settings.TextSpeed)}",
                $"Wulgarny język w dialogach: {(settings.ProfanityEnabled ? "włączony" : "wyłączony")}",
                $"Rysunki ASCII: {(settings.ArtEnabled ? "włączone" : "wyłączone")}",
                "Wróć"))
            {
                case 1:
                    settings.MusicEnabled = !settings.MusicEnabled;
                    ApplyMusic();
                    break;
                case 2:
                    settings.MusicVolume = io.ReadNumber("Głośność (0-100): ", 0, 100);
                    music.SetVolume(settings.MusicVolume);
                    break;
                case 3:
                    settings.TextSpeed = (settings.TextSpeed + 1) % 4;
                    io.NarrationDelayMs = settings.NarrationDelayMs;
                    break;
                case 4:
                    settings.ProfanityEnabled = !settings.ProfanityEnabled;
                    break;
                case 5:
                    settings.ArtEnabled = !settings.ArtEnabled;
                    io.ArtEnabled = settings.ArtEnabled;
                    break;
                default:
                    SaveSettings();
                    return;
            }
        }
    }

    private void SaveSettings()
    {
        string? error = settingsStore.Save(settings);
        if (error is not null)
        {
            io.ShowError(error);
        }
    }

    private void ApplyMusic()
    {
        if (settings.MusicEnabled)
        {
            music.Play(settings.MusicVolume);
        }
        else
        {
            music.StopPlayback();
        }
    }
}
