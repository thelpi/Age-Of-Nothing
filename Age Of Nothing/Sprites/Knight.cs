using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Dimensions(20)]
    [CraftTime(240)]
    [Speed(8)]
    [LifePoints(80)]
    [ResourcesCost(50, 0, 0)]
    public class Knight : Unit
    {
        public Knight(Point center, IEnumerable<FocusableSprite> sprites)
            : base(center, GetSpriteSize<Knight>(), sprites)
        { }
    }
}
