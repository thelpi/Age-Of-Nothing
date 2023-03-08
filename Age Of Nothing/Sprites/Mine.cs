using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Mine : FocusableSprite, ICenteredSprite, IResourceSprite
    {
        public Point Center { get; }

        public int Quantity { get; private set; }

        public abstract PrimaryResources Resource { get; }

        protected Mine(int quantity, Point center, double qtyScale, IReadOnlyList<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(qtyScale * quantity, qtyScale * quantity), () => new Ellipse(), sprites)
        {
            Quantity = quantity;
            Center = center;
        }

        protected Brush GetHoverBrush(Color color)
        {
            return new RadialGradientBrush(
                new GradientStopCollection(new List<GradientStop>
                {
                    new GradientStop(color, 0.9),
                    new GradientStop(Colors.Red, 1)
                }));
        }
    }
}
