using System.Text;

namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Implementacja IGameIO na gołym System.Console. Działa też z przekierowanym wejściem (testy, boty).</summary>
public sealed class ConsoleGameIO : IGameIO
{
    private readonly bool interactive;

    public ConsoleGameIO()
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

        interactive = !Console.IsInputRedirected && !Console.IsOutputRedirected;
    }

    public int NarrationDelayMs { get; set; } = 1500;

    public void Clear()
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
            // Brak prawdziwego terminala.
        }
    }

    public void WriteLine(string text = "") => Console.WriteLine(text);

    public void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public void ShowInfo(string text) => WriteLine(text, ConsoleColor.Yellow);

    public void ShowSuccess(string text) => WriteLine(text, ConsoleColor.Green);

    public void ShowError(string text) => WriteLine(text, ConsoleColor.Red);

    public void Header(string text)
    {
        WriteLine();
        WriteLine($"=== {text} ===", ConsoleColor.Cyan);
    }

    public int Menu(string title, params string[] options)
    {
        ArgumentNullException.ThrowIfNull(options);
        WriteLine();
        WriteLine(title, ConsoleColor.Cyan);
        for (int i = 0; i < options.Length; i++)
        {
            WriteLine($"{i + 1}. {options[i]}");
        }

        return ReadNumber("Wybór: ", 1, options.Length);
    }

    public int ReadNumber(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string? line = Console.ReadLine() ?? throw new EndOfStreamException("Wejście konsoli zostało zamknięte.");
            if (int.TryParse(line.Trim(), out int value) && value >= min && value <= max)
            {
                return value;
            }

            ShowError($"Nieprawidłowy wybór. Podaj liczbę od {min} do {max}.");
        }
    }

    public string ReadText(string prompt, int maxLength = 20)
    {
        while (true)
        {
            Console.Write(prompt);
            string line = (Console.ReadLine() ?? throw new EndOfStreamException("Wejście konsoli zostało zamknięte.")).Trim();
            if (line.Length == 0)
            {
                ShowError("Wpisz przynajmniej jeden znak.");
                continue;
            }

            return line.Length > maxLength ? line[..maxLength] : line;
        }
    }

    public void Narrate(IReadOnlyList<string> lines, ConsoleColor color = ConsoleColor.Yellow)
    {
        ArgumentNullException.ThrowIfNull(lines);
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

    public void Pause(int milliseconds = 700) => WaitOrSkip(milliseconds);

    public void PressAnyKey(string text = "Naciśnij dowolny klawisz, aby kontynuować...")
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

    public void ShowCombatStatus(Hero hero, Enemy enemy)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(enemy);
        WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"{hero.Name}: {hero.Hp}/{hero.MaxHp} HP");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("   vs   ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{enemy.Name}: {enemy.Hp}/{enemy.MaxHp} HP");
        Console.ResetColor();
    }

    public void ShowStats(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        Header("Statystyki bohatera");
        WriteLine($"Imię: {hero.Name}   Klasa: {hero.ClassName}");
        WriteLine($"Poziom: {hero.Level}" + (hero.IsMaxLevel ? " (maksymalny)" : $"   Doświadczenie: {hero.Exp}/{hero.ExpToNextLevel}"));
        WriteLine($"Punkty zdrowia: {hero.Hp}/{hero.MaxHp}");
        WriteLine($"Żywotność: {hero.Vit}   Siła: {hero.Str}   Zręczność: {hero.Dex}");
        WriteLine($"Obrażenia: {hero.MinDmg}-{hero.MaxDmg}   Szansa trafienia: {hero.HitChance:0}%   Krytyk: {hero.CritChance:0}%");
        WriteLine($"Pancerz: {hero.Armor} (redukcja {100 - (100 * 100 / (100 + hero.Armor))}%)   Uniki: {hero.Evasion}%   Ucieczka: {hero.FleeChance}%");
        WriteLine($"Złoto: {hero.Gold}   Mikstury: {hero.Inventory.Count}");
        WriteLine($"Etap wyprawy: {Story.StageName(hero.Stage)}");
        WriteLine("-------------------");
    }

    private enum SkipResult
    {
        None,
        SkipLine,
        SkipAll,
    }

    private SkipResult WaitOrSkip(int milliseconds)
    {
        if (milliseconds <= 0 || !interactive)
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
