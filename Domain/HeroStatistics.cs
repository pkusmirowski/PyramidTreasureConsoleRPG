namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Licznik wyprawy: walki, zabici, złoto, kasyno, wybory. Pokazywany w podsumowaniu i zapisywany.</summary>
public sealed class HeroStatistics
{
    public int Fights { get; set; }

    public int Kills { get; set; }

    public int BossKills { get; set; }

    public int GoldEarned { get; set; }

    public int CasinoWon { get; set; }

    public int CasinoLost { get; set; }

    /// <summary>Wybory kosztem innych: egzekucje, tortury, kradzieże, przysługi dla Podziemia.</summary>
    public int DarkChoices { get; set; }

    /// <summary>Wybory na korzyść Miasta i Bractwa: łaska, pomoc, uczciwość.</summary>
    public int LightChoices { get; set; }

    public int PotionsDrunk { get; set; }

    public int EventsResolved { get; set; }

    /// <summary>Zapisuje wybór moralny: ujemna waga to wybór mroczny, dodatnia jasny, zero nie liczy się.</summary>
    public void RecordChoice(int moralWeight)
    {
        if (moralWeight < 0)
        {
            DarkChoices++;
        }
        else if (moralWeight > 0)
        {
            LightChoices++;
        }
    }

    /// <summary>Waga moralna efektu reputacyjnego: Miasto i Bractwo to jasna strona, Podziemie mroczna.</summary>
    public static int MoralWeight(Faction faction, int delta) => faction switch
    {
        Faction.Underworld => -delta,
        _ => delta,
    };

    public StatisticsSaveData ToSaveData() => new()
    {
        Fights = Fights,
        Kills = Kills,
        BossKills = BossKills,
        GoldEarned = GoldEarned,
        CasinoWon = CasinoWon,
        CasinoLost = CasinoLost,
        DarkChoices = DarkChoices,
        LightChoices = LightChoices,
        PotionsDrunk = PotionsDrunk,
        EventsResolved = EventsResolved,
    };

    public static HeroStatistics FromSaveData(StatisticsSaveData? data) => data is null
        ? new HeroStatistics()
        : new HeroStatistics
        {
            Fights = Math.Max(0, data.Fights),
            Kills = Math.Max(0, data.Kills),
            BossKills = Math.Max(0, data.BossKills),
            GoldEarned = Math.Max(0, data.GoldEarned),
            CasinoWon = Math.Max(0, data.CasinoWon),
            CasinoLost = Math.Max(0, data.CasinoLost),
            DarkChoices = Math.Max(0, data.DarkChoices),
            LightChoices = Math.Max(0, data.LightChoices),
            PotionsDrunk = Math.Max(0, data.PotionsDrunk),
            EventsResolved = Math.Max(0, data.EventsResolved),
        };
}

/// <summary>Płaski zapis statystyk (JSON).</summary>
public sealed class StatisticsSaveData
{
    public int Fights { get; init; }

    public int Kills { get; init; }

    public int BossKills { get; init; }

    public int GoldEarned { get; init; }

    public int CasinoWon { get; init; }

    public int CasinoLost { get; init; }

    public int DarkChoices { get; init; }

    public int LightChoices { get; init; }

    public int PotionsDrunk { get; init; }

    public int EventsResolved { get; init; }
}
