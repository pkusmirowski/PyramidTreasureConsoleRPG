using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class GameStatus
    {
        public static bool CheckGameStatus(IClass characterClass)
        {
            if (characterClass.Level < 5)
            {
                if (characterClass.GameStatus == 0)
                {
                    TravelInfo();
                    return false;
                }
            }

            if (characterClass.Level >= 5 && characterClass.Level < 10)
            {
                if (characterClass.GameStatus == 1)
                {
                    TravelInfo();
                    return false;
                }
            }

            if (characterClass.Level >= 10 && characterClass.Level < 20)
            {
                if (characterClass.GameStatus == 2)
                {
                    TravelInfo();
                    return false;
                }
            }

            if (characterClass.Level == 20)
            {
                if (characterClass.GameStatus == 3)
                {
                    TravelInfo();
                    return false;
                }
            }

            return true;
        }

        private static void TravelInfo()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Przed wyruszeniem w podróż pogadaj z barmanem w tawernie!");
            Console.ResetColor();
        }
    }
}
