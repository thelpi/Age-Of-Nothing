using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Resources
{
    [Dimensions(75)]
    public class GoldMine : Resource
    {
        public override ResourceTypes ResourceType => ResourceTypes.Gold;

        public GoldMine(int quantity, Point position, IEnumerable<Sprite> sprites)
            : base(position, quantity, sprites)
        { }
    }
}
