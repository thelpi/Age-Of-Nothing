using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Dwelling : Structure
    {
        public const double Size = 50;

        public const int VillagerCapacity = 5;

        public Dwelling(Point topleft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topleft, new Point(topleft.X + Size, topleft.Y + Size)), sprites)
        { }

        protected override Color DefaultFill => Colors.SteelBlue;

        protected override Color HoverFill => Colors.LightSteelBlue;
    }
}
