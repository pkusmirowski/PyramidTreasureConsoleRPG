using System.Text;

namespace PyramidTreasureConsoleRPG;

/// <summary>
/// Wszystkie operacje na konsoli w jednym miejscu: kolory, bezpieczne czytanie liczb,
/// pomijalne pauzy narracji. Dzięki temu logika gry nie zależy od Console i da się ją testować.
/// </summary>
public static class GameIO
{
    /// <summary>Czas pauzy po każdej linii narracji (ms). 0 = brak pauz.</summary>
    public static int NarrationDelayMs { get; set; } = 1500;

    private static bool interactive = true;

    /// <summary>Ustawia kodowanie UTF-8, żeby polskie znaki i symbole działały w każdym terminalu.</summary>
    public static void Initialize()
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch (IOException)
        {
            // Konsola przekierowana (np. testy) – kodowanie nie ma znaczenia.
        }

        interactive = !Console.IsInputRedirected && !Console.IsOutputRedirected;
    }

    public static void Clear()
    {
        if (!interactive)
        {
            return;
        }

        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Brak prawdziwego terminala – ignorujemy.
        }
    }

    public static void WriteLine(string text = "") => Console.WriteLine(text);

    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    public static void Info(string text) => WriteLine(text, ConsoleColor.Yellow);

    public static void Success(string text) => WriteLine(text, ConsoleColor.Green);

    public static void Error(string text) => WriteLine(text, ConsoleColor.Red);

    public static void Header(string text)
    {
        WriteLine();
        WriteLine($"=== {text} ===", ConsoleColor.Cyan);
    }

    /// <summary>Wyświetla listę opcji ponumerowanych od 1.</summary>
    public static void Menu(string title, params string[] options)
    {
        WriteLine();
        WriteLine(title, ConsoleColor.Cyan);
        for (int i = 0; i < options.Length; i++)
        {
            WriteLine($"{i + 1}. {options[i]}");
        }
    }

    /// <summary>
    /// Czyta liczbę całkowitą. Zwraca null przy pustym wejściu, złym formacie lub przepełnieniu.
    /// Nigdy nie rzuca wyjątku (dawniej OverflowException zamykał grę).
    /// </summary>
    public static int? ReadInt()
    {
        string? line = Console.ReadLine();
        if (line is null)
        {
            // Koniec strumienia wejścia (np. Ctrl+Z / zamknięty terminal) – kończymy grę zamiast pętlić się w nieskończoność.
            throw new EndOfStreamException("Wejście konsoli zostało zamknięte.");
        }

        return int.TryParse(line.Trim(), out int value) ? value : null;
    }

    /// <summary>Czyta wybór z zakresu [min, max]; powtarza pytanie do skutku.</summary>
    public static int ReadChoice(int min, int max, string prompt = "Wybór: ")
    {
        while (true)
        {
            Console.Write(prompt);
            int? value = ReadInt();
            if (value.HasValue && value.Value >= min && value.Value <= max)
            {
                return value.Value;
            }

            Error($"Nieprawidłowy wybór. Podaj liczbę od {min} do {max}.");
        }
    }

    /// <summary>Czyta wybór z menu o podanej liczbie opcji (1..count).</summary>
    public static int ReadMenuChoice(int count) => ReadChoice(1, count);

    /// <summary>Czyta niepustą linię tekstu (np. imię bohatera).</summary>
    public static string ReadText(string prompt, int maxLength = 20)
    {
        while (true)
        {
            Console.Write(prompt);
            string? line = Console.ReadLine();
            if (line is null)
            {
                throw new EndOfStreamException("Wejście konsoli zostało zamknięte.");
            }

            line = line.Trim();
            if (line.Length == 0)
            {
                Error("Wpisz przynajmniej jeden znak.");
                continue;
            }

            return line.Length > maxLength ? line[..maxLength] : line;
        }
    }

    /// <summary>
    /// Narracja: każda linia jest wyświetlana z pauzą. Dowolny klawisz pomija pauzę,
    /// Esc pomija całą resztę tekstu.
    /// </summary>
    public static void Narrate(ConsoleColor color, params string[] lines)
    {
        bool skipAll = false;
        Console.ForegroundColor = color;
        foreach (string line in lines)
        {
            Console.WriteLine(line);
            if (!skipAll && WaitOrSkip(NarrationDelayMs) == SkipResult.SkipAll)
            {
                skipAll = true;
            }
        }

        Console.ResetColor();
    }

    public static void Narrate(params string[] lines) => Narrate(ConsoleColor.Yellow, lines);

    /// <summary>Krótka pauza (np. po komunikacie w walce), również pomijalna.</summary>
    public static void Pause(int milliseconds = 700) => WaitOrSkip(milliseconds);

    public static void PressAnyKey(string text = "Naciśnij dowolny klawisz, aby kontynuować...")
    {
        if (!interactive)
        {
            return;
        }

        WriteLine(text, ConsoleColor.DarkGray);
        try
        {
            Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            // Wejście przekierowane.
        }
    }

    private enum SkipResult
    {
        None,
        SkipLine,
        SkipAll,
    }

    private static SkipResult WaitOrSkip(int milliseconds)
    {
        if (milliseconds <= 0)
        {
            return SkipResult.None;
        }

        if (!interactive)
        {
            return SkipResult.None;
        }

        const int step = 50;
        int waited = 0;
        try
        {
            while (waited < milliseconds)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    return key.Key == ConsoleKey.Escape ? SkipResult.SkipAll : SkipResult.SkipLine;
                }

                Thread.Sleep(step);
                waited += step;
            }
        }
        catch (InvalidOperationException)
        {
            Thread.Sleep(Math.Max(0, milliseconds - waited));
        }

        return SkipResult.None;
    }
}
