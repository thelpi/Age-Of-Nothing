using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Mine : FocusableSprite, ICenteredSprite, IResourceSprite
    {
        public Point Center { get; }

        public int Quantity { get; private set; }

        public abstract PrimaryResources Resource { get; }

        protected override string Info => $"{Quantity}";

        protected Mine(int quantity, Point center, double qtyScale, IReadOnlyList<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(qtyScale * quantity, qtyScale * quantity), () => new Ellipse(), 0.9, sprites)
        {
            Quantity = quantity;
            Center = center;
        }

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
    }
}
