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

        protected override Brush DefaultFill => Brushes.Gray;

        protected override Brush HoverFill => Brushes.DarkGray;

        protected override Brush FocusFill => Brushes.SlateGray;

        protected override Brush HoverFocusFill => Brushes.DarkSlateGray;
    }
}
