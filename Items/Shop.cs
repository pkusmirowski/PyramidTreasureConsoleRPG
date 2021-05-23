using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Shop
    {
        public static void PotionShop(IClass characterClass)
        {
            if (characterClass != null)
            {
                bool value = true;
                while (value)
                {
                    Console.WriteLine("Co chcesz zrobić:");
                    Console.WriteLine("1: Kup małą miksture leczniczą. 20g");
                    Console.WriteLine("2: Kup średnią miksturę leczniczą. 50g");
                    Console.WriteLine("3. Kup dużą miksturę leczniczą. 100g");
                    Console.WriteLine("4. Wyjdź z sklepu.");
                    int choice = StandardFunctions.ToInt32(Console.ReadLine());
                    Console.Clear();
                    switch (choice)
                    {
                        case 1:
                            BuySmallPotion(characterClass);
                            break;

                        case 2:
                            BuyMediumPotion(characterClass);
                            break;

                        case 3:
                            BuyLargePotion(characterClass);
                            break;

                        case 4:
                            value = StandardFunctions.ExitRoom();
                            break;

                        default:
                            StandardFunctions.NoOption();
                            break;
                    }
                }
            }
        }

        private static void BuyLargePotion(IClass characterClass)
        {
            if (characterClass.Gold >= 100)
            {
                characterClass.Inventory.Add(new LargePotion());
                characterClass.Gold -= 100;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Kupiłeś dużą miksture!\n");
                Console.ResetColor();
            }
            else
            {
                Dialogues.NoGold();
            }
        }

        private static void BuyMediumPotion(IClass characterClass)
        {
            if (characterClass.Gold >= 50)
            {
                characterClass.Inventory.Add(new MediumPotion());
                characterClass.Gold -= 50;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Kupiłeś średnią miksture!\n");
                Console.ResetColor();
            }
            else
            {
                Dialogues.NoGold();
            }
        }

        private static void BuySmallPotion(IClass characterClass)
        {
            if (characterClass.Gold >= 20)
            {
                characterClass.Inventory.Add(new SmallPotion());
                characterClass.Gold -= 20;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Kupiłeś małą miksture!\n");
                Console.ResetColor();
            }
            else
            {
                Dialogues.NoGold();
            }
        }
    }
}