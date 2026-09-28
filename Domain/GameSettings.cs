using System.Text.Json.Serialization;

namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Ustawienia gracza. Zapis i odczyt robi ISettingsStore.</summary>
public sealed class GameSettings
{
    public bool MusicEnabled { get; set; } = true;

    /// <summary>Głośność 0–100.</summary>
    public int MusicVolume { get; set; } = 50;

    /// <summary>0 = brak pauz, 1 = szybko, 2 = normalnie, 3 = wolno.</summary>
    public int TextSpeed { get; set; } = 2;

    public bool AgeConfirmed { get; set; }

    /// <summary>Ostrzejszy język w dialogach.</summary>
    public bool ProfanityEnabled { get; set; } = true;

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

    public void Normalize()
    {
        MusicVolume = Math.Clamp(MusicVolume, 0, 100);
        TextSpeed = Math.Clamp(TextSpeed, 0, 3);
    }
}
