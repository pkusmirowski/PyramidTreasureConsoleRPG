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
        ("Broń / pancerz / amulet", $"{hero.Weapon?.Name ?? "brak"} / {hero.EquippedArmor?.Name ?? "brak"} / {hero.Trinket?.Name ?? "brak"}"),
        ("Talenty", hero.Talents.Count == 0 ? "brak" : string.Join(", ", hero.Talents.Select(t => TalentCatalog.Get(t).Name))),
        ("Złoto", hero.Gold.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        ("Mikstury", hero.Inventory.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        ("Etap wyprawy", Story.StageName(hero.Stage)),
        ("Dzień / miejsce", $"{hero.Day} / {RegionCatalog.Get(hero.CurrentRegion).Name}"),
        ("Reputacja", string.Join(", ", Enum.GetValues<Faction>().Select(f => $"{RegionCatalog.FactionName(f)} {hero.GetReputation(f):+0;-0;0} ({Story.ReputationName(hero.GetReputation(f))})"))),
        ("Nałóg / dług", $"lotos {hero.Addiction} dawek{(hero.Craving ? " (głód!)" : "")} / {(hero.Debt > 0 ? $"{hero.Debt} g" : "brak")}"),
        ("Zadania", $"{hero.Quests.Count(q => q.Value.Status == QuestStatus.Active)} aktywne, {hero.Quests.Count(q => q.Value.Status == QuestStatus.Completed)} ukończone"),
        ("Trudność", $"{hero.DifficultyDefinition.Name} (wrogowie {hero.EnemyScalePercent}%, nagrody {hero.RewardPercent}%)"),
        ("Wyprawa", $"{(hero.NewGamePlus > 0 ? $"nowa gra+ (cykl {hero.NewGamePlus}, wrogowie +{hero.EnemyScalePercent - 100}%), " : "")}{hero.Stats.Fights} walk, {hero.Stats.Kills} zabitych, {hero.Stats.DarkChoices} mrocznych i {hero.Stats.LightChoices} jasnych wyborów"),
    };

    /// <summary>Wiersze podsumowania po zakończeniu gry.</summary>
    public static IReadOnlyList<(string Label, string Value)> SummaryRows(Hero hero)
    {
        HeroStatistics s = hero.Stats;
        string moral = s.DarkChoices > s.LightChoices * 2 ? "bez skrupułów"
            : s.LightChoices > s.DarkChoices * 2 ? "z czystymi rękami"
            : "pragmatycznie";
        return new[]
        {
            ("Bohater", $"{hero.Name}, {hero.ClassName}, poziom {hero.Level}, trudność {hero.DifficultyDefinition.Name.ToLowerInvariant()}{(hero.NewGamePlus > 0 ? $", nowa gra+ {hero.NewGamePlus}" : "")}"),
            ("Dni w drodze", hero.Day.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Walki / zabici / bossowie", $"{s.Fights} / {s.Kills} / {s.BossKills}"),
            ("Złoto z łupów / na koniec", $"{s.GoldEarned} / {hero.Gold}"),
            ("Kasyno: wygrane / przegrane", $"{s.CasinoWon} / {s.CasinoLost}"),
            ("Zdarzenia / mikstury", $"{s.EventsResolved} / {s.PotionsDrunk}"),
            ("Zadania ukończone", hero.Quests.Count(q => q.Value.Status == QuestStatus.Completed).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Wybory mroczne / jasne", $"{s.DarkChoices} / {s.LightChoices} – {moral}"),
            ("Reputacja", string.Join(", ", Enum.GetValues<Faction>().Select(f => $"{RegionCatalog.FactionName(f)} {hero.GetReputation(f):+0;-0;0}"))),
            ("Nałóg / dług", $"lotos {hero.Addiction} dawek / {(hero.Debt > 0 ? $"{hero.Debt} g" : "brak")}"),
        };
    }
}
