namespace PyramidTreasureConsoleRPG.Domain;

public enum TalentId
{
    WarriorRage,
    WarriorWall,
    WarriorVeteran,
    WarriorExecutioner,
    ArcherEagleEye,
    ArcherShadow,
    ArcherMarksman,
    ArcherHunter,
    AssassinVenom,
    AssassinThiefTrade,
    AssassinGhost,
    AssassinNightBlade,
}

public enum TalentEffect
{
    /// <summary>Procent więcej obrażeń, gdy HP poniżej 1/3.</summary>
    DamageWhenLow,
    ArmorFlat,
    CritFlat,
    EvasionFlat,
    HitFlat,
    MaxHpPercent,
    GoldPercent,
    FleeFlat,
    /// <summary>Procent więcej obrażeń zawsze.</summary>
    DamagePercent,
}

public sealed record TalentDefinition(TalentId Id, HeroClass Class, int Level, string Name, string Description, TalentEffect Effect, int Power);

/// <summary>Talenty: na poziomach 5, 10, 15 i 20 wybór jednego z dwóch na klasę.</summary>
public static class TalentCatalog
{
    private static readonly TalentDefinition[] Talents =
    [
        new(TalentId.WarriorRage, HeroClass.Warrior, 5, "Szał", "+25% obrażeń, gdy masz mniej niż 1/3 zdrowia.", TalentEffect.DamageWhenLow, 25),
        new(TalentId.WarriorWall, HeroClass.Warrior, 5, "Mur", "+12 pancerza.", TalentEffect.ArmorFlat, 12),
        new(TalentId.WarriorVeteran, HeroClass.Warrior, 10, "Weteran", "+15% maksymalnego zdrowia.", TalentEffect.MaxHpPercent, 15),
        new(TalentId.WarriorExecutioner, HeroClass.Warrior, 10, "Kat", "+10% obrażeń.", TalentEffect.DamagePercent, 10),
        new(TalentId.ArcherEagleEye, HeroClass.Archer, 5, "Sokole oko", "+10 do szansy trafienia krytycznego.", TalentEffect.CritFlat, 10),
        new(TalentId.ArcherShadow, HeroClass.Archer, 5, "Cień", "+8 do uników.", TalentEffect.EvasionFlat, 8),
        new(TalentId.ArcherMarksman, HeroClass.Archer, 10, "Strzelec wyborowy", "+8 do szansy trafienia.", TalentEffect.HitFlat, 8),
        new(TalentId.ArcherHunter, HeroClass.Archer, 10, "Łowca", "+15 do szansy ucieczki.", TalentEffect.FleeFlat, 15),
        new(TalentId.AssassinVenom, HeroClass.Assassin, 5, "Jad", "+12% obrażeń.", TalentEffect.DamagePercent, 12),
        new(TalentId.AssassinThiefTrade, HeroClass.Assassin, 5, "Złodziejski fach", "+25% złota z walk.", TalentEffect.GoldPercent, 25),
        new(TalentId.AssassinGhost, HeroClass.Assassin, 10, "Duch", "+8 do uników.", TalentEffect.EvasionFlat, 8),
        new(TalentId.AssassinNightBlade, HeroClass.Assassin, 10, "Nocne ostrze", "+8 do szansy trafienia krytycznego.", TalentEffect.CritFlat, 8),
    ];

    /// <summary>Poziomy, na których wybiera się talent (wynikają z katalogu).</summary>
    public static IReadOnlyList<int> Levels { get; } = Talents.Select(t => t.Level).Distinct().Order().ToList();

    public static IReadOnlyList<TalentDefinition> All => Talents;

    public static TalentDefinition Get(TalentId id) => Talents.First(t => t.Id == id);

    public static IReadOnlyList<TalentDefinition> ForClassAtLevel(HeroClass heroClass, int level) => Talents.Where(t => t.Class == heroClass && t.Level == level).ToList();
}
