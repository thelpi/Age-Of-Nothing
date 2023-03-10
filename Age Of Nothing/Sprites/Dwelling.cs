using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Dwelling : Structure
    {
        private const double _size = 50;

        public const int VillagerCapacity = 5;

        public Dwelling(Point topleft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topleft, new Point(topleft.X + _size, topleft.Y + _size)), sprites)
        { }

        protected override Color DefaultFill => Colors.Yellow;

        protected override Color HoverFill => Colors.LightYellow;
    }
}
