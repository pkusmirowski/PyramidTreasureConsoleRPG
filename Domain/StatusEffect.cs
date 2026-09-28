namespace PyramidTreasureConsoleRPG.Domain;

public enum StatusKind
{
    /// <summary>Obrażenia co turę, ignorują pancerz.</summary>
    Bleed,

    /// <summary>Obrażenia co turę i −15 do trafienia.</summary>
    Poison,

    /// <summary>Utrata tury.</summary>
    Stun,

    /// <summary>Połowa otrzymywanych obrażeń; u bohatera następny cios jest krytyczny.</summary>
    Guard,

    /// <summary>−20 do trafienia.</summary>
    Fear,

    /// <summary>−15% zadawanych obrażeń (choroba, kac).</summary>
    Weak,
}

/// <summary>Status z licznikiem tur. Power = obrażenia na turę dla krwawienia i trucizny.</summary>
public sealed class StatusEffect
{
    public StatusEffect(StatusKind kind, int turns, int power = 0)
    {
        Kind = kind;
        TurnsLeft = turns;
        Power = power;
    }

    public StatusKind Kind { get; }

    public int TurnsLeft { get; set; }

    public int Power { get; set; }

    public static string Name(StatusKind kind) => kind switch
    {
        StatusKind.Bleed => "krwawienie",
        StatusKind.Poison => "trucizna",
        StatusKind.Stun => "ogłuszenie",
        StatusKind.Guard => "obrona",
        StatusKind.Fear => "strach",
        StatusKind.Weak => "osłabienie",
        _ => kind.ToString(),
    };
}

/// <summary>Wynik tyknięcia statusów na początku tury właściciela.</summary>
public sealed record StatusTick(StatusKind Kind, int Damage, bool Expired);

/// <summary>Wspólna lista statusów bohatera i wroga.</summary>
public sealed class StatusList
{
    private readonly List<StatusEffect> effects = [];

    public IReadOnlyList<StatusEffect> All => effects;

    public bool Has(StatusKind kind) => effects.Any(e => e.Kind == kind);

    public StatusEffect? Get(StatusKind kind) => effects.Find(e => e.Kind == kind);

    /// <summary>Nakłada status; istniejący odświeża do dłuższego czasu i większej mocy. Zwraca true, gdy był nowy.</summary>
    public bool Add(StatusKind kind, int turns, int power = 0)
    {
        StatusEffect? existing = Get(kind);
        if (existing is null)
        {
            effects.Add(new StatusEffect(kind, turns, power));
            return true;
        }

        existing.TurnsLeft = Math.Max(existing.TurnsLeft, turns);
        existing.Power = Math.Max(existing.Power, power);
        return false;
    }

    public bool Remove(StatusKind kind) => effects.RemoveAll(e => e.Kind == kind) > 0;

    public void Clear() => effects.Clear();

    /// <summary>Tyknięcie: zwraca obrażenia z krwawienia/trucizny i statusy, które właśnie wygasły.</summary>
    public IReadOnlyList<StatusTick> Tick()
    {
        var ticks = new List<StatusTick>();
        foreach (StatusEffect effect in effects.ToList())
        {
            int damage = effect.Kind is StatusKind.Bleed or StatusKind.Poison ? Math.Max(1, effect.Power) : 0;
            effect.TurnsLeft--;
            bool expired = effect.TurnsLeft <= 0;
            if (expired)
            {
                effects.Remove(effect);
            }

            ticks.Add(new StatusTick(effect.Kind, damage, expired));
        }

        return ticks;
    }

    public int HitPenalty => (Has(StatusKind.Poison) ? 15 : 0) + (Has(StatusKind.Fear) ? 20 : 0);

    public double DamageMultiplier => Has(StatusKind.Weak) ? 0.85 : 1.0;

    public double IncomingMultiplier => Has(StatusKind.Guard) ? 0.5 : 1.0;

    public string Describe() => effects.Count == 0 ? string.Empty : string.Join(", ", effects.Select(e => $"{StatusEffect.Name(e.Kind)} ({e.TurnsLeft})"));
}
