using System.Text.Json;

namespace PyramidTreasureConsoleRPG;

public sealed record SaveInfo(string Name, HeroClass Class, int Level, DateTime SavedAt);

/// <summary>Zapis i odczyt gry. Folder da się podmienić (testy).</summary>
public static class SaveSystem
{
    public const string FolderName = "GreatPyramidTreasureRPG_DataSave";
    public const string FileName = "DataSave.json";

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string SaveFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FolderName);

    public static string SaveFilePath => Path.Combine(SaveFolder, FileName);

    public static bool SaveExists() => File.Exists(SaveFilePath);

    public static bool Save(Hero hero, out string message)
    {
        ArgumentNullException.ThrowIfNull(hero);
        try
        {
            Directory.CreateDirectory(SaveFolder);
            SaveData data = hero.ToSaveData();
            data.SavedAt = DateTime.Now;
            File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(data, Options));
            message = "Gra została zapisana!";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            message = $"Nie udało się zapisać gry: {ex.Message}";
            return false;
        }
    }

    public static Hero? Load(out string message)
    {
        SaveData? data = ReadData(out message);
        if (data is null)
        {
            return null;
        }

        try
        {
            Hero hero = Hero.FromSaveData(data);
            message = $"Wczytano: {hero.Name}, {hero.ClassName}, poziom {hero.Level}.";
            return hero;
        }
        catch (InvalidDataException ex)
        {
            message = $"Plik zapisu jest uszkodzony: {ex.Message}";
            return null;
        }
    }

    /// <summary>Krótki opis zapisu do menu głównego (bez tworzenia bohatera).</summary>
    public static SaveInfo? Peek()
    {
        SaveData? data = ReadData(out _);
        if (data is null || !Enum.IsDefined(typeof(HeroClass), data.Class))
        {
            return null;
        }

        return new SaveInfo(data.Name, (HeroClass)data.Class, data.Level, data.SavedAt);
    }

    private static SaveData? ReadData(out string message)
    {
        if (!SaveExists())
        {
            message = "Brak zapisanej gry.";
            return null;
        }

        try
        {
            SaveData? data = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(SaveFilePath));
            if (data is null)
            {
                message = "Plik zapisu jest pusty.";
                return null;
            }

            if (data.Version != SaveData.CurrentVersion)
            {
                message = $"Plik zapisu pochodzi z innej wersji gry (wersja {data.Version}, wymagana {SaveData.CurrentVersion}). Zacznij nową grę.";
                return null;
            }

            message = string.Empty;
            return data;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            message = $"Nie udało się odczytać zapisu: {ex.Message}";
            return null;
        }
    }
}
