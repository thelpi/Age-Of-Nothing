using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Market : Structure, ICenteredSprite
    {
        public Point Center { get; protected set; }

        public Market(Rect rect)
            : base(rect)
        {
            Center = new Point(
                rect.Left + rect.Width / 2,
                rect.Top + rect.Height / 2);
        }

        protected override Brush DefaultFill => Brushes.Purple;

        protected override Brush HoverFill => Brushes.MediumPurple;
    }
}
