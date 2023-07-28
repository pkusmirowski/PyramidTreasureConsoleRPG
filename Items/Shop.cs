using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Shop
    {
        public static void PotionShop(IClass characterClass)
        {
            if (characterClass == null)
            {
                return;
            }

            bool value = true;
            while (value)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Kup małą miksture leczniczą. 20g");
                Console.WriteLine("2: Kup średnią miksturę leczniczą. 50g");
                Console.WriteLine("3. Kup dużą miksturę leczniczą. 100g");
                Console.WriteLine("4. Wyjdź z sklepu.");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    StandardFunctions.NoOption();
                    continue;
                }

                Console.Clear();

                switch (choice)
                {
                    case 1:
                        BuyPotion(characterClass, new SmallPotion(), 20);
                        break;

                    case 2:
                        BuyPotion(characterClass, new MediumPotion(), 50);
                        break;

                    case 3:
                        BuyPotion(characterClass, new LargePotion(), 100);
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

        private static void BuyPotion(IClass characterClass, IItem potion, int price)
        {
            if (characterClass.Gold >= price)
            {
                characterClass.Inventory.Add(potion);
                characterClass.Gold -= price;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Kupiłeś {potion.Name}!\n");
                Console.ResetColor();
            }
            else
            {
                Dialogues.NoGold();
            }
        }
    }
}
