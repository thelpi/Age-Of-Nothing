using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public class Forest : Sprite, IResourceSprite
    {
        const int WoodDensity = 1;

        private Rect _usefulSurface;

        public Forest(Rect rect)
            : base(rect, () => new Rectangle())
        {
            _usefulSurface = Surface;
        }

        public int Quantity => (int)Math.Floor(_usefulSurface.Width * _usefulSurface.Height * WoodDensity);

        public PrimaryResources Resource => PrimaryResources.Wood;

        protected override Brush DefaultFill => CreateBrush(Brushes.Green);

        protected override Brush HoverFill => CreateBrush(Brushes.ForestGreen);

        private DrawingBrush CreateBrush(Brush singleBrush)
        {
            var checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(
                new GeometryDrawing(
                    singleBrush,
                    null,
                    new EllipseGeometry(
                        new Rect(0, 0, Surface.Width, Surface.Height))));

            return new DrawingBrush
            {
                Drawing = checkersDrawingGroup,
                Viewport = new Rect(0, 0, 1 / (Surface.Width / 10), 1 / (Surface.Height / 10)),
                TileMode = TileMode.Tile
            };
        }
    }
}
