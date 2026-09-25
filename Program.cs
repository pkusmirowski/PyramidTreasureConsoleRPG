namespace PyramidTreasureConsoleRPG;

public static class Program
{
    public static int Main()
    {
        GameIO.Initialize();
        string dataFolder = SaveSystem.SaveFolder;
        GameSettings settings = GameSettings.Load(dataFolder);
        GameIO.NarrationDelayMs = settings.NarrationDelayMs;

        string musicPath = Path.Combine(AppContext.BaseDirectory, "Audio", "ancient_egypt.wav");
        using var music = new MusicPlayer(musicPath);
        if (settings.MusicEnabled)
        {
            music.Play(settings.MusicVolume);
        }

        try
        {
            new Game(settings, music, dataFolder).Run();
            GameIO.WriteLine("Do zobaczenia!");
            return 0;
        }
        catch (EndOfStreamException)
        {
            // Zamknięte wejście (np. potok) – kończymy spokojnie.
            return 0;
        }
    }
}
