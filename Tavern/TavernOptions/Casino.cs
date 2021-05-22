using System;
using System.Collections.Generic;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Casino
    {
        public static void CasinoOptions(IClass characterClass)
        {
            if (characterClass != null)
            {
                bool value = true;
                while (value)
                {
                    Console.WriteLine("Co chcesz zrobić:");
                    Console.WriteLine("1: Ruletka.");
                    Console.WriteLine("2. Jednoręki bandyta.");
                    Console.WriteLine("3. Wyjdź z kasyna.");
                    int choice = StandardFunctions.ToInt32(Console.ReadLine());
                    Console.Clear();
                    switch (choice)
                    {
                        case 1:
                            Casino.Roulette(characterClass);
                            break;

                        case 2:
                            Casino.SlotMachine(characterClass);
                            break;

                        case 3:
                            value = StandardFunctions.ExitRoom();
                            break;

                        default:
                            StandardFunctions.NoOption();
                            break;
                    }
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
                bool ifPlay;
                if (choice == 1)
                {
                    ifPlay = true;
                }
                else
                {
                    ifPlay = false;
                    ifRoulette = false;
                }

                while (ifPlay && ifRoulette)
                {
                    bool ifGold = true;
                    int gold = 0;
                    while (ifGold)
                    {
                        Console.WriteLine("Ile chcesz postawić? / 0 - zrezygnuj.");

                        ////Zrobić reszte try / catch
                        gold = StandardFunctions.ToInt32(Console.ReadLine());
                        if (gold == 0)
                        {
                            ifPlay = false;
                            break;
                        }

                        if (gold > characterClass.Gold)
                        {
                            Dialogues.NoGold();
                        }
                        else
                        {
                            ifGold = false;
                        }
                    }

                    if (gold > 0)
                    {
                        Console.WriteLine("1: Czarne. / 2. Czerwone.");
                        int color = StandardFunctions.ToInt32(Console.ReadLine());

                        List<int> roulette = new List<int> { 1, 2 };
                        int index = roulette.RandomElement();
                        characterClass.Gold -= gold;
                        if (index == color)
                        {
                            if (index == 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Czarne! Wygrałeś!");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Czerwone! Wygrałeś!");
                                Console.ResetColor();
                            }

                            gold *= 2;
                            characterClass.Gold += gold;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                            Console.ResetColor();
                        }
                        else
                        {
                            if (index == 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Czarne! Przegrałeś!");
                                Console.ResetColor();
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Czerwone! Przegrałeś!");
                                Console.ResetColor();
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }
        }

        private static void SlotMachine(IClass characterClass)
        {
            bool ifSlotMachine = true;
            while (ifSlotMachine)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Zagraj.");
                Console.WriteLine("2: Zrezygnuj.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                bool ifPlay;
                if (choice == 1)
                {
                    ifPlay = true;
                }
                else
                {
                    ifPlay = false;
                    ifSlotMachine = false;
                }

                while (ifPlay && ifSlotMachine)
                {
                    bool ifGold = true;
                    int gold = 0;
                    while (ifGold)
                    {
                        Console.WriteLine("Ile chcesz postawić? / 0 - zrezygnuj.");

                        ////Zrobić reszte try / catch
                        gold = StandardFunctions.ToInt32(Console.ReadLine());
                        if (gold == 0)
                        {
                            ifPlay = false;
                            break;
                        }

                        if (gold > characterClass.Gold)
                        {
                            Dialogues.NoGold();
                        }
                        else
                        {
                            ifGold = false;
                        }
                    }

                    if (gold > 0)
                    {

                        List<int> slotMachine = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
                        int firstReel = slotMachine.RandomElement();
                        int secondReel = slotMachine.RandomElement();
                        int thirdReel = slotMachine.RandomElement();
                        characterClass.Gold -= gold;

                        if (firstReel == secondReel)
                        {
                            if (firstReel == secondReel && firstReel == thirdReel)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"{firstReel}, {secondReel}, {thirdReel}");
                                switch (firstReel)
                                {
                                    case 1:
                                        gold *= 3;
                                        characterClass.Gold += gold;
                                        break;
                                    case 2:
                                        gold *= 4;
                                        characterClass.Gold += gold;
                                        break;
                                    case 3:
                                        gold *= 5;
                                        characterClass.Gold += gold;
                                        break;
                                    case 4:
                                        gold *= 6;
                                        characterClass.Gold += gold;
                                        break;
                                    case 5:
                                        gold *= 7;
                                        characterClass.Gold += gold;
                                        break;
                                    case 6:
                                        gold *= 8;
                                        characterClass.Gold += gold;
                                        break;
                                    case 7:
                                        gold *= 9;
                                        characterClass.Gold += gold;
                                        break;
                                }
                                Console.WriteLine($"Wygrałeś linię! +{gold} złota");
                                Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"{firstReel}, {secondReel}, {thirdReel}");
                                switch (firstReel)
                                {
                                    case 1:
                                        characterClass.Gold += gold;
                                        break;
                                    case 2:
                                        characterClass.Gold += gold;
                                        break;
                                    case 3:
                                        characterClass.Gold += gold;
                                        break;
                                    case 4:
                                        gold *= 3;
                                        characterClass.Gold += gold;
                                        break;
                                    case 5:
                                        gold *= 5;
                                        characterClass.Gold += gold;
                                        break;
                                    case 6:
                                        gold *= 7;
                                        characterClass.Gold += gold;
                                        break;
                                    case 7:
                                        gold *= 9;
                                        characterClass.Gold += gold;
                                        break;
                                }
                                Console.WriteLine($"Wygrałeś 2 z lini! +{gold} złota");
                                Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{firstReel}, {secondReel}, {thirdReel}");
                            Console.WriteLine("Przegrałeś!");
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Aktualny stan konta: {characterClass.Gold}.");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }
    }
}