using System.Text;

namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>
/// Wspólne, niezależne od biblioteki UI operacje na System.Console: wykrywanie terminala,
/// odczyt liczb i tekstu z potoku, pomijalne pauzy. Używane przez obie implementacje IGameIO.
/// </summary>
internal static class ConsolePrompts
{
    public enum SkipResult
    {
        None,
        SkipLine,
        SkipAll,
    }

    /// <summary>True, gdy gra rozmawia z prawdziwym terminalem (a nie potokiem, botem czy CI).</summary>
    public static bool IsInteractive { get; } = !Console.IsInputRedirected && !Console.IsOutputRedirected;

    public static void EnsureUtf8()
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch (IOException)
        {
            // Konsola przekierowana – kodowanie nie ma znaczenia.
        }
    }

    public static string ReadLineOrThrow() => Console.ReadLine() ?? throw new EndOfStreamException("Wejście konsoli zostało zamknięte.");

    /// <summary>Czyta liczbę z zakresu przez zwykły ReadLine (tryb nieinteraktywny). Zwraca null przy złym wejściu.</summary>
    public static int? TryReadNumber(string prompt, int min, int max)
    {
        Console.Write(prompt);
        string line = ReadLineOrThrow();
        return int.TryParse(line.Trim(), out int value) && value >= min && value <= max ? value : null;
    }

    public static string ReadText(string prompt, int maxLength, Action<string> onEmpty)
    {
        ArgumentNullException.ThrowIfNull(onEmpty);
        while (true)
        {
            Console.Write(prompt);
            string line = ReadLineOrThrow().Trim();
            if (line.Length == 0)
            {
                onEmpty("Wpisz przynajmniej jeden znak.");
                continue;
            }

            return line.Length > maxLength ? line[..maxLength] : line;
        }
    }

    /// <summary>Czeka podany czas; dowolny klawisz przerywa pauzę, Esc oznacza pominięcie całej narracji.</summary>
    public static SkipResult WaitOrSkip(int milliseconds)
    {
        if (milliseconds <= 0 || !IsInteractive)
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

    public static void WaitForAnyKey()
    {
        if (!IsInteractive)
        {
            return;
        }

        try
        {
            Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            // Wejście przekierowane.
        }
    }

    public static void TryClear()
    {
        if (!IsInteractive)
        {
            return;
        }

        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Brak prawdziwego terminala.
        }
    }
}
