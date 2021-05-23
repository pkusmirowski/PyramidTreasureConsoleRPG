namespace GreatPyramidTreasureConsoleRPG
{
    public static class Program
    {
        public static void Main()
        {
            const string dir = "\\Audio\\ancient_egypt.wav";
            StandardFunctions.SoundPlayer(dir);
            Game game = new();
            game.StartGame();
        }
    }
}