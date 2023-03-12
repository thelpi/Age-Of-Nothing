using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public class Forest : FocusableSprite, ICenteredSprite, IResourceSprite
    {
        private const int _size = 30;
        private const int _quantity = 100;

        public Point Center { get; }

        public int Quantity { get; private set; }

        public PrimaryResources Resource => PrimaryResources.Wood;

        protected override string Info => $"{Quantity}";

        public Forest(Point center, IEnumerable<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(_size, _size), () => new Ellipse(), 0.9, sprites, isCraft: false)
        {
            Quantity = _quantity;
            Center = center;
        }

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

        public static IEnumerable<Forest> GenerateForestRectangle(Rect rect, IEnumerable<FocusableSprite> sprites)
        {
            for (var i = rect.X; i < rect.BottomRight.X; i += _size)
            {
                for (var j = rect.Y; j < rect.BottomRight.Y; j += _size)
                {
                    yield return new Forest(new Point(i + _size / (double)2, j + _size / (double)2), sprites);
                }
            }
        }
    }
}
