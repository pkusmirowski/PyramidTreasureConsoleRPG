using System;
using System.Collections.Generic;
using System.Linq;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class CollectionExtension
    {
        public static T GetRandomElement<T>(this ICollection<T> collection)
        {
            Random rng = new();
            return collection != null ? collection.ElementAt(rng.Next(collection.Count)) : default;
        }
    }
}
