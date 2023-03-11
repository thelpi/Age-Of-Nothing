using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    [Size(Size)]
    public class Dwelling : Structure
    {
        const int Size = 50;

        public const int VillagerCapacity = 5;

        public Dwelling(Point topleft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topleft, new Point(topleft.X + Size, topleft.Y + Size)), sprites)
        { }

        protected override Color DefaultFill => Colors.SteelBlue;

        protected override Color HoverFill => Colors.LightSteelBlue;
    }
}
