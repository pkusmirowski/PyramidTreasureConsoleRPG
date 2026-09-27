namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Implementacja IGameIO na gołym System.Console (tryb awaryjny `--plain`). Działa też z przekierowanym wejściem.</summary>
public sealed class ConsoleGameIO : IGameIO
{
    public ConsoleGameIO()
    {
        ConsolePrompts.EnsureUtf8();
    }

    public int NarrationDelayMs { get; set; } = 1500;

    public void Clear() => ConsolePrompts.TryClear();

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
            int? value = ConsolePrompts.TryReadNumber(prompt, min, max);
            if (value.HasValue)
            {
                return value.Value;
            }

            ShowError($"Nieprawidłowy wybór. Podaj liczbę od {min} do {max}.");
        }
    }

    public string ReadText(string prompt, int maxLength = 20) => ConsolePrompts.ReadText(prompt, maxLength, ShowError);

    public void Narrate(IReadOnlyList<string> lines, ConsoleColor color = ConsoleColor.Yellow)
    {
        ArgumentNullException.ThrowIfNull(lines);
        bool skipAll = false;
        Console.ForegroundColor = color;
        foreach (string line in lines)
        {
            Console.WriteLine(line);
            if (!skipAll && ConsolePrompts.WaitOrSkip(NarrationDelayMs) == ConsolePrompts.SkipResult.SkipAll)
            {
                skipAll = true;
            }
        }

        Console.ResetColor();
    }

    public void Pause(int milliseconds = 700) => ConsolePrompts.WaitOrSkip(milliseconds);

    public void PressAnyKey(string text = "Naciśnij dowolny klawisz, aby kontynuować...")
    {
        if (!ConsolePrompts.IsInteractive)
        {
            return;
        }

        WriteLine(text, ConsoleColor.DarkGray);
        ConsolePrompts.WaitForAnyKey();
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
        foreach ((string label, string value) in HeroStats.Rows(hero))
        {
            WriteLine($"{label}: {value}");
        }

        WriteLine("-------------------");
    }
}
