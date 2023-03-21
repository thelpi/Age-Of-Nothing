using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Age_Of_Nothing
{
    public static class DomainExtensions
    {
        public static bool FirstIfNotNull<T>(this IEnumerable<T> sprites, Point position, out T sprite)
            where T : Sprites.Sprite
        {
            return sprites.FirstIfNotNull(x => x.Surface.Contains(position), out sprite);
        }

        public static T GetClosestSprite<T>(this IEnumerable<T> collection, Point position)
            where T : Sprites.Sprite
        {
            return collection.OrderBy(x => Point.Subtract(position, x.Center).LengthSquared).First();
        }

        public static bool IntersectIntangibleStructure(this Rect surface, IEnumerable<Sprites.Sprite> sprites)
        {
            return sprites.Any(x => x.Is<Sprites.Structures.Structure>(out var structure)
                && structure.Tangible
                && structure.Surface.IntersectsWith(surface));
        }
    }
}
