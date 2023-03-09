using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class RockMine : Mine
    {
        public RockMine(int quantity, Point position, double qtyScale, IReadOnlyList<FocusableSprite> sprites)
            : base(quantity, position, qtyScale, sprites)
        { }

        public override PrimaryResources Resource => PrimaryResources.Rock;

        protected override Color DefaultFill => Colors.Silver;

        protected override Color HoverFill => Colors.Gainsboro;
    }
}
