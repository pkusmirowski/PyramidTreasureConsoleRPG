using System;
using System.Collections.Generic;

namespace GreatPyramidTreasureConsoleRPG
{


    public static class CollectionExtension
    {
        private static readonly Random Rng = new Random();

        public static T RandomElement<T>(this IList<T> list)
        {
            if (list != null)
            {
                return list[Rng.Next(list.Count)];
            }
            else
            {
                return default;
            }
        }

        public static T RandomElement<T>(this T[] array)
        {
            if (array != null)
            {
                return array[Rng.Next(array.Length)];
            }
            else
            {
                return default;
            }
        }
    }
}