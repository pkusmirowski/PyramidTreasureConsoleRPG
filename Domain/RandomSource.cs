namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Źródło losowości. Wstrzykiwane wszędzie tam, gdzie gra losuje – bez globalnego stanu.</summary>
public interface IRandomSource
{
    /// <summary>Liczba z zakresu [minInclusive, maxExclusive).</summary>
    int NextInt(int minInclusive, int maxExclusive);
}

public sealed class SystemRandomSource : IRandomSource
{
    public int NextInt(int minInclusive, int maxExclusive) => Random.Shared.Next(minInclusive, maxExclusive);
}

/// <summary>Deterministyczne źródło do testów i powtarzalnych rozgrywek.</summary>
public sealed class SeededRandomSource(int seed) : IRandomSource
{
    private readonly Random random = new(seed);

    public int NextInt(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
}

public static class RandomExtensions
{
    /// <summary>Liczba z zakresu [min, max] – obie granice włącznie.</summary>
    public static int Range(this IRandomSource rng, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(rng);
        if (max < min)
        {
            (min, max) = (max, min);
        }

        return rng.NextInt(min, max + 1);
    }

    /// <summary>Zwraca true z prawdopodobieństwem percent (0–100).</summary>
    public static bool Chance(this IRandomSource rng, double percent)
    {
        ArgumentNullException.ThrowIfNull(rng);
        if (percent <= 0)
        {
            return false;
        }

        if (percent >= 100)
        {
            return true;
        }

        return rng.NextInt(0, 100) < percent;
    }

    public static T Pick<T>(this IRandomSource rng, IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(rng);
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("Kolekcja nie może być pusta.", nameof(items));
        }

        return items[rng.NextInt(0, items.Count)];
    }

    /// <summary>Losowa kolejność (Fisher–Yates).</summary>
    public static List<T> Shuffle<T>(this IRandomSource rng, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(rng);
        var list = items.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.NextInt(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }
}
