using System.Text.Json;
using System.Text.Json.Serialization;

namespace PyramidTreasureConsoleRPG;

/// <summary>Ustawienia gracza zapisywane obok pliku zapisu gry.</summary>
public sealed class GameSettings
{
    public const string FileName = "settings.json";

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    public bool MusicEnabled { get; set; } = true;

    /// <summary>Głośność 0–100.</summary>
    public int MusicVolume { get; set; } = 50;

    /// <summary>0 = brak pauz, 1 = szybko, 2 = normalnie, 3 = wolno.</summary>
    public int TextSpeed { get; set; } = 2;

    public bool AgeConfirmed { get; set; }

    [JsonIgnore]
    public int NarrationDelayMs => TextSpeed switch
    {
        0 => 0,
        1 => 700,
        3 => 3000,
        _ => 1500,
    };

    public static string TextSpeedName(int speed) => speed switch
    {
        0 => "bez pauz",
        1 => "szybko",
        3 => "wolno",
        _ => "normalnie",
    };

    public static GameSettings Load(string folder)
    {
        string path = Path.Combine(folder, FileName);
        try
        {
            if (File.Exists(path))
            {
                var loaded = JsonSerializer.Deserialize<GameSettings>(File.ReadAllText(path));
                if (loaded is not null)
                {
                    loaded.MusicVolume = Math.Clamp(loaded.MusicVolume, 0, 100);
                    loaded.TextSpeed = Math.Clamp(loaded.TextSpeed, 0, 3);
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

    public void Save(string folder)
    {
        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, FileName), JsonSerializer.Serialize(this, WriteOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            GameIO.Error($"Nie udało się zapisać ustawień: {ex.Message}");
        }
    }
}
