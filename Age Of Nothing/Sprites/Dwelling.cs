using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Dimensions(Size)]
    [CraftTime(200)]
    [ResourcesCost(0, 50, 10)]
    [UnitsStorage(5)]
    public class Dwelling : Structure
    {
        private const int Size = 50;

        public Dwelling(Point topleft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topleft, new Point(topleft.X + Size, topleft.Y + Size)), sprites)
        { }
    }
}
