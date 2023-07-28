using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class GameStatus
    {
        public static bool CheckGameStatus(IClass characterClass)
        {
            if (characterClass.Level < 5 && characterClass.GameStatus == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Przed wyruszeniem w podróż pogadaj z barmanem w tawernie!");
                Console.ResetColor();
                return false;
            }
            else if (characterClass.Level >= 5 && characterClass.Level < 10 && characterClass.GameStatus == 1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Przed wyruszeniem w podróż pogadaj z barmanem w tawernie!");
                Console.ResetColor();
                return false;
            }
            else if (characterClass.Level >= 10 && characterClass.Level < 20 && characterClass.GameStatus == 2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Przed wyruszeniem w podróż pogadaj z barmanem w tawernie!");
                Console.ResetColor();
                return false;
            }
            else if (characterClass.Level == 20 && characterClass.GameStatus == 3)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Przed wyruszeniem w podróż pogadaj z barmanem w tawernie!");
                Console.ResetColor();
                return false;
            }

            return true;
        }
    }
}
