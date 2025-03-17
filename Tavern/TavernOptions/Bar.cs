using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public class Bar
    {
        private const int WaterCost = 5;
        private const int WhiskyCost = 15;
        private const int SpecialDrinkCost = 150;
        private const int WaterExpGain = 5;
        private const int WhiskyExpGain = 5;
        private const int SpecialDrinkExpGain = 5000;
        private const int SpecialDrinkStatIncrease = 5;
        private const int MinLevelForWater = 5;
        private const int MinLevelForWhisky = 5;
        private const int MaxLevelForWhisky = 10;
        private const int MinLevelForSpecialDrink = 15;

        private bool specialDrink = true;

        public void BarOptions(IClass characterClass)
        {
            if (characterClass == null) return;

            bool continueBarOptions = true;
            while (continueBarOptions)
            {
                DisplayBarMenu();
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        Dialogues.ConvWithBarman(characterClass);
                        break;
                    case 2:
                        DrinkSmth(characterClass);
                        break;
                    case 3:
                        ShowGoods();
                        break;
                    case 4:
                        continueBarOptions = StandardFunctions.ExitRoom();
                        break;
                    default:
                        StandardFunctions.NoOption();
                        break;
                }
            }
        }

        private static void DisplayBarMenu()
        {
            Console.WriteLine("Co chcesz zrobić:");
            Console.WriteLine("1. Zapytaj barmana co słychać w okolicy.");
            Console.WriteLine("2. Napij się czegoś.");
            Console.WriteLine("3. Pokaż mi swoje towary.");
            Console.WriteLine("4. Odejdź od baru.");
        }

        private static void DrinkWater(IClass characterClass)
        {
            if (!HasEnoughGold(characterClass, WaterCost)) return;

            Console.WriteLine("Napiłeś się wody ze źródła.");
            Console.WriteLine("Czułeś jak Twoje ciało odzyskuje siły!");
            characterClass.Gold -= WaterCost;

            if (characterClass.Level < MinLevelForWater)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Masz za niski poziom, aby napić się wody ze źródła!");
                Console.ResetColor();
                return;
            }

            if (characterClass.Level < MinLevelForWhisky)
            {
                characterClass.Exp += WaterExpGain;
                characterClass.AddLevel();
                characterClass.UpdateStats();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Twoje punkty doświadczenia zwiększyły się o 5.");
                Console.WriteLine("Twoje statystyki zwiększyły się o 1.");
            }
            else
            {
                characterClass.UpdateStats();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Jesteś już na tak wysokim poziomie, że woda nie daje Ci już żadnego doświadczenia.");
                Console.WriteLine("Twoje statystyki zwiększyły się o 1.");
            }
            Console.ResetColor();
        }

        private static void DrinkWhisky(IClass characterClass)
        {
            if (!HasEnoughGold(characterClass, WhiskyCost)) return;

            characterClass.Gold -= WhiskyCost;

            if (characterClass.Level >= MinLevelForWhisky && characterClass.Level < MaxLevelForWhisky)
            {
                characterClass.Exp += WhiskyExpGain;
                characterClass.AddLevel();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Twoje punkty doświadczenia zwiększyły się o 5.");
            }
            else if (characterClass.Level >= MaxLevelForWhisky)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Masz za wysoki poziom!");
                Console.WriteLine("Po prostu napiłeś się whisky!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Masz za niski poziom aby wypić Whisky!!!");
            }
            Console.ResetColor();
        }

        private static bool HasEnoughGold(IClass characterClass, int cost)
        {
            if (characterClass.Gold < cost)
            {
                Dialogues.NoGold();
                return false;
            }
            return true;
        }

        private static void ShowGoods()
        {
            // To do + ekwipunek
        }

        private void DrinkSmth(IClass characterClass)
        {
            bool continueDrinking = true;
            while (continueDrinking)
            {
                DisplayDrinkMenu();
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        DrinkWater(characterClass);
                        break;
                    case 2:
                        DrinkWhisky(characterClass);
                        break;
                    case 3:
                        DrinkSmthSpecial(characterClass);
                        break;
                    case 4:
                        continueDrinking = StandardFunctions.ExitRoom();
                        break;
                    default:
                        StandardFunctions.NoOption();
                        break;
                }
            }
        }

        private static void DisplayDrinkMenu()
        {
            Console.WriteLine("Co chcesz zrobić:");
            Console.WriteLine("1: Zamów szklankę wody /5g");
            Console.WriteLine("2: Zamów szklankę dobrej whisky /15g ");
            Console.WriteLine("3. Zamów coś specjalnego /150g");
            Console.WriteLine("4. Zrezygnuj.");
        }

        private void DrinkSmthSpecial(IClass characterClass)
        {
            if (!specialDrink)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("To była jedyna sztuka, niestety.");
                Console.WriteLine("Nie wiem, czy kiedykolwiek dostanę podobny towar.");
                Console.ResetColor();
                return;
            }

            if (!HasEnoughGold(characterClass, SpecialDrinkCost)) return;

            if (characterClass.Level < MinLevelForSpecialDrink)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Jesteś zbyt słaby, by skosztować tej specjalności!");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Napij się specjalności prosto od krzyżaków z Marlborka!");
            Console.WriteLine("Miód pitny zwany Grunwald! Coś wspaniałego!");
            Console.WriteLine("Napiłeś się legendarnego miodu pitnego, który pochodzi ze stołów biesiadnych przed bitwą pod Grunwaldem.");

            characterClass.Gold -= SpecialDrinkCost;
            characterClass.Exp += SpecialDrinkExpGain;
            characterClass.AddLevel();
            characterClass.UpdateStats();
            specialDrink = false;

            Console.WriteLine("Twoje punkty doświadczenia zwiększyły się o 5000.");
            IncreaseCharacterStat(characterClass);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Twoja wybrana statystyka zwiększyła się o 5.");
            Console.ResetColor();
        }

        private static void IncreaseCharacterStat(IClass characterClass)
        {
            int statChoice = -1;
            while (statChoice < 1 || statChoice > 3)
            {
                Console.WriteLine("Wybierz, której statystyki chcesz zwiększyć:");
                Console.WriteLine("1: Siła");
                Console.WriteLine("2: Zręczność");
                Console.WriteLine("3: Żywotność");

                statChoice = StandardFunctions.ToInt32(Console.ReadLine());

                if (statChoice < 1 || statChoice > 3)
                {
                    Console.WriteLine("Nieprawidłowa wartość. Wybierz liczbę od 1 do 3.");
                }
            }

            switch (statChoice)
            {
                case 1:
                    characterClass.Str += SpecialDrinkStatIncrease;
                    break;
                case 2:
                    characterClass.Dex += SpecialDrinkStatIncrease;
                    break;
                case 3:
                    characterClass.Vit += SpecialDrinkStatIncrease;
                    break;
            }
        }
    }
}