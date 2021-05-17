using System;

namespace GreatPyramidTreasureConsoleRPG
{

    public static class Rest
    {
        public static void RestOptions(IClass characterClass)
        {
            if (characterClass != null)
            {
                bool value = true;
                while (value)
                {
                    Console.WriteLine("Co chcesz zrobić:");
                    Console.WriteLine("1: Wynamij pokój i odpręż się /10g Leczy cię do pełna.");
                    Console.WriteLine("2: Pogadaj z pobliskimi dziewczynami.");
                    Console.WriteLine("3. Wyjdź z pomieszczenia.");
                    int choice = StandardFunctions.ToInt32(Console.ReadLine());
                    Console.Clear();
                    switch (choice)
                    {
                        case 1:
                            Rest.RestCharacter(characterClass);
                            break;

                        case 2:
                            Dialogues.TalkToTheGirls();
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

        private static void RestCharacter(IClass characterClass)
        {
            if (characterClass.Hp == characterClass.MaxHP)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Masz maksymalną liczbe pkt zdrowia: {characterClass.Hp}");
                Console.ResetColor();
            }
            else if (characterClass.Gold < 10)
            {
                Dialogues.NoGold();
            }
            else
            {
                characterClass.Gold -= 10;
                characterClass.Hp = characterClass.MaxHP;
                Console.WriteLine("Po długiej nocy czujesz się wypoczęty ...");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Twoje punkty zdrowia {characterClass.Hp}");
                Console.WriteLine($"Twoje złoto {characterClass.Gold}");
                Console.ResetColor();
            }
        }
    }
}