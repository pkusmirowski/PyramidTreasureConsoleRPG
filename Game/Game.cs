using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public class Game
    {
        private bool End { get; set; }

        public void StartGame()
        {
            Dialogues.Intro();
            this.MainGame();
        }

        private void MainGame()
        {
            var game = new GameState();
            while (!this.End)
            {
                Console.WriteLine("1. Rozpocznij grę / Graj dalej.");
                Console.WriteLine("2. Wyjdź z gry.");
                int choice = StandardFunctions.ToInt32(Console.ReadLine());
                Console.Clear();
                switch (choice)
                {
                    case 1:
                        game.StartGame();
                        break;

                    case 2:
                        this.End = true;
                        break;
                }
            }
        }
    }
}