using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class IronMine : Mine
    {
        public IronMine(int quantity, Point position, double qtyScale, IReadOnlyList<CenteredSprite> sprites)
            : base(quantity, position, qtyScale, sprites)
        { }

        protected override Brush DefaultFill => new SolidColorBrush(Colors.SteelBlue);

        protected override Brush HoverFill => new SolidColorBrush(Colors.LightSteelBlue);

        protected override Brush FocusFill => GetHoverBrush(Colors.SteelBlue);

        protected override Brush HoverFocusFill => GetHoverBrush(Colors.LightSteelBlue);
    }
}
