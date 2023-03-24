using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    [Dimensions(50)]
    [CraftTime(200)]
    [ResourcesCost(0, 50, 10)]
    [UnitsStorage(5)]
    [LifePoints(200)]
    public class Dwelling : Structure
    {
        public Dwelling(Point topleft, Controller parent)
            : base(topleft, parent, false)
        { }
    }
}
