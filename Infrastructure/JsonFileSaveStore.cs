using System.Text.Json;

namespace PyramidTreasureConsoleRPG.Infrastructure;

public sealed record SaveInfo(string Name, HeroClass HeroClass, int Level, DateTime SavedAt);

public sealed record SaveLoadResult(SaveData? Data, string Message);

/// <summary>Magazyn zapisu gry. Zwraca surowe dane; bohatera odtwarza Hero.FromSaveData.</summary>
public interface ISaveStore
{
    /// <summary>Slot autozapisu (przed piramidą).</summary>
    const int AutoSlot = 0;

    /// <summary>Liczba ręcznych slotów (1..SlotCount).</summary>
    const int SlotCount = 3;

    bool Exists(int slot = 1);

    SaveInfo? Peek(int slot = 1);

    SaveLoadResult Load(int slot = 1);

    /// <summary>Zwraca true przy powodzeniu; komunikat opisuje wynik (np. błąd dysku).</summary>
    bool Save(SaveData data, int slot, out string message);

    bool Save(SaveData data, out string message) => Save(data, 1, out message);

    /// <summary>Czy jakikolwiek slot (łącznie z autozapisem) ma zapis.</summary>
    bool AnyExists()
    {
        for (int slot = AutoSlot; slot <= SlotCount; slot++)
        {
            if (Exists(slot))
            {
                return true;
            }
        }

        return false;
    }

    static string SlotName(int slot) => slot == AutoSlot ? "Autozapis" : $"Slot {slot}";
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

    /// <summary>Ścieżka slotu 1 – ta sama co w starszych wersjach gry, żeby stare zapisy nadal działały.</summary>
    public string FilePath => FilePathFor(1);

    public string FilePathFor(int slot)
    {
        ValidateSlot(slot);
        string name = slot switch
        {
            ISaveStore.AutoSlot => "DataSaveAuto.json",
            1 => FileName,
            _ => $"DataSave{slot}.json",
        };
        return Path.Combine(folder, name);
    }

    public bool Exists(int slot = 1) => File.Exists(FilePathFor(slot));

    public bool Save(SaveData data, out string message) => Save(data, 1, out message);

    public bool Save(SaveData data, int slot, out string message)
    {
        ArgumentNullException.ThrowIfNull(data);
        try
        {
            Directory.CreateDirectory(folder);
            data.SavedAt = clock.GetLocalNow().DateTime;
            File.WriteAllText(FilePathFor(slot), JsonSerializer.Serialize(data, GameJsonContext.Default.SaveData));
            message = slot == ISaveStore.AutoSlot ? "Autozapis wykonany." : $"Gra została zapisana ({ISaveStore.SlotName(slot)})!";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            message = $"Nie udało się zapisać gry: {ex.Message}";
            return false;
        }
    }

    public SaveInfo? Peek(int slot = 1)
    {
        SaveData? data = Load(slot).Data;
        if (data is null || !Enum.IsDefined((HeroClass)data.Class))
        {
            return null;
        }

        return new SaveInfo(data.Name, (HeroClass)data.Class, data.Level, data.SavedAt);
    }

    public SaveLoadResult Load(int slot = 1)
    {
        if (!Exists(slot))
        {
            return new SaveLoadResult(null, "Brak zapisanej gry.");
        }

        try
        {
            SaveData? data = JsonSerializer.Deserialize(File.ReadAllText(FilePathFor(slot)), GameJsonContext.Default.SaveData);
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

    private static void ValidateSlot(int slot)
    {
        if (slot < ISaveStore.AutoSlot || slot > ISaveStore.SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Nieznany slot zapisu.");
        }
    }
}
