using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    [Dimensions(25)]
    [CraftTime(60)]
    [ResourcesCost(0, 0, 20)]
    [LifePoints(2000)]
    public class Wall : Structure
    {
        public Wall(Point topLeft, IEnumerable<Sprite> sprites)
            : base(topLeft, sprites, true)
        { }
    }
}
