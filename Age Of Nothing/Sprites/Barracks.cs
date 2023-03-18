using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Dimensions(96)]
    [CraftTime(400)]
    [ResourcesCost(20, 100, 20)]
    [LifePoints(1000)]
    public class Barracks : Structure
    {
        public Barracks(Point topleft, IEnumerable<FocusableSprite> sprites)
            : base(new Rect(topleft, new Point(topleft.X + GetSpriteSize<Barracks>().Width, topleft.Y + GetSpriteSize<Barracks>().Height)), sprites)
        { }
    }
}
