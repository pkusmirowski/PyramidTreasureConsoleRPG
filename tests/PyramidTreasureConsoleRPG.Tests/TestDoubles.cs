namespace PyramidTreasureConsoleRPG.Tests;

/// <summary>Losowość ze skryptu: zwraca kolejne zadane wartości (przycięte do zakresu), potem seed.</summary>
public sealed class ScriptedRandomSource : IRandomSource
{
    private readonly Queue<int> values;
    private readonly Random fallback = new(1);

    public ScriptedRandomSource(params int[] values)
    {
        this.values = new Queue<int>(values);
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (values.Count == 0)
        {
            return fallback.Next(minInclusive, maxExclusive);
        }

        return Math.Clamp(values.Dequeue(), minInclusive, maxExclusive - 1);
    }
}

/// <summary>Dubler konsoli: odpowiedzi z kolejki, wyjście do bufora.</summary>
public sealed class ScriptedGameIO : IGameIO
{
    private readonly Queue<string> answers;

    public ScriptedGameIO(params string[] answers)
    {
        this.answers = new Queue<string>(answers);
    }

    public List<string> Output { get; } = new();

    public List<string> MenusShown { get; } = new();

    public int NarrationDelayMs { get; set; }

    public string AllOutput => string.Join("\n", Output);

    public void Clear()
    {
    }

    public void WriteLine(string text = "") => Output.Add(text);

    public void WriteLine(string text, ConsoleColor color) => Output.Add(text);

    public void ShowInfo(string text) => Output.Add(text);

    public void ShowSuccess(string text) => Output.Add(text);

    public void ShowError(string text) => Output.Add(text);

    public void Header(string text) => Output.Add(text);

    public int Menu(string title, params string[] options)
    {
        MenusShown.Add(title);
        Output.Add(title);
        Output.AddRange(options);
        return ReadNumber(title, 1, options.Length);
    }

    public int ReadNumber(string prompt, int min, int max)
    {
        if (answers.Count == 0)
        {
            throw new InvalidOperationException($"Brak zaplanowanej odpowiedzi na: {prompt}");
        }

        int value = int.Parse(answers.Dequeue(), System.Globalization.CultureInfo.InvariantCulture);
        if (value < min || value > max)
        {
            throw new InvalidOperationException($"Odpowiedź {value} poza zakresem {min}-{max} dla: {prompt}");
        }

        return value;
    }

    public string ReadText(string prompt, int maxLength = 20) => answers.Dequeue();

    public void Narrate(IReadOnlyList<string> lines, ConsoleColor color = ConsoleColor.Yellow) => Output.AddRange(lines);

    public void Pause(int milliseconds = 700)
    {
    }

    public void PressAnyKey(string text = "")
    {
    }

    public void ShowCombatStatus(Hero hero, IReadOnlyList<Enemy> enemies) => Output.Add($"{hero.Hp}/{hero.MaxHp} vs {string.Join(",", enemies.Select(e => e.Hp))}");

    public void ShowStats(Hero hero) => Output.Add($"stats {hero.Name}");
}
