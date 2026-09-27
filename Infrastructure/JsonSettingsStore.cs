using System.Text.Json;

namespace PyramidTreasureConsoleRPG.Infrastructure;

public interface ISettingsStore
{
    GameSettings Load();

    /// <summary>Zwraca komunikat błędu albo null przy powodzeniu.</summary>
    string? Save(GameSettings settings);
}

public sealed class JsonSettingsStore : ISettingsStore
{
    public const string FileName = "settings.json";

    private readonly string folder;

    public JsonSettingsStore(string folder)
    {
        this.folder = folder ?? throw new ArgumentNullException(nameof(folder));
    }

    public string FilePath => Path.Combine(folder, FileName);

    public GameSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var loaded = JsonSerializer.Deserialize(File.ReadAllText(FilePath), GameJsonContext.Default.GameSettings);
                if (loaded is not null)
                {
                    loaded.Normalize();
                    return loaded;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Uszkodzone ustawienia – wracamy do domyślnych.
        }

        return new GameSettings();
    }

    public string? Save(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, GameJsonContext.Default.GameSettings));
            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"Nie udało się zapisać ustawień: {ex.Message}";
        }
    }
}
