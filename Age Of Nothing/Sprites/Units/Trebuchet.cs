using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Units
{
    [Dimensions(20)]
    [CraftTime(500)]
    [Speed(2)]
    [LifePoints(100)]
    [ResourcesCost(100, 50, 0)]
    [CraftIn(typeof(Structures.Castle))]
    public class Trebuchet : Unit
    {
        public Trebuchet(Point center, Controller parent, int team)
            : base(center, parent, team)
        { }
    }
}
