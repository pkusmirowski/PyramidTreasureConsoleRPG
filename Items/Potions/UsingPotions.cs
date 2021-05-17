using System;
using System.Linq;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class UsingPotions
    {
        public static void PotionOptions(IClass characterClass)
        {
            if (characterClass != null)
            {
                bool value = true;
                while (value)
                {
                    Console.WriteLine("Co chcesz zrobić:");
                    Console.WriteLine("1: Wyświetl mikstury, które posiadasz w sakwie.");
                    Console.WriteLine("2: Wypij jedną z mikstur.");
                    Console.WriteLine("3: Wyjdź.");
                    int choice = StandardFunctions.ToInt32(Console.ReadLine());
                    Console.Clear();
                    switch (choice)
                    {
                        case 1:
                            ShowPotions(characterClass);
                            break;

                        case 2:
                            value = DrinkPotion(characterClass);
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

        private static bool DrinkPotion(IClass characterClass)
        {
            bool value = true;
            while (value)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Wypij małą miksturę.");
                Console.WriteLine("2: Wypij średnią miksturę.");
                Console.WriteLine("3. Wypij dużą miksturę.");
                Console.WriteLine("4. Wyjdź z tawerny.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                if (choice > 0 && choice <= 3)
                {
                    if (characterClass.Hp == characterClass.MaxHP)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Masz maksymalną ilość punktów życia {characterClass.Hp}!");
                        Console.ResetColor();
                    }
                    else
                    {
                        var foundPotion = characterClass.Inventory.Find(s => s.Id == choice);
                        if (foundPotion != null)
                        {
                            var previousHP = characterClass.Hp;
                            characterClass.Hp += foundPotion.RestoreHP;
                            if (characterClass.Hp > previousHP)
                            {
                                characterClass.Hp = characterClass.MaxHP;
                            }

                            characterClass.Inventory.Remove(foundPotion);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Uleczłeś się.\nTwoje punkty życia wynoszą {characterClass.Hp}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Nie masz tych mikstur!");
                            Console.ResetColor();
                        }
                    }
                }

                if (choice == 4)
                {
                    value = StandardFunctions.ExitRoom();
                }
                else
                {
                    StandardFunctions.NoOption();
                }
            }

            return value;
        }

        private static void ShowPotions(IClass characterClass)
        {
            var duplicates = characterClass.Inventory
                .GroupBy(x => new { x.Name })
                .Select(group => new { Name = group.Key, Count = group.Count() })
                .OrderByDescending(x => x.Count);
            if (duplicates == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Nie posiadasz żadnych mikstur!");
                Console.ResetColor();
            }
            else
            {
                foreach (var x in duplicates)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Ilość: " + x.Count + " " + x.Name);
                    Console.ResetColor();
                }
            }
        }
    }
}