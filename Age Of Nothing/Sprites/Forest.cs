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

        protected override string Info => $"{Quantity}";

        protected override Color DefaultFill => Colors.Green;

        protected override Color HoverFill => Colors.ForestGreen;

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

        private DrawingBrush CreateBrush(Color singleColor)
        {
            var checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(
                new GeometryDrawing(
                    new SolidColorBrush(singleColor),
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

        public override void RefreshVisual(bool hover)
        {
            GetVisual().Fill = hover
                ? CreateBrush(HoverFill)
                : CreateBrush(DefaultFill);
            RefreshToolTip();
        }
    }
}
