using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class IronMine : Mine
    {
        public IronMine(int quantity, Point position, double qtyScale, IEnumerable<FocusableSprite> sprites)
            : base(quantity, position, qtyScale, sprites)
        { }

        public override PrimaryResources Resource => PrimaryResources.Iron;

        protected override Color DefaultFill => Colors.SteelBlue;

        protected override Color HoverFill => Colors.LightSteelBlue;
    }
}
