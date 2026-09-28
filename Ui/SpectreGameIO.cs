using Spectre.Console;

namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>
/// IGameIO na Spectre.Console: menu strzałkami, tabele, paski HP. Wymaga prawdziwego terminala;
/// przy przekierowanym wejściu lub wyjściu (bot, CI, potok) Program wybiera ConsoleGameIO,
/// bo AnsiConsole nie pisze do potoku.
/// </summary>
public sealed class SpectreGameIO : IGameIO
{
    public SpectreGameIO()
    {
        ConsolePrompts.EnsureUtf8();
    }

    /// <summary>Czy ta implementacja może działać w bieżącym terminalu.</summary>
    public static bool IsSupported => ConsolePrompts.IsInteractive && AnsiConsole.Profile.Capabilities.Interactive;

    public int NarrationDelayMs { get; set; } = 1500;

    public void Clear() => AnsiConsole.Clear();

    public void WriteLine(string text = "") => AnsiConsole.WriteLine(text);

    public void WriteLine(string text, ConsoleColor color) => AnsiConsole.MarkupLine($"[{ToMarkup(color)}]{Markup.Escape(text)}[/]");

    public void ShowInfo(string text) => WriteLine(text, ConsoleColor.Yellow);

    public void ShowSuccess(string text) => WriteLine(text, ConsoleColor.Green);

    public void ShowError(string text) => WriteLine(text, ConsoleColor.Red);

    public void Header(string text)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[aqua]{Markup.Escape(text)}[/]").LeftJustified());
    }

    public int Menu(string title, params string[] options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var prompt = new SelectionPrompt<int>()
            .Title($"[aqua]{Markup.Escape(title)}[/]")
            .PageSize(12)
            .MoreChoicesText("[grey](strzałki: więcej opcji)[/]")
            .HighlightStyle(new Style(Color.Black, Color.Yellow))
            .UseConverter(i => $"{i + 1}. {Markup.Escape(options[i])}")
            .AddChoices(Enumerable.Range(0, options.Length));
        return AnsiConsole.Prompt(prompt) + 1;
    }

    public int ReadNumber(string prompt, int min, int max)
    {
        var textPrompt = new TextPrompt<int>(Markup.Escape(prompt.TrimEnd()))
            .PromptStyle("yellow")
            .ValidationErrorMessage($"[red]Podaj liczbę od {min} do {max}.[/]")
            .Validate(v => v >= min && v <= max);
        return AnsiConsole.Prompt(textPrompt);
    }

    public string ReadText(string prompt, int maxLength = 20)
    {
        var textPrompt = new TextPrompt<string>(Markup.Escape(prompt.TrimEnd()))
            .PromptStyle("yellow")
            .ValidationErrorMessage("[red]Wpisz przynajmniej jeden znak.[/]")
            .Validate(v => !string.IsNullOrWhiteSpace(v));
        string value = AnsiConsole.Prompt(textPrompt).Trim();
        return value.Length > maxLength ? value[..maxLength] : value;
    }

    public void Narrate(IReadOnlyList<string> lines, ConsoleColor color = ConsoleColor.Yellow)
    {
        ArgumentNullException.ThrowIfNull(lines);
        bool skipAll = false;
        foreach (string line in lines)
        {
            WriteLine(line, color);
            if (!skipAll && ConsolePrompts.WaitOrSkip(NarrationDelayMs) == ConsolePrompts.SkipResult.SkipAll)
            {
                skipAll = true;
            }
        }
    }

    public void Pause(int milliseconds = 700) => ConsolePrompts.WaitOrSkip(milliseconds);

    public void PressAnyKey(string text = "Naciśnij dowolny klawisz, aby kontynuować...")
    {
        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(text)}[/]");
        ConsolePrompts.WaitForAnyKey();
    }

    public void ShowCombatStatus(Hero hero, IReadOnlyList<Enemy> enemies)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(enemies);
        AnsiConsole.WriteLine();
        var table = new Table().Border(TableBorder.Rounded).HideHeaders().AddColumn(string.Empty).AddColumn(string.Empty).AddColumn(string.Empty).AddColumn(string.Empty);
        table.AddRow($"[green]{Markup.Escape(hero.Name)}[/]", HpBar(hero.Hp, hero.MaxHp, "green"), $"[green]{hero.Hp}/{hero.MaxHp} HP[/]", $"[fuchsia]{Markup.Escape(hero.Statuses.Describe())}[/]");
        foreach (Enemy enemy in enemies)
        {
            table.AddRow($"[red]{Markup.Escape(enemy.Name)}[/]", HpBar(enemy.Hp, enemy.MaxHp, "red"), $"[red]{enemy.Hp}/{enemy.MaxHp} HP[/]", $"[fuchsia]{Markup.Escape(enemy.Statuses.Describe())}[/]");
        }

        AnsiConsole.Write(table);
    }

    public void ShowStats(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        var table = new Table().Border(TableBorder.Rounded).Title("[aqua]Statystyki bohatera[/]");
        table.AddColumn("Cecha").AddColumn("Wartość");
        foreach ((string label, string value) in HeroStats.Rows(hero))
        {
            table.AddRow(Markup.Escape(label), Markup.Escape(value));
        }

        AnsiConsole.Write(table);
    }

    private static string HpBar(int current, int max, string color)
    {
        const int width = 20;
        int filled = max <= 0 ? 0 : Math.Clamp((int)Math.Round(width * (double)current / max), 0, width);
        return $"[{color}]{new string('█', filled)}[/][grey]{new string('░', width - filled)}[/]";
    }

    private static string ToMarkup(ConsoleColor color) => color switch
    {
        ConsoleColor.Red => "red",
        ConsoleColor.DarkRed => "maroon",
        ConsoleColor.Green => "green",
        ConsoleColor.DarkGreen => "darkgreen",
        ConsoleColor.Yellow => "yellow",
        ConsoleColor.DarkYellow => "olive",
        ConsoleColor.Blue => "blue",
        ConsoleColor.DarkBlue => "navy",
        ConsoleColor.Cyan => "aqua",
        ConsoleColor.DarkCyan => "teal",
        ConsoleColor.Magenta => "fuchsia",
        ConsoleColor.DarkMagenta => "purple",
        ConsoleColor.Gray => "silver",
        ConsoleColor.DarkGray => "grey",
        ConsoleColor.White => "white",
        _ => "default",
    };
}
