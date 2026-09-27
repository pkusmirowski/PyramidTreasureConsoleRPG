namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Wiersze ekranu statystyk – wspólne dla wszystkich implementacji IGameIO.</summary>
internal static class HeroStats
{
    public static IReadOnlyList<(string Label, string Value)> Rows(Hero hero) => new[]
    {
        ("Imię", hero.Name),
        ("Klasa", hero.ClassName),
        ("Poziom", hero.IsMaxLevel ? $"{hero.Level} (maksymalny)" : $"{hero.Level}   doświadczenie {hero.Exp}/{hero.ExpToNextLevel}"),
        ("Punkty zdrowia", $"{hero.Hp}/{hero.MaxHp}"),
        ("Żywotność / Siła / Zręczność", $"{hero.Vit} / {hero.Str} / {hero.Dex}"),
        ("Obrażenia", $"{hero.MinDmg}-{hero.MaxDmg}"),
        ("Szansa trafienia / krytyk", $"{hero.HitChance:0}% / {hero.CritChance:0}%"),
        ("Pancerz", $"{hero.Armor} (redukcja {100 - (100 * 100 / (100 + hero.Armor))}%)"),
        ("Uniki / ucieczka", $"{hero.Evasion}% / {hero.FleeChance}%"),
        ("Złoto", hero.Gold.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        ("Mikstury", hero.Inventory.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        ("Etap wyprawy", Story.StageName(hero.Stage)),
    };
}
