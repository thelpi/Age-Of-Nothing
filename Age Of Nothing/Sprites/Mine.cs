using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Mine : CenteredSprite
    {
        protected Mine(int quantity, Point position, double qtyScale, IReadOnlyList<CenteredSprite> sprites)
            : base(position, qtyScale * quantity, sprites)
        {
            Quantity = quantity;
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
