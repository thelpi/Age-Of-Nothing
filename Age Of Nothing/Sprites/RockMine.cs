using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class RockMine : Resource
    {
        private const int _size = 75;

        public RockMine(int quantity, Point position, IEnumerable<FocusableSprite> sprites)
            : base(quantity, position, _size, sprites)
        { }

        public override ResourceTypes ResourceType => ResourceTypes.Rock;
    }
}
