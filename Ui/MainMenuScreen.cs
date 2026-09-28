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

        while (true)
        {
            io.Clear();
            io.WriteLine("PYRAMID TREASURE – konsolowe RPG", ConsoleColor.Cyan);
            SaveInfo? save = saves.Peek();
            string load = save is null
                ? "Wczytaj grę (brak zapisu)"
                : $"Wczytaj grę ({save.Name}, poziom {save.Level}, {save.SavedAt:yyyy-MM-dd HH:mm})";
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
        Hero hero = Hero.Create(HeroClasses.All[choice - 1].Kind, name);
        io.ShowSuccess($"{hero.Name}, {hero.ClassName}, rusza na wyprawę po Graala. Zacznij od rozmowy z barmanem w tawernie.");
        return hero;
    }

    private void LoadGame()
    {
        SaveLoadResult result = saves.Load();
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
        SessionEnd end = region.Run(hero);
        io.Clear();
        if (end == SessionEnd.Completed)
        {
            io.ShowSuccess("Dziękujemy za grę!");
            io.PressAnyKey();
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
