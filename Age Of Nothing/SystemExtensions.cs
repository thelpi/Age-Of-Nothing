using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Age_Of_Nothing
{
    public static class SystemExtensions
    {
        public static bool FirstIfNotNull<T>(this IEnumerable<T> collection, Func<T, bool> predicate, out T item)
            where T : class
        {
            item = collection.FirstOrDefault(predicate);
            return item != null;
        }

        public static bool FirstIfNotNull<T>(this IEnumerable<T> sprites, Point position, out T sprite)
            where T : Sprites.Sprite
        {
            return sprites.FirstIfNotNull(x => x.Surface.Contains(position), out sprite);
        }

        public static T GetClosestSprite<T>(this IEnumerable<T> collection, Point position)
            where T : Sprites.ICenteredSprite
        {
            return collection.OrderBy(x => Point.Subtract(position, x.Center).LengthSquared).First();
        }
    }
}
