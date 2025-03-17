using NAudio.Wave;
using System.IO;
using System;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Program
    {
        private static readonly string MusicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "ancient_egypt.wav");
        private static WaveOutEvent _outputDevice;
        private static AudioFileReader _audioFile;

        public static void Main()
        {
            // When the program exits, clean up audio
            AppDomain.CurrentDomain.ProcessExit += OnProgramExit;

            PlayMusicInBackground(MusicPath);

            var game = new Game();
            game.StartGame();
        }

        private static void OnProgramExit(object sender, EventArgs e)
        {
            DisposeAudio();
        }


        private static void PlayMusicInBackground(string musicPath)
        {
            try
            {
                _audioFile = new AudioFileReader(musicPath);
                _outputDevice = new WaveOutEvent();
                _outputDevice.Init(_audioFile);
                _outputDevice.Volume = 0.5f;
                _outputDevice.Play();

                _outputDevice.PlaybackStopped += (sender, e) =>
                {
                    _audioFile.Position = 0;
                    _outputDevice.Play();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing background music: {ex.Message}");
            }
        }

        private static void DisposeAudio()
        {
            if (_outputDevice != null)
            {
                _outputDevice.Stop();
                _outputDevice.Dispose();
                _outputDevice = null;
            }

            if (_audioFile != null)
            {
                _audioFile.Dispose();
                _audioFile = null;
            }
        }
    }
}
