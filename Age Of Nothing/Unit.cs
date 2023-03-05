using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing
{
    public class Unit : CenteredSprite
    {
        public Unit(Point position, double speed, double size, IReadOnlyList<CenteredSprite> sprites)
            : base(position, size, sprites)
        {
            Speed = speed;
        }

        // pixels by frame
        public double Speed { get; }

        public Point? TargetPosition { get; set; }

        protected override int IndexZ => 2;

        protected override Brush DefaultFill => Brushes.Blue;

        protected override Brush HoverFill => Brushes.CornflowerBlue;

        protected override Brush FocusFill => new RadialGradientBrush(Colors.Blue, Colors.Red);

        protected override Brush HoverFocusFill => new RadialGradientBrush(Colors.CornflowerBlue, Colors.Red);

        public bool CheckForMovement()
        {
            if (!TargetPosition.HasValue)
                return false;

            var (x2, y2) = MathTools.ComputePointOnLine(Position.X, Position.Y,
                TargetPosition.Value.X, TargetPosition.Value.Y, Speed);

            Position = new Point(x2, y2);
            if (TargetPosition == Position)
                TargetPosition = null;

            return true;
        }
    }
}
