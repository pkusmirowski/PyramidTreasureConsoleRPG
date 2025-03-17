using System;
using System.Globalization;
using System.Media;
using System.Threading;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class StandardFunctions
    {
        private static readonly ThreadLocal<Random> RandomDmg = new(() => new Random());

        /// <summary>
        /// Displays a message indicating no option is available.
        /// </summary>
        public static void NoOption()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Brak opcji.");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays the default option message.
        /// </summary>
        public static void DefaultOption() => Console.WriteLine("Domyślnie opcja 1.");

        /// <summary>
        /// Displays a message indicating the user is exiting the room.
        /// </summary>
        /// <returns>Always returns false.</returns>
        public static bool ExitRoom()
        {
            Console.WriteLine("Wychodzisz ...");
            return false;
        }

        /// <summary>
        /// Generates a random damage value between the specified minimum and maximum values.
        /// </summary>
        /// <param name="minDmg">The minimum damage value.</param>
        /// <param name="maxDmg">The maximum damage value.</param>
        /// <returns>A random damage value.</returns>
        public static int RandDmg(int minDmg, int maxDmg)
        {
            return RandomDmg.Value.Next(minDmg, maxDmg + 1);
        }

        /// <summary>
        /// Converts a string to an integer. If the conversion fails, it displays an error message and returns 0.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The converted integer value, or 0 if the conversion fails.</returns>
        public static int ToInt32(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            try
            {
                return int.Parse(value, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wprowadzono zły znak!");
                Console.ResetColor();
                Thread.Sleep(700);
                return 0;
            }
        }

        /// <summary>
        /// Plays a sound from the specified directory.
        /// </summary>
        /// <param name="dir">The directory of the sound file.</param>
        public static void SoundPlayer(string dir)
        {
            var player = new SoundPlayer($"{AppDomain.CurrentDomain.BaseDirectory}{dir}");
            player.Play();
        }

        /// <summary>
        /// Pauses the execution for a specified duration.
        /// </summary>
        public static void Sleep()
        {
            Thread.Sleep(4000);
        }
    }
}
