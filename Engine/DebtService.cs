namespace PyramidTreasureConsoleRPG.Engine;

public enum LoanOutcome
{
    Granted,
    TooMuch,
    AlreadyInDebt,
}

public enum RepayOutcome
{
    Repaid,
    Partial,
    NotEnoughGold,
    NoDebt,
}

/// <summary>Lichwiarz w kasynie: pożyczka do 500 g, 10% dziennie, po 5 dniach egzekutorzy.</summary>
public static class DebtService
{
    public static LoanOutcome Borrow(Hero hero, int amount)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.Debt > 0)
        {
            return LoanOutcome.AlreadyInDebt;
        }

        if (amount <= 0 || amount > DayService.MaxLoan)
        {
            return LoanOutcome.TooMuch;
        }

        hero.Gold += amount;
        hero.Debt = amount;
        hero.DebtDay = hero.Day;
        hero.ClearFlag("debt:overdue");
        return LoanOutcome.Granted;
    }

    public static RepayOutcome Repay(Hero hero, int amount)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.Debt <= 0)
        {
            return RepayOutcome.NoDebt;
        }

        if (amount <= 0 || hero.Gold < amount)
        {
            return RepayOutcome.NotEnoughGold;
        }

        int paid = Math.Min(amount, hero.Debt);
        hero.Gold -= paid;
        hero.Debt -= paid;
        if (hero.Debt == 0)
        {
            hero.ClearFlag("debt:overdue");
            hero.ClearFlag("debt:canpay");
            return RepayOutcome.Repaid;
        }

        return RepayOutcome.Partial;
    }
}
