using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    [Dimensions(100)]
    [CraftTime(400)]
    [ResourcesCost(20, 100, 20)]
    [LifePoints(1000)]
    public class Barracks : Structure
    {
        public Barracks(Point topleft, Controller parent)
            : base(topleft, parent, false)
        { }
    }
}
