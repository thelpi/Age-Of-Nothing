using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Dimensions(20)]
    [CraftTime(200)]
    [Speed(5)]
    [LifePoints(40)]
    [ResourcesCost(20, 10, 0)]
    public class Archer : Unit
    {
        public Archer(Point center, IEnumerable<FocusableSprite> sprites)
            : base(center, GetSpriteSize<Archer>(), sprites, GetLifePoints<Archer>())
        { }
    }
}
