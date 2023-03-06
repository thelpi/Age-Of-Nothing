using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public class Forest : Sprite
    {
        public Forest(Rect rect) : base(rect.Width, rect.Height, () => new Rectangle())
        {
            Surface = rect;

            DefaultFill = CreateBrush(Brushes.Green);
            HoverFill = CreateBrush(Brushes.ForestGreen);

            RefreshVisual(false);
            RefreshPosition();
            Visual.SetValue(Panel.ZIndexProperty, 1);
        }

        public override Rect Surface { get; }

        protected override int IndexZ => 1;

        protected override Brush DefaultFill { get; }

        protected override Brush HoverFill { get; }

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
    }
}
