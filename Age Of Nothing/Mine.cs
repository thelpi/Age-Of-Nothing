using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing
{
    public class Mine : CenteredSprite
    {
        public Mine(int quantity, Point position, double qtyScale, bool isIron, IReadOnlyList<CenteredSprite> sprites)
            : base(position, qtyScale * quantity, sprites)
        {
            Quantity = quantity;
            IsIron = isIron;

            // otherwise the Iron color does not apply
            RefreshVisual(false);
        }

        public bool IsIron { get; }

        public int Quantity { get; }

        protected override int IndexZ => 1;

        private readonly Color HoverRockFill = Colors.Gainsboro;
        private readonly Color DefaultRockFill = Colors.Silver;
        private readonly Color HoverIronFill = Colors.LightSteelBlue;
        private readonly Color DefaultIronFill = Colors.SteelBlue;

        protected override Brush DefaultFill => new SolidColorBrush(IsIron ? DefaultIronFill : DefaultRockFill);

        protected override Brush HoverFill => new SolidColorBrush(IsIron ? HoverIronFill : HoverRockFill);

        protected override Brush FocusFill => new RadialGradientBrush(
            new GradientStopCollection(new List<GradientStop>
            {
                new GradientStop(IsIron ? DefaultIronFill : DefaultRockFill, 0.9),
                new GradientStop(Colors.Red, 1)
            }));

        protected override Brush HoverFocusFill => new RadialGradientBrush(
            new GradientStopCollection(new List<GradientStop>
            {
                new GradientStop(IsIron ? HoverIronFill : HoverRockFill, 0.9),
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
