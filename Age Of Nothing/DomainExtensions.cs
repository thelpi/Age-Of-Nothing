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
            return surface.GetIntangibleStructureIntersections(sprites).Any();
        }

        public static IEnumerable<Sprites.Sprite> GetIntangibleStructureIntersections(this Rect surface, IEnumerable<Sprites.Sprite> sprites)
        {
            return sprites.Where(x => x.Is<Sprites.Structures.Structure>(out var structure)
                && structure.Tangible
                && structure.Surface.IntersectsWith(surface));
        }

        public static Directions GetOppositeDirection(this Directions direction)
        {
            return (Directions)System.Math.Abs((int)direction - 2);
        }

        public static bool IsOppositeDirection(this Directions direction, Directions otherDirection)
        {
            return direction.GetOppositeDirection() == otherDirection;
        }
    }
}
