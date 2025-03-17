using System;
using System.Collections.Generic;
using System.Linq;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class CollectionExtension
    {
        private static readonly Random _rng = new();

        public static T GetRandomElement<T>(this ICollection<T> collection)
        {
            if (collection == null || collection.Count == 0)
            {
                throw new ArgumentException("Collection cannot be null or empty.", nameof(collection));
            }

            lock (_rng)
            {
                return collection.ElementAt(_rng.Next(collection.Count));
            }
        }
    }
}
