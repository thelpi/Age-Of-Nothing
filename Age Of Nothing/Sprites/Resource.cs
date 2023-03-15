using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Resource : FocusableSprite, ICenteredSprite
    {
        public Point Center { get; }

        public int Quantity { get; private set; }

        public abstract ResourceTypes ResourceType { get; }

        protected override string Info => $"{Quantity}";

        protected Resource(int quantity, Point center, double qtyScale, IEnumerable<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(qtyScale * quantity, qtyScale * quantity), () => new Ellipse(), 0.9, sprites, isCraft: false)
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
