using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Unit : FocusableSprite
    {
        private readonly LinkedList<Point> _targetCycle = new LinkedList<Point>();
        private LinkedListNode<Point> _targetPositionNode;
        private bool _loop;

        public Unit(Point position, double speed, double size, IReadOnlyList<FocusableSprite> sprites)
            : base(position, size, sprites, 2, true)
        {
            Speed = speed;
        }

        // pixels by frame
        public double Speed { get; }

        protected override Brush DefaultFill => Brushes.SandyBrown;

        protected override Brush HoverFill => Brushes.PeachPuff;

        protected override Brush FocusFill => new RadialGradientBrush(
            new GradientStopCollection(new List<GradientStop>
            {
                new GradientStop(Colors.SandyBrown, 0.75),
                new GradientStop(Colors.Red, 1)
            }));

        protected override Brush HoverFocusFill => new RadialGradientBrush(
            new GradientStopCollection(new List<GradientStop>
            {
                new GradientStop(Colors.PeachPuff, 0.75),
                new GradientStop(Colors.Red, 1)
            }));

        public bool CheckForMovement()
        {
            lock (_targetCycle)
            {
                if (_targetPositionNode == null)
                    return false;

                var tp = _targetPositionNode.Value;
                var (x2, y2) = MathTools.ComputePointOnLine(Position.X, Position.Y, tp.X, tp.Y, Speed);

                Position = new Point(x2, y2);
                if (tp == Position)
                {
                    _targetPositionNode = _targetPositionNode.Next;
                    if (_targetPositionNode == null && _loop)
                        _targetPositionNode = _targetCycle.First;
                }

                return true;
            }
        }

        public void SetCycle(params Point[] points)
        {
            lock (_targetCycle)
            {
                _targetCycle.Clear();
                var first = true;
                _loop = false;
                LinkedListNode<Point> node = null;
                foreach (var point in points)
                {
                    _loop = !first;
                    node = first
                        ? _targetCycle.AddFirst(point)
                        : _targetCycle.AddAfter(node, point);
                    first = false;
                }
                _targetPositionNode = _targetCycle.First;
            }
        }
    }
}
