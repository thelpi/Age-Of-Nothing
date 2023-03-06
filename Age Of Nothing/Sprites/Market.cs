using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Market : Structure
    {
        public Point Position { get; }

        public Market(Rect rect) : base(rect)
        {
            Position = new Point(
                rect.Left + rect.Width / 2,
                rect.Top + rect.Height / 2);
        }

        protected override Brush DefaultFill => Brushes.Purple;

        protected override Brush HoverFill => Brushes.MediumPurple;
    }
}
