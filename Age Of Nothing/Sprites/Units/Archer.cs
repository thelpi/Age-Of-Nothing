using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Units
{
    [Dimensions(20)]
    [CraftTime(200)]
    [Speed(5)]
    [LifePoints(40)]
    [ResourcesCost(20, 10, 0)]
    [CraftIn(typeof(Structures.Barracks))]
    public class Archer : Unit
    {
        public Archer(Point center, IEnumerable<Sprite> sprites)
            : base(center, sprites)
        { }
    }
}
