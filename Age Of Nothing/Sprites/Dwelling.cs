using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Dwelling : Structure
    {
        private const double _size = 50;

        public const int VillagerCapacity = 5;

        public Dwelling(Point topleft)
            : base(new Rect(topleft, new Point(topleft.X + _size, topleft.Y + _size)))
        { }

        protected override Brush DefaultFill => Brushes.Yellow;

        protected override Brush HoverFill => Brushes.LightYellow;
    }
}
