using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Size(Size)]
    [CraftTime(200)]
    [Resources(0, 50, 10)]
    public class Dwelling : Structure
    {
        private const int Size = 50;

        public const int VillagerCapacity = 5;

        public Dwelling(Point topleft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topleft, new Point(topleft.X + Size, topleft.Y + Size)), sprites)
        { }

        protected override Color DefaultFill => Colors.SteelBlue;

        protected override Color HoverFill => Colors.LightSteelBlue;
    }
}
