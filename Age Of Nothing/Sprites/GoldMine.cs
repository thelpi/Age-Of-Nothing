using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class GoldMine : Mine
    {
        public GoldMine(int quantity, Point position, double qtyScale, IEnumerable<FocusableSprite> sprites)
            : base(quantity, position, qtyScale, sprites)
        { }

        public override PrimaryResources Resource => PrimaryResources.Gold;

        protected override Color DefaultFill => Colors.Gold;

        protected override Color HoverFill => Colors.LightGoldenrodYellow;
    }
}
