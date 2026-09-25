namespace PyramidTreasureConsoleRPG;

public enum RouletteColor
{
    Green,
    Black,
    Red,
}

/// <summary>
/// Kasyno. Każda gra pobiera stawkę Z GÓRY i wypłaca wygraną – dzięki temu nie da się
/// wygrać bez ryzyka ani postawić ujemnej kwoty. Czyste funkcje wypłat są publiczne dla testów.
/// </summary>
public static class Casino
{
    public const int MaxBet = 500;
    public const int BlackjackTarget = 21;
    public const int DealerStandsOn = 17;

    private static readonly int[] BlackNumbers = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };

    public static void Visit(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            GameIO.Header("Kasyno");
            GameIO.WriteLine($"Twoje złoto: {hero.Gold}   (maksymalna stawka: {MaxBet})", ConsoleColor.DarkYellow);
            GameIO.Menu("W co grasz?", "Ruletka", "Jednoręki bandyta", "Blackjack", "Kości", "Wyjdź z kasyna");
            int choice = GameIO.ReadMenuChoice(5);
            GameIO.Clear();
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
                case 5:
                    GameIO.WriteLine("Wychodzisz z kasyna...");
                    return;
            }
        }
    }

    // ---------- czyste reguły (testowalne) ----------

    public static RouletteColor ColorOf(int number)
    {
        if (number == 0)
        {
            return RouletteColor.Green;
        }

        return BlackNumbers.Contains(number) ? RouletteColor.Black : RouletteColor.Red;
    }

    /// <summary>Wypłata (łącznie ze zwrotem stawki) za bęben automatu. 0 = przegrana.</summary>
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

    /// <summary>Wartość ręki w blackjacku. Karty 1–13 (1 = as, 11–13 = figury).</summary>
    public static int BlackjackScore(IEnumerable<int> cards)
    {
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
        _ => card.ToString(),
    };

    // ---------- wspólna obsługa stawek ----------

    /// <summary>Czyta stawkę 1..min(MaxBet, złoto). 0 = rezygnacja. Ujemne i za duże kwoty są odrzucane.</summary>
    public static int ReadBet(Hero hero)
    {
        int max = Math.Min(MaxBet, hero.Gold);
        if (max < 1)
        {
            GameIO.Error("Nie masz złota. Krupier pokazuje ci drzwi.");
            return 0;
        }

        GameIO.WriteLine($"Masz {hero.Gold} złota. Stawka od 1 do {max}, 0 = rezygnacja.", ConsoleColor.DarkYellow);
        return GameIO.ReadChoice(0, max, "Ile stawiasz? ");
    }

    private static void PlayLoop(Hero hero, Action<Hero, int> game)
    {
        while (true)
        {
            int bet = ReadBet(hero);
            if (bet == 0)
            {
                GameIO.Clear();
                return;
            }

            hero.Gold -= bet;
            game(hero, bet);
            GameIO.WriteLine($"Aktualny stan konta: {hero.Gold} złota.", ConsoleColor.DarkYellow);
            GameIO.Menu("Grasz dalej?", "Tak", "Nie");
            bool again = GameIO.ReadMenuChoice(2) == 1;
            GameIO.Clear();
            if (!again)
            {
                return;
            }
        }
    }

    private static void Win(Hero hero, int payout)
    {
        hero.Gold += payout;
        GameIO.Success($"Wygrałeś {payout} złota!");
    }

    // ---------- gry ----------

    private static void Roulette(Hero hero, int bet)
    {
        GameIO.Menu("Na co stawiasz?", "Czarne (x2)", "Czerwone (x2)", "Konkretny numer 0-36 (x36)");
        int choice = GameIO.ReadMenuChoice(3);
        int number = choice == 3 ? GameIO.ReadChoice(0, 36, "Numer (0-36): ") : -1;
        int winning = Rng.Range(0, 36);
        RouletteColor color = ColorOf(winning);
        GameIO.WriteLine($"Kulka zatrzymuje się na: {winning} ({ColorName(color)}).", ConsoleColor.Cyan);

        bool won = choice switch
        {
            1 => color == RouletteColor.Black,
            2 => color == RouletteColor.Red,
            _ => winning == number,
        };
        if (won)
        {
            Win(hero, choice == 3 ? bet * 36 : bet * 2);
        }
        else
        {
            GameIO.Error("Niestety, przegrałeś.");
        }
    }

    private static string ColorName(RouletteColor color) => color switch
    {
        RouletteColor.Black => "czarne",
        RouletteColor.Red => "czerwone",
        _ => "zielone",
    };

    private static void SlotMachine(Hero hero, int bet)
    {
        int first = Rng.Range(1, 7);
        int second = Rng.Range(1, 7);
        int third = Rng.Range(1, 7);
        GameIO.WriteLine("+-----+-----+-----+");
        GameIO.WriteLine($"|  {ReelSymbol(first)}  |  {ReelSymbol(second)}  |  {ReelSymbol(third)}  |");
        GameIO.WriteLine("+-----+-----+-----+");

        int payout = SlotPayout(first, second, third, bet);
        if (payout == 0)
        {
            GameIO.Error("Nic z tego. Automat połyka twoje złoto.");
        }
        else
        {
            if (first == 7 && second == 7 && third == 7)
            {
                GameIO.WriteLine("JACKPOT!!!", ConsoleColor.Magenta);
            }

            Win(hero, payout);
        }
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

    private static void Blackjack(Hero hero, int bet)
    {
        var player = new List<int> { DrawCard(), DrawCard() };
        var dealer = new List<int> { DrawCard(), DrawCard() };
        GameIO.WriteLine($"Twoje karty: {Hand(player)} = {BlackjackScore(player)}");
        GameIO.WriteLine($"Karta krupiera: {CardName(dealer[0])}");

        if (BlackjackScore(player) == BlackjackTarget)
        {
            if (BlackjackScore(dealer) == BlackjackTarget)
            {
                GameIO.WriteLine($"Karty krupiera: {Hand(dealer)} = 21. Remis – stawka wraca.");
                hero.Gold += bet;
            }
            else
            {
                GameIO.WriteLine("BLACKJACK!", ConsoleColor.Magenta);
                Win(hero, bet * 5 / 2);
            }

            return;
        }

        while (true)
        {
            GameIO.Menu("Co robisz?", "Dobierz kartę", "Pasuj");
            if (GameIO.ReadMenuChoice(2) == 2)
            {
                break;
            }

            player.Add(DrawCard());
            GameIO.WriteLine($"Twoje karty: {Hand(player)} = {BlackjackScore(player)}");
            if (BlackjackScore(player) > BlackjackTarget)
            {
                GameIO.Error("Przebiłeś! Przegrywasz.");
                return;
            }
        }

        while (BlackjackScore(dealer) < DealerStandsOn)
        {
            dealer.Add(DrawCard());
        }

        int playerScore = BlackjackScore(player);
        int dealerScore = BlackjackScore(dealer);
        GameIO.WriteLine($"Karty krupiera: {Hand(dealer)} = {dealerScore}");
        if (dealerScore > BlackjackTarget || playerScore > dealerScore)
        {
            Win(hero, bet * 2);
        }
        else if (playerScore == dealerScore)
        {
            GameIO.Info("Remis – stawka wraca.");
            hero.Gold += bet;
        }
        else
        {
            GameIO.Error("Krupier wygrywa.");
        }
    }

    private static string Hand(List<int> cards) => string.Join(", ", cards.Select(CardName));

    private static int DrawCard() => Rng.Range(1, 13);

    private static void Craps(Hero hero, int bet)
    {
        int roll = RollDice();
        GameIO.WriteLine($"Wyrzucono: {roll}");
        if (roll is 7 or 11)
        {
            Win(hero, bet * 2);
            return;
        }

        if (roll is 2 or 3 or 12)
        {
            GameIO.Error("Przegrałeś!");
            return;
        }

        int point = roll;
        GameIO.Info($"Twoim punktem jest {point}. Rzucasz, aż wypadnie {point} (wygrana) albo 7 (przegrana).");
        while (true)
        {
            GameIO.Pause(500);
            int next = RollDice();
            GameIO.WriteLine($"Wyrzucono: {next}");
            if (next == point)
            {
                Win(hero, bet * 2);
                return;
            }

            if (next == 7)
            {
                GameIO.Error("Siódemka. Przegrałeś!");
                return;
            }
        }
    }

    private static int RollDice() => Rng.Range(1, 6) + Rng.Range(1, 6);
}
