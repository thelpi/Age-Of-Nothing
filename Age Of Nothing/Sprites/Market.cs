using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Dimensions(128)]
    [UnitsStorage(5)]
    public class Market : Structure
    {
        public Market(Point topLeft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topLeft, new Point(topLeft.X + GetSpriteSize<Market>().Width, topLeft.Y + GetSpriteSize<Market>().Height)), sprites)
        { }
    }
}
