using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class RockMine : Resource
    {
        public RockMine(int quantity, Point position, double qtyScale, IEnumerable<FocusableSprite> sprites)
            : base(quantity, position, qtyScale, sprites)
        { }

        public override ResourceTypes ResourceType => ResourceTypes.Rock;

        protected override Color DefaultFill => Colors.Silver;

        protected override Color HoverFill => Colors.Gainsboro;
    }
}
