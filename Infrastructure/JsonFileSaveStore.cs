using System.Text.Json;

namespace PyramidTreasureConsoleRPG.Infrastructure;

public sealed record SaveInfo(string Name, HeroClass HeroClass, int Level, DateTime SavedAt);

public sealed record SaveLoadResult(SaveData? Data, string Message);

/// <summary>Magazyn zapisu gry. Zwraca surowe dane; bohatera odtwarza Hero.FromSaveData.</summary>
public interface ISaveStore
{
    bool Exists();

    SaveInfo? Peek();

    SaveLoadResult Load();

    /// <summary>Zwraca true przy powodzeniu; komunikat opisuje wynik (np. błąd dysku).</summary>
    bool Save(SaveData data, out string message);
}

public sealed class JsonFileSaveStore : ISaveStore
{
    public const string FileName = "DataSave.json";

    private readonly string folder;
    private readonly TimeProvider clock;

    public JsonFileSaveStore(string folder, TimeProvider? clock = null)
    {
        this.folder = folder ?? throw new ArgumentNullException(nameof(folder));
        this.clock = clock ?? TimeProvider.System;
    }

    public string FilePath => Path.Combine(folder, FileName);

    public bool Exists() => File.Exists(FilePath);

    public bool Save(SaveData data, out string message)
    {
        ArgumentNullException.ThrowIfNull(data);
        try
        {
            Directory.CreateDirectory(folder);
            data.SavedAt = clock.GetLocalNow().DateTime;
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, GameJsonContext.Default.SaveData));
            message = "Gra została zapisana!";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            message = $"Nie udało się zapisać gry: {ex.Message}";
            return false;
        }
    }

    public SaveInfo? Peek()
    {
        SaveData? data = Load().Data;
        if (data is null || !Enum.IsDefined((HeroClass)data.Class))
        {
            return null;
        }

        return new SaveInfo(data.Name, (HeroClass)data.Class, data.Level, data.SavedAt);
    }

    public SaveLoadResult Load()
    {
        if (!Exists())
        {
            return new SaveLoadResult(null, "Brak zapisanej gry.");
        }

        try
        {
            SaveData? data = JsonSerializer.Deserialize(File.ReadAllText(FilePath), GameJsonContext.Default.SaveData);
            if (data is null)
            {
                return new SaveLoadResult(null, "Plik zapisu jest pusty.");
            }

            if (data.Version < SaveData.OldestSupportedVersion || data.Version > SaveData.CurrentVersion)
            {
                return new SaveLoadResult(null, $"Plik zapisu pochodzi z innej wersji gry (wersja {data.Version}, obsługiwane {SaveData.OldestSupportedVersion}-{SaveData.CurrentVersion}). Zacznij nową grę.");
            }

            return new SaveLoadResult(data, string.Empty);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new SaveLoadResult(null, $"Nie udało się odczytać zapisu: {ex.Message}");
        }
    }
}
