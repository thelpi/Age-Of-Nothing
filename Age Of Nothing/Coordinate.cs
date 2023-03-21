using System.Windows;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class Coordinate
    {
        public Sprite TargetSprite { get; }

        public Cardinals? CurrentCardinal { get; set; }

        public Point TargetPoint { get; }

        public Coordinate(Sprite targetSprite)
        {
            TargetPoint = targetSprite.Center;
            TargetSprite = targetSprite;
        }

        public Coordinate(Point targetPoint)
        {
            TargetPoint = targetPoint;
            TargetSprite = null;
        }
    }
}
