using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing
{
    public class Forest
    {
        public Forest(Rect rect)
        {
            Surface = rect;
            Visual = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height
            };
            Visual.MouseEnter += (a, b) => RefreshVisual(true);
            Visual.MouseLeave += (a, b) => RefreshVisual(false);

            DefaultBrush = CreateBrush(Brushes.Green);
            HoverBrush = CreateBrush(Brushes.ForestGreen);

            RefreshVisual(false);
            RefreshPosition();
            Visual.SetValue(Panel.ZIndexProperty, 1);
        }

        private DrawingBrush CreateBrush(Brush singleBrush)
        {
            var myBrush = new DrawingBrush();

            var backgroundSquare = new GeometryDrawing(
                singleBrush, null, new EllipseGeometry(new Rect(0, 0, Surface.Width, Surface.Height)));

            var checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);

            myBrush.Drawing = checkersDrawingGroup;
            myBrush.Viewport = new Rect(0, 0, 1 / (Surface.Width / 10), 1 / (Surface.Height / 10));
            myBrush.TileMode = TileMode.Tile;
            return myBrush;
        }

        public Rect Surface { get; }
        public Rectangle Visual { get; }

        private Brush DefaultBrush { get; }
        private Brush HoverBrush { get; }

        public void RefreshVisual(bool hover)
        {
            Visual.Fill = hover ? HoverBrush : DefaultBrush;
        }

        public void RefreshPosition()
        {
            Visual.SetValue(Canvas.LeftProperty, Surface.Left);
            Visual.SetValue(Canvas.TopProperty, Surface.Top);
        }
    }
}
