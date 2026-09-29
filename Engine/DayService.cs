namespace PyramidTreasureConsoleRPG.Engine;

public sealed record DayReport(IReadOnlyList<string> Notes);

/// <summary>Co dzień: odsetki od długu, głód lotosu, termin egzekutorów. Wywoływane przy każdym upływie dnia.</summary>
public static class DayService
{
    public const int DebtInterestPercent = 10;
    public const int DebtGraceDays = 5;
    public const int MaxLoan = 500;
    public const int AddictionThreshold = 3;
    public const int CravingAfterDays = 2;
    public const int WithdrawalDays = 7;

    /// <summary>Przesuwa kalendarz o podaną liczbę dni i nakłada skutki. Zwraca komunikaty dla gracza.</summary>
    public static DayReport AdvanceDays(Hero hero, int days)
    {
        ArgumentNullException.ThrowIfNull(hero);
        var notes = new List<string>();
        for (int i = 0; i < days; i++)
        {
            hero.Day++;
            TickDebt(hero, notes);
            TickAddiction(hero, notes);
        }

        return new DayReport(notes);
    }

    private static void TickDebt(Hero hero, List<string> notes)
    {
        if (hero.Debt <= 0)
        {
            return;
        }

        int interest = Math.Max(1, hero.Debt * DebtInterestPercent / 100);
        hero.Debt += interest;
        int overdueDays = hero.Day - hero.DebtDay;
        if (overdueDays >= DebtGraceDays && hero.SetFlag("debt:overdue"))
        {
            notes.Add($"Termin spłaty długu minął. Lichwiarz wysłał egzekutorów – szukaj ich w porcie i Starym Mieście. Dług: {hero.Debt} g.");
        }
        else if (overdueDays == DebtGraceDays - 1)
        {
            notes.Add($"Jutro mija termin spłaty długu ({hero.Debt} g).");
        }

        if (hero.Gold >= hero.Debt)
        {
            hero.SetFlag("debt:canpay");
        }
        else
        {
            hero.ClearFlag("debt:canpay");
        }
    }

    private static void TickAddiction(Hero hero, List<string> notes)
    {
        if (hero.Addiction < AddictionThreshold)
        {
            return;
        }

        int since = hero.Day - hero.LastLotusDay;
        if (since >= WithdrawalDays)
        {
            hero.Addiction = 0;
            hero.Craving = false;
            notes.Add("Głód lotosu mija. Tydzień bez fajki wystarczył, by ręce przestały się trząść.");
            return;
        }

        if (since >= CravingAfterDays && !hero.Craving)
        {
            hero.Craving = true;
            notes.Add("Głód lotosu. Ręce się trzęsą, cel ucieka sprzed oczu (−15% obrażeń, −10 trafienia), dopóki nie zapalisz albo nie przeczekasz tygodnia.");
        }
    }
}
