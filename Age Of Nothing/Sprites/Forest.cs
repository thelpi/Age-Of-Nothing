using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public class Forest : Sprite, IResourceSprite
    {
        public Forest(Rect rect)
            : base(rect, () => new Rectangle())
        {
            Quantity = (int)(Math.Floor(rect.Width * rect.Height) / 10);
        }

        public int Quantity { get; private set; }

        public PrimaryResources Resource => PrimaryResources.Wood;

        protected override Brush DefaultFill => CreateBrush(Brushes.Green);

        protected override Brush HoverFill => CreateBrush(Brushes.ForestGreen);

        protected override string Info => $"{Quantity}";

        public int ReduceQuantity(int qtyLost)
        {
            if (qtyLost > Quantity)
            {
                var qtyReturned = Quantity;
                Quantity = 0;
                return qtyReturned;
            }
            else
            {
                Quantity -= qtyLost;
                return qtyLost;
            }
        }

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
