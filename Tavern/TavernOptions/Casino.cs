using System;
using System.Collections.Generic;
using System.Linq;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Casino
    {
        private const int MaxScore = 21;
        private const int DealerMaxScore = 17;
        private const int MinBet = 1;
        private const int MaxBet = 100;

        public static void CasinoOptions(IClass characterClass)
        {
            if (characterClass == null) return;

            bool value = true;
            while (value)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Ruletka.");
                Console.WriteLine("2. Jednoręki bandyta.");
                Console.WriteLine("3. BlackJack.");
                Console.WriteLine("4. Kości.");
                Console.WriteLine("5. Wyjdź z kasyna.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        Roulette(characterClass);
                        break;
                    case 2:
                        SlotMachine(characterClass);
                        break;
                    case 3:
                        Blackjack(characterClass);
                        break;
                    case 4:
                        Craps(characterClass);
                        break;
                    case 5:
                        value = StandardFunctions.ExitRoom();
                        break;
                    default:
                        StandardFunctions.NoOption();
                        break;
                }
            }
        }

        private static void Roulette(IClass characterClass)
        {
            bool ifRoulette = true;
            while (ifRoulette)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Zagraj.");
                Console.WriteLine("2: Zrezygnuj.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                bool ifPlay = choice == 1;

                while (ifPlay && ifRoulette)
                {
                    int gold = GetBetAmount(characterClass);
                    if (gold == 0)
                    {
                        ifPlay = false;
                        break;
                    }

                    Console.WriteLine("1: Czarne. / 2. Czerwone. / 3. Wybierz numer.");
                    int choice2 = StandardFunctions.ToInt32(Console.ReadLine());

                    if (choice2 == 1 || choice2 == 2)
                    {
                        PlayRouletteColor(characterClass, gold, choice2);
                    }
                    else if (choice2 == 3)
                    {
                        PlayRouletteNumber(characterClass, gold);
                    }
                }
            }
        }

        private static void PlayRouletteColor(IClass characterClass, int gold, int choice2)
        {
            int[] blackNumbers = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };
            int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            int winningNumber = new Random().Next(0, 37);

            if ((choice2 == 1 && blackNumbers.Contains(winningNumber)) || (choice2 == 2 && redNumbers.Contains(winningNumber)))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Wylosowano {winningNumber} - Gratulacje, wygrałeś!");
                gold *= 2;
                characterClass.Gold += gold;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Wylosowano {winningNumber} - Niestety, przegrałeś!");
            }
            Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
            Console.ResetColor();
        }

        private static void PlayRouletteNumber(IClass characterClass, int gold)
        {
            int number = GetRouletteNumber();
            int winningNumber = new Random().Next(0, 37);
            characterClass.Gold -= gold;

            if (winningNumber == number)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Wylosowano {winningNumber} - Gratulacje, wygrałeś!");
                gold *= 36;
                characterClass.Gold += gold;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Wylosowano {winningNumber} - Niestety, przegrałeś!");
            }
            Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
            Console.ResetColor();
        }

        private static int GetRouletteNumber()
        {
            int number = -1; // Initialize number to a default value
            bool ifNumber = true;
            while (ifNumber)
            {
                Console.WriteLine("Który numer chcesz obstawić (0-36)?");
                number = StandardFunctions.ToInt32(Console.ReadLine());

                if (number >= 0 && number <= 36)
                {
                    ifNumber = false;
                }
                else
                {
                    Console.WriteLine("Niepoprawny numer. Spróbuj ponownie.");
                }
            }
            return number;
        }

        private static int GetBetAmount(IClass characterClass)
        {
            int gold = 0;
            bool ifGold = true;
            while (ifGold)
            {
                Console.WriteLine("Ile chcesz postawić? / 0 - zrezygnuj.");
                gold = StandardFunctions.ToInt32(Console.ReadLine());

                if (gold == 0 || gold <= characterClass.Gold)
                {
                    ifGold = false;
                }
                else
                {
                    Dialogues.NoGold();
                }
            }
            return gold;
        }

        private static void SlotMachine(IClass characterClass)
        {
            bool playAgain = true;

            while (playAgain)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Zagraj.");
                Console.WriteLine("2: Zrezygnuj.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();

                if (choice == 2) break;

                int bet = GetBetAmount(characterClass);
                if (bet == 0) break;

                PlaySlotMachine(characterClass, bet);
            }
        }

        private static void PlaySlotMachine(IClass characterClass, int bet)
        {
            int[] reels = { 1, 2, 3, 4, 5, 6, 7 };
            int firstReel = GetRandomReel(reels);
            int secondReel = GetRandomReel(reels);
            int thirdReel = GetRandomReel(reels);

            Console.WriteLine("+-------+");
            Console.WriteLine($"| {firstReel} |");
            Console.WriteLine("+-------+");
            Console.WriteLine($"| {secondReel} |");
            Console.WriteLine("+-------+");
            Console.WriteLine($"| {thirdReel} |");
            Console.WriteLine("+-------+");

            if (firstReel == 7 && secondReel == 7 && thirdReel == 7)
            {
                int jackpot = 777 * bet;
                Console.WriteLine($"JACKPOT!!! Wygrałeś {jackpot} złota!");
                characterClass.Gold += jackpot;
            }
            else if (firstReel == secondReel && secondReel == thirdReel)
            {
                int payout = GetPayout(firstReel) * bet;
                Console.WriteLine($"Wygrałeś linię! +{payout} złota");
                characterClass.Gold += payout;
            }
            else if (firstReel == secondReel || secondReel == thirdReel || firstReel == thirdReel)
            {
                int payout = GetPayout(firstReel) * bet / 2;
                Console.WriteLine($"Wygrałeś 2 z lini! +{payout} złota");
                characterClass.Gold += payout;
            }
            else
            {
                Console.WriteLine("Przegrałeś!");
            }

            Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
            characterClass.Gold -= bet;
        }

        private static void Blackjack(IClass characterClass)
        {
            bool playAgain = true;

            while (playAgain)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Zagraj.");
                Console.WriteLine("2: Zrezygnuj.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();

                if (choice == 2) break;

                int bet = GetValidBet(characterClass);

                List<int> playerCards = new List<int> { DrawCard(), DrawCard() };
                List<int> dealerCards = new List<int> { DrawCard(), DrawCard() };

                Console.WriteLine($"Twoje karty: {string.Join(", ", playerCards)}");
                Console.WriteLine($"Karta krupiera: {dealerCards[0]}");

                bool playerTurn = true;

                while (playerTurn)
                {
                    Console.WriteLine("Co chcesz zrobić:");
                    Console.WriteLine("1: Dołóż kartę.");
                    Console.WriteLine("2: Pasuj.");
                    int turnChoice = StandardFunctions.ToInt32(Console.ReadLine());

                    if (turnChoice == 1)
                    {
                        playerCards.Add(DrawCard());
                        Console.WriteLine($"Twoje karty: {string.Join(", ", playerCards)}");
                        if (CalculateScore(playerCards) > MaxScore)
                        {
                            Console.WriteLine("Przegrałeś!");
                            playerTurn = false;
                        }
                    }
                    else if (turnChoice == 2)
                    {
                        playerTurn = false;
                    }
                }

                while (CalculateScore(dealerCards) < DealerMaxScore)
                {
                    dealerCards.Add(DrawCard());
                }

                Console.WriteLine($"Twoje karty: {string.Join(", ", playerCards)}");
                Console.WriteLine($"Karty krupiera: {string.Join(", ", dealerCards)}");

                int playerScore = CalculateScore(playerCards);
                int dealerScore = CalculateScore(dealerCards);

                if (playerScore > MaxScore || (dealerScore <= MaxScore && dealerScore > playerScore))
                {
                    Console.WriteLine("Przegrałeś!");
                    characterClass.Gold -= bet;
                }
                else if (playerScore == dealerScore)
                {
                    Console.WriteLine("Remis!");
                }
                else
                {
                    Console.WriteLine("Wygrałeś!");
                    characterClass.Gold += bet;
                }
            }
        }

        private static int GetValidBet(IClass characterClass)
        {
            int bet = 0;
            while (bet <= 0 || bet > characterClass.Gold)
            {
                Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                Console.WriteLine("Ile chcesz postawić?");
                bet = StandardFunctions.ToInt32(Console.ReadLine());
                if (bet <= 0)
                {
                    Console.WriteLine("Nieprawidłowa wartość. Postaw co najmniej 1 złoto.");
                }
                else if (bet > characterClass.Gold)
                {
                    Dialogues.NoGold();
                }
            }
            return bet;
        }

        private static void Craps(IClass characterClass)
        {
            bool playAgain = true;

            while (playAgain)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Zagraj.");
                Console.WriteLine("2: Zrezygnuj.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();

                if (choice == 2) break;

                int bet = GetValidCrapsBet(characterClass);

                int roll = RollDice();
                Console.WriteLine($"Wyrzucono: {roll}");

                if (roll == 7 || roll == 11)
                {
                    Console.WriteLine("Wygrałeś!");
                    characterClass.Gold += bet;
                }
                else if (roll == 2 || roll == 3 || roll == 12)
                {
                    Console.WriteLine("Przegrałeś!");
                    characterClass.Gold -= bet;
                }
                else
                {
                    int point = roll;
                    Console.WriteLine($"Twoim punktem jest {point}. Rzucaj ponownie, aby wygrać.");

                    bool continueRolling = true;

                    while (continueRolling)
                    {
                        int nextRoll = RollDice();
                        Console.WriteLine($"Wyrzucono: {nextRoll}");

                        if (nextRoll == point)
                        {
                            Console.WriteLine("Wygrałeś!");
                            characterClass.Gold += bet;
                            continueRolling = false;
                        }
                        else if (nextRoll == 7)
                        {
                            Console.WriteLine("Przegrałeś!");
                            characterClass.Gold -= bet;
                            continueRolling = false;
                        }
                    }
                }

                Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
            }
        }

        private static int GetValidCrapsBet(IClass characterClass)
        {
            int bet = 0;
            while (bet <= 0 || bet > characterClass.Gold || bet < MinBet || bet > MaxBet)
            {
                Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                Console.WriteLine($"Minimalna stawka to {MinBet} a maksymalna to {MaxBet} złota.");
                Console.WriteLine("Ile chcesz postawić?");
                bet = StandardFunctions.ToInt32(Console.ReadLine());
                if (bet <= 0)
                {
                    Console.WriteLine("Nieprawidłowa wartość. Postaw co najmniej 1 złoto.");
                }
                else if (bet > characterClass.Gold)
                {
                    Dialogues.NoGold();
                }
                else if (bet < MinBet || bet > MaxBet)
                {
                    Console.WriteLine($"Nieprawidłowa wartość. Stawka musi być pomiędzy {MinBet} a {MaxBet} złota.");
                }
            }
            return bet;
        }

        private static int RollDice()
        {
            Random random = new Random();
            int die1 = random.Next(1, 7);
            int die2 = random.Next(1, 7);
            return die1 + die2;
        }

        private static int DrawCard()
        {
            Random random = new Random();
            int cardValue = random.Next(2, 12);
            return cardValue > 10 ? 10 : cardValue;
        }

        private static int CalculateScore(List<int> cards)
        {
            int score = 0;
            int aceCount = 0;

            foreach (int card in cards)
            {
                if (card == 1)
                {
                    aceCount++;
                    score += 11;
                }
                else if (card > 10)
                {
                    score += 10;
                }
                else
                {
                    score += card;
                }
            }

            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        private static int GetRandomReel(int[] reels)
        {
            Random random = new Random();
            int index = random.Next(reels.Length);
            int reelValue = reels[index];

            switch (reelValue)
            {
                case 1:
                    Console.Write("🍇");
                    break;
                case 2:
                    Console.Write("🍉");
                    break;
                case 3:
                    Console.Write("🍊");
                    break;
                case 4:
                    Console.Write("🍋");
                    break;
                case 5:
                    Console.Write("🍓");
                    break;
                case 6:
                    Console.Write("🍒");
                    break;
                case 7:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("7️⃣");
                    Console.ResetColor();
                    break;
                default:
                    break;
            }

            Console.Write("  ");

            return reelValue;
        }

        private static int GetPayout(int reelValue)
        {
            return reelValue switch
            {
                1 => 3,
                2 => 4,
                3 => 5,
                4 => 6,
                5 => 7,
                6 => 8,
                7 => 9,
                _ => throw new ArgumentException("Niepoprawna wartość bębna."),
            };
        }
    }
}