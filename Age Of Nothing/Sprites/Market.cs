using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Market : Structure
    {
        public Market(Rect rect) : base(rect)
        { }

        protected override Brush DefaultFill => Brushes.Purple;

        protected override Brush HoverFill => Brushes.MediumPurple;
    }
}
