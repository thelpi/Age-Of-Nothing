using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Unit : FocusableSprite, ICenteredSprite
    {
        private Point _center;
        private readonly LinkedList<(Point pt, Sprite tgt)> _targetCycle = new LinkedList<(Point, Sprite)>();
        private LinkedListNode<(Point pt, Sprite tgt)> _targetPositionNode;
        private bool _loop;
        public const int BuildFramesCount = 120;

        protected Unit(Point center, double speed, double size, IEnumerable<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(size, size), () => new Ellipse(), 0.75, sprites, 2, true)
        {
            Speed = speed;
            _center = center;
        }

        // pixels by frame
        public double Speed { get; }

        protected override Color DefaultFill => Colors.SandyBrown;

        protected override Color HoverFill => Colors.PeachPuff;

        public Point Center
        {
            get => _center;
            protected set
            {
                _center = value;
                Move(Center.ComputeSurfaceFromMiddlePoint(Surface.Width, Surface.Height).TopLeft);
            }
        }

        public (bool move, Sprite tgt) CheckForMovement()
        {
            lock (_targetCycle)
            {
                if (_targetPositionNode == null)
                    return (false, null);

                var (pt, tgt) = _targetPositionNode.Value;
                var (x2, y2) = GeometryTools.ComputePointOnLine(Center.X, Center.Y, pt.X, pt.Y, Speed);

                Center = new Point(x2, y2);
                if (pt == Center)
                {
                    _targetPositionNode = _targetPositionNode.Next;
                    if (_targetPositionNode == null && _loop)
                        _targetPositionNode = _targetCycle.First;
                }
                else
                {
                    tgt = null;
                }

                return (true, tgt);
            }
        }

        public void SetCycle(params (Point, Sprite)[] points)
        {
            lock (_targetCycle)
            {
                _targetCycle.Clear();
                var first = true;
                _loop = false;
                LinkedListNode<(Point, Sprite)> node = null;
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
