using System;
using System.Globalization;
using System.Media;
using System.Threading;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class StandardFunctions
    {
        private static readonly Random RandomDmg = new();

        public static void NoOption()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Brak opcji.");
            Console.ResetColor();
        }

        public static void DefaultOption() => Console.WriteLine("Domyślnie opcja 1.");

        public static bool ExitRoom()
        {
            Console.WriteLine("Wychodzisz ...");
            return false;
        }

        public static int RandDmg(int minDmg, int maxDmg)
        {
            lock (RandomDmg)
            {
                return RandomDmg.Next(minDmg, maxDmg + 1);
            }
        }

        public static int ToInt32(string value)
        {
            if (value == null)
            {
                return 0;
            }

            bool flag;
            do
            {
                try
                {
                    return int.Parse(value, (IFormatProvider)CultureInfo.CurrentCulture);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wprowadzono zły znak!");
                    Console.ResetColor();
                    Thread.Sleep(700);
                    flag = false;
                }
            } while (flag);

            return 0;
        }

        public static void SoundPlayer(string dir)
        {
            var player = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + dir);
            player.Play();
        }

        public static void Sleep()
        {
            Thread.Sleep(4000);
        }
    }
}
