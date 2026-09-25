namespace PyramidTreasureConsoleRPG;

/// <summary>Źródło losowości – podmienialne w testach.</summary>
public interface IRandomSource
{
    /// <summary>Liczba z zakresu [minInclusive, maxExclusive).</summary>
    int Next(int minInclusive, int maxExclusive);
}

public sealed class SystemRandomSource : IRandomSource
{
    public int Next(int minInclusive, int maxExclusive) => Random.Shared.Next(minInclusive, maxExclusive);
}

/// <summary>Deterministyczne źródło do testów.</summary>
public sealed class SeededRandomSource(int seed) : IRandomSource
{
    private readonly Random random = new(seed);

    public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
}

/// <summary>Jedno wspólne źródło losowości dla całej gry (dawniej były cztery różne).</summary>
public static class Rng
{
    public static IRandomSource Source { get; set; } = new SystemRandomSource();

    /// <summary>Liczba z zakresu [min, max] – obie granice włącznie.</summary>
    public static int Range(int min, int max)
    {
        if (max < min)
        {
            (min, max) = (max, min);
        }

        return Source.Next(min, max + 1);
    }

    /// <summary>Zwraca true z prawdopodobieństwem percent (0–100).</summary>
    public static bool Chance(double percent)
    {
        if (percent <= 0)
        {
            return false;
        }

        if (percent >= 100)
        {
            return true;
        }

        return Source.Next(0, 100) < percent;
    }

    public static T Pick<T>(IReadOnlyList<T> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("Kolekcja nie może być pusta.", nameof(items));
        }

        return items[Source.Next(0, items.Count)];
    }

    /// <summary>Losowa kolejność (Fisher–Yates).</summary>
    public static List<T> Shuffle<T>(IEnumerable<T> items)
    {
        var list = items.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Source.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }
}
