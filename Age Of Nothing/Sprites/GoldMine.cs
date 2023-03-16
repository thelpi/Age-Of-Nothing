using System.Collections.Generic;
using System.Windows;

namespace Age_Of_Nothing.Sprites
{
    public class GoldMine : Resource
    {
        private const int _size = 75;

        public override ResourceTypes ResourceType => ResourceTypes.Gold;

        public GoldMine(int quantity, Point position, IEnumerable<FocusableSprite> sprites)
            : base(quantity, position, _size, sprites)
        { }
    }
}
