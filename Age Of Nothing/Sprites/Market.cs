using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [CraftTime(400)]
    [ResourcesCost(10, 200, 50)]
    [Dimensions(128)]
    [UnitsStorage(5)]
    [LifePoints(1000)]
    public class Market : Structure
    {
        public Market(Point topLeft, IEnumerable<FocusableSprite> sprites)
            : base(topLeft, sprites)
        { }
    }
}
