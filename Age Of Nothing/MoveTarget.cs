using System.Windows;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class MoveTarget
    {
        public Sprite TargetSprite { get; }

        public Directions? ForcedDirection { get; set; }

        public Point TargetPoint { get; }

        public MoveTarget(Sprite targetSprite)
        {
            TargetPoint = targetSprite.Center;
            TargetSprite = targetSprite;
        }

        public MoveTarget(Point targetPoint)
        {
            TargetPoint = targetPoint;
            TargetSprite = null;
        }
    }
}
