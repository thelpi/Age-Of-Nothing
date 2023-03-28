using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    [CraftTime(1000)]
    [ResourcesCost(20, 200, 400)]
    [Dimensions(150)]
    [UnitsStorage(20)]
    [LifePoints(5000)]
    public class Castle : Structure
    {
        public Castle(Point topLeft, Controller parent, int team)
            : base(topLeft, parent, true, team)
        { }
    }
}
