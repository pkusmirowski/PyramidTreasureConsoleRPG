using System;

namespace GreatPyramidTreasureConsoleRPG
{

    public static class Tavern
    {
        public static void TavernOptions(IClass characterClass)
        {
            bool value = true;
            var bar = new Bar();
            while (value)
            {
                Console.WriteLine("Co chcesz zrobić:");
                Console.WriteLine("1: Podejdź do baru.");
                Console.WriteLine("2: Podejdź do kasyna.");
                Console.WriteLine("3. Zapytaj się o pokój. /Możliwość ulczenia się");
                Console.WriteLine("4. Wyjdź z tawerny.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        bar.BarOptions(characterClass);
                        break;

                    case 2:
                        Casino.CasinoOptions(characterClass);
                        break;

                    case 3:
                        Rest.RestOptions(characterClass);
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
}