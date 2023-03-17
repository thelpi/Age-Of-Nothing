using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Dimensions(Size)]
    [UnitsStorage(5)]
    public class Market : Structure
    {
        private const int Size = 128;

        public Market(Point topLeft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topLeft, new Point(topLeft.X + Size, topLeft.Y + Size)), sprites)
        { }
    }
}
