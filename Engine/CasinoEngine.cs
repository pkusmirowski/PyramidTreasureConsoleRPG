namespace PyramidTreasureConsoleRPG.Engine;

public enum RouletteColor
{
    Green,
    Black,
    Red,
}

public enum RouletteBet
{
    Black = 1,
    Red = 2,
    Number = 3,
}

public sealed record RouletteResult(int WinningNumber, RouletteColor Color, bool Won, int Payout);

public sealed record SlotResult(IReadOnlyList<int> Reels, int Payout, bool Jackpot);

public sealed record CrapsResult(IReadOnlyList<int> Rolls, int? Point, bool Won, int Payout);

public enum BlackjackOutcome
{
    InProgress,
    PlayerBlackjack,
    PlayerBust,
    PlayerWins,
    Push,
    DealerWins,
}

/// <summary>
/// Reguły kasyna. Każda gra zakłada, że stawka została już pobrana; wynik zawiera wypłatę
/// łącznie ze zwrotem stawki (0 = przegrana). Wszystko jest czyste i testowalne.
/// </summary>
public static class CasinoEngine
{
    public const int MaxBet = 500;
    public const int BlackjackTarget = 21;
    public const int DealerStandsOn = 17;

    private static readonly int[] BlackNumbers = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };

    public static RouletteColor ColorOf(int number)
    {
        if (number == 0)
        {
            return RouletteColor.Green;
        }

        return BlackNumbers.Contains(number) ? RouletteColor.Black : RouletteColor.Red;
    }

    public static string ColorName(RouletteColor color) => color switch
    {
        RouletteColor.Black => "czarne",
        RouletteColor.Red => "czerwone",
        _ => "zielone",
    };

    public static RouletteResult PlayRoulette(int bet, RouletteBet betType, int number, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        int winning = rng.Range(0, 36);
        RouletteColor color = ColorOf(winning);
        bool won = betType switch
        {
            RouletteBet.Black => color == RouletteColor.Black,
            RouletteBet.Red => color == RouletteColor.Red,
            _ => winning == number,
        };
        int payout = !won ? 0 : betType == RouletteBet.Number ? bet * 36 : bet * 2;
        return new RouletteResult(winning, color, won, payout);
    }

    /// <summary>Wypłata (łącznie ze zwrotem stawki) za bębny automatu. 0 = przegrana.</summary>
    public static int SlotPayout(int first, int second, int third, int bet)
    {
        if (first == 7 && second == 7 && third == 7)
        {
            return bet * 50;
        }

        if (first == second && second == third)
        {
            return bet * 10;
        }

        if (first == second || second == third || first == third)
        {
            return bet * 3 / 2;
        }

        return 0;
    }

    public static SlotResult SpinSlots(int bet, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        int[] reels = { rng.Range(1, 7), rng.Range(1, 7), rng.Range(1, 7) };
        int payout = SlotPayout(reels[0], reels[1], reels[2], bet);
        bool jackpot = reels[0] == 7 && reels[1] == 7 && reels[2] == 7;
        return new SlotResult(reels, payout, jackpot);
    }

    public static CrapsResult PlayCraps(int bet, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        var rolls = new List<int>();
        int roll = RollDice(rng);
        rolls.Add(roll);
        if (roll is 7 or 11)
        {
            return new CrapsResult(rolls, null, true, bet * 2);
        }

        if (roll is 2 or 3 or 12)
        {
            return new CrapsResult(rolls, null, false, 0);
        }

        int point = roll;
        while (true)
        {
            int next = RollDice(rng);
            rolls.Add(next);
            if (next == point)
            {
                return new CrapsResult(rolls, point, true, bet * 2);
            }

            if (next == 7)
            {
                return new CrapsResult(rolls, point, false, 0);
            }
        }
    }

    /// <summary>Wartość ręki w blackjacku. Karty 1–13 (1 = as, 11–13 = figury).</summary>
    public static int BlackjackScore(IEnumerable<int> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        int score = 0;
        int aces = 0;
        foreach (int card in cards)
        {
            if (card == 1)
            {
                aces++;
                score += 11;
            }
            else
            {
                score += Math.Min(card, 10);
            }
        }

        while (score > BlackjackTarget && aces > 0)
        {
            score -= 10;
            aces--;
        }

        return score;
    }

    public static string CardName(int card) => card switch
    {
        1 => "A",
        11 => "J",
        12 => "Q",
        13 => "K",
        _ => card.ToString(System.Globalization.CultureInfo.InvariantCulture),
    };

    private static int RollDice(IRandomSource rng) => rng.Range(1, 6) + rng.Range(1, 6);
}

/// <summary>Jedna partia blackjacka. Naturalne 21 wypłaca 5/2, remis zwraca stawkę, wygrana 2x.</summary>
public sealed class BlackjackGame
{
    private readonly IRandomSource rng;
    private readonly List<int> player = new();
    private readonly List<int> dealer = new();

    public BlackjackGame(int bet, IRandomSource rng)
    {
        this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        Bet = bet;
        player.Add(Draw());
        player.Add(Draw());
        dealer.Add(Draw());
        dealer.Add(Draw());
        if (PlayerScore == CasinoEngine.BlackjackTarget)
        {
            Outcome = DealerScore == CasinoEngine.BlackjackTarget ? BlackjackOutcome.Push : BlackjackOutcome.PlayerBlackjack;
        }
    }

    public int Bet { get; }

    public IReadOnlyList<int> PlayerCards => player;

    public IReadOnlyList<int> DealerCards => dealer;

    public int PlayerScore => CasinoEngine.BlackjackScore(player);

    public int DealerScore => CasinoEngine.BlackjackScore(dealer);

    public BlackjackOutcome Outcome { get; private set; } = BlackjackOutcome.InProgress;

    public bool IsFinished => Outcome != BlackjackOutcome.InProgress;

    public int Payout => Outcome switch
    {
        BlackjackOutcome.PlayerBlackjack => Bet * 5 / 2,
        BlackjackOutcome.PlayerWins => Bet * 2,
        BlackjackOutcome.Push => Bet,
        _ => 0,
    };

    public void Hit()
    {
        EnsureInProgress();
        player.Add(Draw());
        if (PlayerScore > CasinoEngine.BlackjackTarget)
        {
            Outcome = BlackjackOutcome.PlayerBust;
        }
    }

    public void Stand()
    {
        EnsureInProgress();
        while (DealerScore < CasinoEngine.DealerStandsOn)
        {
            dealer.Add(Draw());
        }

        int playerScore = PlayerScore;
        int dealerScore = DealerScore;
        Outcome = dealerScore > CasinoEngine.BlackjackTarget || playerScore > dealerScore
            ? BlackjackOutcome.PlayerWins
            : playerScore == dealerScore ? BlackjackOutcome.Push : BlackjackOutcome.DealerWins;
    }

    private void EnsureInProgress()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("Partia jest zakończona.");
        }
    }

    private int Draw() => rng.Range(1, 13);
}
