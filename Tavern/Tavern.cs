using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Tavern
    {
        public static void TavernOptions(IClass characterClass)
        {
            var bar = new Bar();
            while (true)
            {
                Console.WriteLine("Witaj w tawernie! Co chcesz zrobić?");
                Console.WriteLine("1. Podejdź do baru i porozmawiaj z barmanem.");
                Console.WriteLine("2. Podejdź do kasyna i spróbuj szczęścia w grach hazardowych.");
                Console.WriteLine("3. Zapytaj się o pokój, żeby się przespać i zregenerować siły.");
                Console.WriteLine("4. Wyjdź z tawerny.");

                var choice = StandardFunctions.ToInt32(Console.ReadLine());
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
                        Console.WriteLine("Do zobaczenia!");
                        return;

                    default:
                        StandardFunctions.NoOption();
                        break;
                }
            }
        }
    }
}
