using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing
{
    public class Mine
    {
        public Mine(int value, Point position, double sizeRate)
        {
            Value = value;
            Position = position;
            Size = sizeRate * value;

            Visual = new Ellipse
            {
                Width = Size,
                Height = Size
            };
            Visual.MouseEnter += (a, b) => RefreshVisual(true);
            Visual.MouseLeave += (a, b) => RefreshVisual(false);
            RefreshVisual(false);

            Visual.SetValue(Canvas.LeftProperty, Position.X - (Size / 2));
            Visual.SetValue(Canvas.TopProperty, Position.Y - (Size / 2));
            Visual.SetValue(Panel.ZIndexProperty, 1);
        }

        public int Value { get; }

        public Point Position { get; }

        public double Size { get; }

        public Ellipse Visual { get; }

        public Rect Surface => new Rect(
            new Point(Position.X - Size / 2, Position.Y - Size / 2),
            new Point(Position.X + Size / 2, Position.Y + Size / 2));

        public void RefreshVisual(bool hover)
        {
            Visual.Fill = hover ? Brushes.DarkGray : Brushes.Gray;
        }
    }
}
