using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Market : Structure, ICenteredSprite
    {
        public Point Center { get; protected set; }

        private const int _size = 128;

        public Market(Point topLeft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topLeft, new Point(topLeft.X + _size, topLeft.Y + _size)), sprites)
        {
            Center = new Point(
                Surface.Left + Surface.Width / 2,
                Surface.Top + Surface.Height / 2);
        }

        protected override Color DefaultFill => Colors.Purple;

        protected override Color HoverFill => Colors.MediumPurple;
    }
}
