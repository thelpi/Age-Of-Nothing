using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Resources
{
    [Dimensions(80)]
    public class GoldMine : Resource
    {
        public override ResourceTypes ResourceType => ResourceTypes.Gold;

        public GoldMine(int quantity, Point position, Controller parent)
            : base(position, quantity, parent)
        { }
    }
}
