using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Size(Size)]
    public class Market : Structure, ICenteredSprite
    {
        public Point Center { get; protected set; }

        const int Size = 128;

        public Market(Point topLeft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topLeft, new Point(topLeft.X + Size, topLeft.Y + Size)), sprites)
        {
            Center = new Point(
                Surface.Left + Surface.Width / 2,
                Surface.Top + Surface.Height / 2);
        }

        protected override Color DefaultFill => Colors.Purple;

        protected override Color HoverFill => Colors.MediumPurple;
    }
}
