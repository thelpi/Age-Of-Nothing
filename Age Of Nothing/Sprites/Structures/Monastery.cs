using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    [Dimensions(80)]
    [CraftTime(300)]
    [ResourcesCost(50, 50, 50)]
    [UnitsStorage(5)]
    [LifePoints(800)]
    public class Monastery : Structure
    {
        public Monastery(Point topLeft, Controller parent, int team)
            : base(topLeft, parent, false, team)
        { }
    }
}
