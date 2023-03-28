using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Units
{
    [Dimensions(10)]
    [CraftTime(400)]
    [Speed(3)]
    [LifePoints(15)]
    [ResourcesCost(20, 0, 0)]
    [CraftIn(typeof(Structures.Monastery))]
    public class Monk : Unit
    {
        public Monk(Point center, Controller parent, int team)
            : base(center, parent, team)
        { }
    }
}
