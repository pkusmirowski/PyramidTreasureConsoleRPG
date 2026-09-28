namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>
/// Cały kontakt gry z terminalem. Ekrany rozmawiają tylko z tym interfejsem, dzięki czemu
/// da się podmienić implementację (goła konsola, Spectre.Console, dubler w testach).
/// </summary>
public interface IGameIO
{
    /// <summary>Czas pauzy po każdej linii narracji (ms). 0 = brak pauz.</summary>
    int NarrationDelayMs { get; set; }

    void Clear();

    void WriteLine(string text = "");

    void WriteLine(string text, ConsoleColor color);

    void ShowInfo(string text);

    void ShowSuccess(string text);

    void ShowError(string text);

    void Header(string text);

    /// <summary>Wyświetla tytuł i ponumerowane opcje, po czym czyta wybór 1..N.</summary>
    int Menu(string title, params string[] options);

    /// <summary>Czyta liczbę z zakresu [min, max]; powtarza pytanie do skutku.</summary>
    int ReadNumber(string prompt, int min, int max);

    /// <summary>Czyta niepustą linię tekstu (np. imię bohatera).</summary>
    string ReadText(string prompt, int maxLength = 20);

    /// <summary>Narracja z pauzami: klawisz pomija pauzę, Esc resztę tekstu.</summary>
    void Narrate(IReadOnlyList<string> lines, ConsoleColor color = ConsoleColor.Yellow);

    /// <summary>Krótka, pomijalna pauza.</summary>
    void Pause(int milliseconds = 700);

    void PressAnyKey(string text = "Naciśnij dowolny klawisz, aby kontynuować...");

    /// <summary>Nagłówek walki: bohater i wszyscy żywi wrogowie ze statusami.</summary>
    void ShowCombatStatus(Hero hero, IReadOnlyList<Enemy> enemies);

    void ShowStats(Hero hero);
}
