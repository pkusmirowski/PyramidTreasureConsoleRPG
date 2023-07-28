using NAudio.Wave;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Program
    {
        private static readonly string MusicPath = $"{System.IO.Directory.GetCurrentDirectory()}\\Audio\\ancient_egypt.wav";


        public static void Main()
        {
            PlayMusicInBackground(MusicPath);

            var game = new Game();
            game.StartGame();
        }

        private static void PlayMusicInBackground(string musicPath)
        {
            var reader = new AudioFileReader(musicPath);
            var output = new WaveOutEvent();
            output.Init(reader);
            output.Play();
            output.Volume = 0.5f;

            // loop the music in the background
            output.PlaybackStopped += (sender, e) =>
            {
                reader.Position = 0;
                output.Play();
            };
        }
    }
}
