using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing
{
    public class Mine : CenteredSprite
    {
        public Mine(int quantity, Point position, double qtyScale, IReadOnlyList<CenteredSprite> sprites)
            : base(position, qtyScale * quantity, sprites)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }

        protected override int IndexZ => 1;

        protected override Brush DefaultFill => Brushes.Silver;

        protected override Brush HoverFill => Brushes.Gainsboro;

        protected override Brush FocusFill => new RadialGradientBrush(
            new GradientStopCollection(new List<GradientStop>
            {
                new GradientStop(Colors.Silver, 0.9),
                new GradientStop(Colors.Red, 1)
            }));

        protected override Brush HoverFocusFill => new RadialGradientBrush(
            new GradientStopCollection(new List<GradientStop>
            {
                new GradientStop(Colors.Gainsboro, 0.9),
                new GradientStop(Colors.Red, 1)
            }));

        public override void RefreshVisual(bool hover)
        {
            Visual.Fill = hover
                ? (Focused
                    ? HoverFocusFill
                    : HoverFill)
                : (Focused
                    ? FocusFill
                    : DefaultFill);
        }
    }
}
