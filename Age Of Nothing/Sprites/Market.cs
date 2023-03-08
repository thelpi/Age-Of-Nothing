using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Market : Structure, ICenteredSprite
    {
        public Point Center { get; protected set; }

        private const int _size = 128;

        public Market(Point topLeft)
            : base(new Rect(topLeft, new Point(topLeft.X + _size, topLeft.Y + _size)))
        {
            Center = new Point(
                Surface.Left + Surface.Width / 2,
                Surface.Top + Surface.Height / 2);
        }

        protected override Brush DefaultFill => Brushes.Purple;

        protected override Brush HoverFill => Brushes.MediumPurple;
    }
}
