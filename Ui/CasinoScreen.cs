namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Ekran kasyna. Stawka jest pobierana z góry, wypłata dopisywana po grze.</summary>
public sealed class CasinoScreen
{
    private readonly IGameIO io;
    private readonly IRandomSource rng;

    public CasinoScreen(IGameIO io, IRandomSource rng)
    {
        this.io = io ?? throw new ArgumentNullException(nameof(io));
        this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.Header("Kasyno");
            io.WriteLine($"Twoje złoto: {hero.Gold}   (maksymalna stawka: {CasinoEngine.MaxBet})", ConsoleColor.DarkYellow);
            int choice = io.Menu("W co grasz?", "Ruletka", "Jednoręki bandyta", "Blackjack", "Kości", "Wyjdź z kasyna");
            io.Clear();
            switch (choice)
            {
                case 1:
                    PlayLoop(hero, Roulette);
                    break;
                case 2:
                    PlayLoop(hero, SlotMachine);
                    break;
                case 3:
                    PlayLoop(hero, Blackjack);
                    break;
                case 4:
                    PlayLoop(hero, Craps);
                    break;
                default:
                    io.WriteLine("Wychodzisz z kasyna...");
                    return;
            }
        }
    }

    /// <summary>Czyta stawkę 1..min(MaxBet, złoto). 0 = rezygnacja.</summary>
    private int ReadBet(Hero hero)
    {
        int max = Math.Min(CasinoEngine.MaxBet, hero.Gold);
        if (max < 1)
        {
            io.ShowError("Nie masz złota. Krupier pokazuje ci drzwi.");
            return 0;
        }

        io.WriteLine($"Masz {hero.Gold} złota. Stawka od 1 do {max}, 0 = rezygnacja.", ConsoleColor.DarkYellow);
        return io.ReadNumber("Ile stawiasz? ", 0, max);
    }

    private void PlayLoop(Hero hero, Func<Hero, int, int> game)
    {
        while (true)
        {
            int bet = ReadBet(hero);
            if (bet == 0)
            {
                io.Clear();
                return;
            }

            hero.Gold -= bet;
            int payout = game(hero, bet);
            if (payout > 0)
            {
                hero.Gold += payout;
                io.ShowSuccess($"Wygrałeś {payout} złota!");
            }

            io.WriteLine($"Aktualny stan konta: {hero.Gold} złota.", ConsoleColor.DarkYellow);
            bool again = io.Menu("Grasz dalej?", "Tak", "Nie") == 1;
            io.Clear();
            if (!again)
            {
                return;
            }
        }
    }

    private int Roulette(Hero hero, int bet)
    {
        int choice = io.Menu("Na co stawiasz?", "Czarne (x2)", "Czerwone (x2)", "Konkretny numer 0-36 (x36)");
        var betType = (RouletteBet)choice;
        int number = betType == RouletteBet.Number ? io.ReadNumber("Numer (0-36): ", 0, 36) : -1;
        RouletteResult result = CasinoEngine.PlayRoulette(bet, betType, number, rng);
        io.WriteLine($"Kulka zatrzymuje się na: {result.WinningNumber} ({CasinoEngine.ColorName(result.Color)}).", ConsoleColor.Cyan);
        if (!result.Won)
        {
            io.ShowError("Niestety, przegrałeś.");
        }

        return result.Payout;
    }

    private int SlotMachine(Hero hero, int bet)
    {
        SlotResult result = CasinoEngine.SpinSlots(bet, rng);
        io.WriteLine("+-----+-----+-----+");
        io.WriteLine($"|  {ReelSymbol(result.Reels[0])}  |  {ReelSymbol(result.Reels[1])}  |  {ReelSymbol(result.Reels[2])}  |");
        io.WriteLine("+-----+-----+-----+");
        if (result.Jackpot)
        {
            io.WriteLine("JACKPOT!!!", ConsoleColor.Magenta);
        }
        else if (result.Payout == 0)
        {
            io.ShowError("Nic z tego. Automat połyka twoje złoto.");
        }

        return result.Payout;
    }

    private static string ReelSymbol(int value) => value switch
    {
        1 => "@",
        2 => "$",
        3 => "%",
        4 => "&",
        5 => "*",
        6 => "#",
        _ => "7",
    };

    private int Blackjack(Hero hero, int bet)
    {
        var game = new BlackjackGame(bet, rng);
        io.WriteLine($"Twoje karty: {Hand(game.PlayerCards)} = {game.PlayerScore}");
        io.WriteLine($"Karta krupiera: {CasinoEngine.CardName(game.DealerCards[0])}");

        while (!game.IsFinished)
        {
            if (io.Menu("Co robisz?", "Dobierz kartę", "Pasuj") == 1)
            {
                game.Hit();
                io.WriteLine($"Twoje karty: {Hand(game.PlayerCards)} = {game.PlayerScore}");
            }
            else
            {
                game.Stand();
            }
        }

        switch (game.Outcome)
        {
            case BlackjackOutcome.PlayerBlackjack:
                io.WriteLine("BLACKJACK!", ConsoleColor.Magenta);
                break;
            case BlackjackOutcome.PlayerBust:
                io.ShowError("Przebiłeś! Przegrywasz.");
                break;
            case BlackjackOutcome.Push:
                io.WriteLine($"Karty krupiera: {Hand(game.DealerCards)} = {game.DealerScore}");
                io.ShowInfo("Remis – stawka wraca.");
                break;
            case BlackjackOutcome.DealerWins:
                io.WriteLine($"Karty krupiera: {Hand(game.DealerCards)} = {game.DealerScore}");
                io.ShowError("Krupier wygrywa.");
                break;
            default:
                io.WriteLine($"Karty krupiera: {Hand(game.DealerCards)} = {game.DealerScore}");
                break;
        }

        return game.Payout;
    }

    private static string Hand(IReadOnlyList<int> cards) => string.Join(", ", cards.Select(CasinoEngine.CardName));

    private int Craps(Hero hero, int bet)
    {
        CrapsResult result = CasinoEngine.PlayCraps(bet, rng);
        io.WriteLine($"Wyrzucono: {result.Rolls[0]}");
        if (result.Point.HasValue)
        {
            io.ShowInfo($"Twoim punktem jest {result.Point}. Rzucasz, aż wypadnie {result.Point} (wygrana) albo 7 (przegrana).");
            foreach (int roll in result.Rolls.Skip(1))
            {
                io.Pause(400);
                io.WriteLine($"Wyrzucono: {roll}");
            }
        }

        if (!result.Won)
        {
            io.ShowError(result.Point.HasValue ? "Siódemka. Przegrałeś!" : "Przegrałeś!");
        }

        return result.Payout;
    }
}
