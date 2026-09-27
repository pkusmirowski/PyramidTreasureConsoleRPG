namespace PyramidTreasureConsoleRPG;

/// <summary>Menu główne: bramka wiekowa, nowa gra, wczytanie, ustawienia.</summary>
public sealed class Game
{
    private readonly GameSettings settings;
    private readonly MusicPlayer? music;
    private readonly string settingsFolder;

    public Game(GameSettings settings, MusicPlayer? music, string settingsFolder)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        this.music = music;
        this.settingsFolder = settingsFolder;
    }

    public void Run()
    {
        if (!AgeGate())
        {
            return;
        }

        while (true)
        {
            GameIO.Clear();
            GameIO.WriteLine("PYRAMID TREASURE – konsolowe RPG", ConsoleColor.Cyan);
            SaveInfo? save = SaveSystem.Peek();
            string load = save is null
                ? "Wczytaj grę (brak zapisu)"
                : $"Wczytaj grę ({save.Name}, poziom {save.Level}, {save.SavedAt:yyyy-MM-dd HH:mm})";
            GameIO.Menu("Menu główne", "Nowa gra", load, "Ustawienia", "Wyjdź z gry");
            switch (GameIO.ReadMenuChoice(4))
            {
                case 1:
                    GameIO.Clear();
                    Play(NewHero());
                    break;
                case 2:
                    GameIO.Clear();
                    Hero? loaded = SaveSystem.Load(out string message);
                    if (loaded is null)
                    {
                        GameIO.Error(message);
                        GameIO.PressAnyKey();
                    }
                    else
                    {
                        GameIO.Success(message);
                        Play(loaded);
                    }

                    break;
                case 3:
                    SettingsMenu();
                    break;
                case 4:
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

        GameIO.Clear();
        GameIO.WriteLine("UWAGA: gra zawiera treści przeznaczone dla osób dorosłych:", ConsoleColor.Red);
        GameIO.WriteLine("brutalne opisy przemocy, alkohol, hazard i wulgarny język.");
        GameIO.Menu("Czy masz ukończone 18 lat?", "Tak, mam 18 lat lub więcej", "Nie");
        if (GameIO.ReadMenuChoice(2) == 2)
        {
            GameIO.Info("Rozumiem. Wróć, gdy dorośniesz.");
            return false;
        }

        settings.AgeConfirmed = true;
        settings.Save(settingsFolder);
        return true;
    }

    private static Hero NewHero()
    {
        Dialogues.Intro();
        GameIO.WriteLine();
        string name = GameIO.ReadText("Podaj swoje imię: ");
        GameIO.Clear();
        GameIO.Menu(
            "Wybierz klasę:",
            "Wojownik – dużo zdrowia, ciężkie ciosy, pancerz rośnie z poziomem. Specjalność: trzystronne cięcie.",
            "Łucznik – obrażenia ze zręczności, najlepsze uniki, słabszy pancerz. Specjalność: podwójny strzał.",
            "Asasyn – najcelniejszy, najczęstsze krytyki. Specjalność: zatrute ostrze.");
        HeroClass heroClass = (HeroClass)GameIO.ReadMenuChoice(3);
        GameIO.Clear();
        Hero hero = Hero.Create(heroClass, name);
        GameIO.Success($"{hero.Name}, {hero.ClassName}, rusza na wyprawę po Graala. Zacznij od rozmowy z barmanem w tawernie.");
        return hero;
    }

    private static void Play(Hero hero)
    {
        SessionEnd end = new GameSession(hero).Run();
        GameIO.Clear();
        if (end == SessionEnd.Completed)
        {
            GameIO.Success("Dziękujemy za grę!");
            GameIO.PressAnyKey();
        }
    }

    private void SettingsMenu()
    {
        while (true)
        {
            GameIO.Clear();
            string musicState = music is null || !music.IsAvailable
                ? "niedostępna na tym systemie"
                : settings.MusicEnabled ? "włączona" : "wyłączona";
            GameIO.Menu(
                "Ustawienia",
                $"Muzyka: {musicState}",
                $"Głośność muzyki: {settings.MusicVolume}%",
                $"Prędkość tekstu: {GameSettings.TextSpeedName(settings.TextSpeed)}",
                "Wróć");
            switch (GameIO.ReadMenuChoice(4))
            {
                case 1:
                    settings.MusicEnabled = !settings.MusicEnabled;
                    ApplyMusic();
                    break;
                case 2:
                    settings.MusicVolume = GameIO.ReadChoice(0, 100, "Głośność (0-100): ");
                    music?.SetVolume(settings.MusicVolume);
                    break;
                case 3:
                    settings.TextSpeed = (settings.TextSpeed + 1) % 4;
                    GameIO.NarrationDelayMs = settings.NarrationDelayMs;
                    break;
                case 4:
                    settings.Save(settingsFolder);
                    return;
            }
        }
    }

    private void ApplyMusic()
    {
        if (music is null)
        {
            return;
        }

        if (settings.MusicEnabled)
        {
            music.Play(settings.MusicVolume);
        }
        else
        {
            music.Stop();
        }
    }
}
