using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Mine : FocusableSprite, ICenteredSprite
    {
        public Point Center { get; }

        protected Mine(int quantity, Point center, double qtyScale, IReadOnlyList<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(qtyScale * quantity, qtyScale * quantity), () => new Ellipse(), sprites)
        {
            Quantity = quantity;
            Center = center;
        }

        public int Quantity { get; }

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
