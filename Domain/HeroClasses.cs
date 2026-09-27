namespace PyramidTreasureConsoleRPG.Domain;

public enum HeroClass
{
    Warrior = 1,
    Archer = 2,
    Assassin = 3,
}

public enum SpecialAttackKind
{
    /// <summary>Trzy cięcia po 60 % obrażeń, każde z trafieniem −15.</summary>
    TripleCut,

    /// <summary>Dwa strzały po 80 % obrażeń.</summary>
    DoubleShot,

    /// <summary>Jedno uderzenie 120 % obrażeń, trafienie −10, krytyk ×3.</summary>
    PoisonedBlade,
}

/// <summary>
/// Definicja klasy postaci: same liczby i nazwy. Wzory, które je zużywają, są w <see cref="Hero"/>.
/// Dzięki temu dodanie klasy to nowy wpis w <see cref="HeroClasses"/>, nie nowa klasa C#.
/// </summary>
public sealed record HeroClassDefinition(
    HeroClass Kind,
    string Name,
    string Description,
    int StartVit,
    int StartStr,
    int StartDex,
    int HpPerVit,
    int BaseMinDmg,
    int BaseMaxDmg,
    int StrWeight,
    int DexWeight,
    double BaseHitChance,
    double BaseCritChance,
    int VitPerLevel,
    int StrPerLevel,
    int DexPerLevel,
    int ArmorDexDivisor,
    bool ArmorAddsLevel,
    int EvasionDexDivisor,
    int HitChanceDexDivisor,
    string NormalAttackName,
    string StrongAttackName,
    string SpecialAttackName,
    SpecialAttackKind SpecialAttack);

public static class HeroClasses
{
    public static HeroClassDefinition Warrior { get; } = new(
        HeroClass.Warrior,
        "Wojownik",
        "dużo zdrowia, ciężkie ciosy, pancerz rośnie z poziomem. Specjalność: trzystronne cięcie.",
        StartVit: 15, StartStr: 4, StartDex: 2,
        HpPerVit: 14, BaseMinDmg: 2, BaseMaxDmg: 7, StrWeight: 2, DexWeight: 0,
        BaseHitChance: 60, BaseCritChance: 20,
        VitPerLevel: 2, StrPerLevel: 3, DexPerLevel: 1,
        ArmorDexDivisor: 1, ArmorAddsLevel: true, EvasionDexDivisor: 4, HitChanceDexDivisor: 0,
        "Normalny atak", "Silny atak", "Trzystronne cięcie", SpecialAttackKind.TripleCut);

    public static HeroClassDefinition Archer { get; } = new(
        HeroClass.Archer,
        "Łucznik",
        "obrażenia ze zręczności, najlepsze uniki, słabszy pancerz. Specjalność: podwójny strzał.",
        StartVit: 11, StartStr: 1, StartDex: 3,
        HpPerVit: 12, BaseMinDmg: 2, BaseMaxDmg: 5, StrWeight: 0, DexWeight: 2,
        BaseHitChance: 55, BaseCritChance: 25,
        VitPerLevel: 2, StrPerLevel: 1, DexPerLevel: 3,
        ArmorDexDivisor: 3, ArmorAddsLevel: false, EvasionDexDivisor: 2, HitChanceDexDivisor: 0,
        "Strzał z łuku", "Skupiony strzał", "Podwójny strzał", SpecialAttackKind.DoubleShot);

    public static HeroClassDefinition Assassin { get; } = new(
        HeroClass.Assassin,
        "Asasyn",
        "najcelniejszy, najczęstsze krytyki. Specjalność: zatrute ostrze.",
        StartVit: 12, StartStr: 2, StartDex: 2,
        HpPerVit: 13, BaseMinDmg: 3, BaseMaxDmg: 6, StrWeight: 1, DexWeight: 1,
        BaseHitChance: 60, BaseCritChance: 30,
        VitPerLevel: 2, StrPerLevel: 2, DexPerLevel: 3,
        ArmorDexDivisor: 2, ArmorAddsLevel: false, EvasionDexDivisor: 3, HitChanceDexDivisor: 4,
        "Atak sztyletem", "Atak z ukrycia", "Zatrute ostrze", SpecialAttackKind.PoisonedBlade);

    public static IReadOnlyList<HeroClassDefinition> All { get; } = [Warrior, Archer, Assassin];

    public static HeroClassDefinition Get(HeroClass kind) => kind switch
    {
        HeroClass.Warrior => Warrior,
        HeroClass.Archer => Archer,
        HeroClass.Assassin => Assassin,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
