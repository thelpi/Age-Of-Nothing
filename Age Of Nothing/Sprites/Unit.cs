using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public class Unit : FocusableSprite, ICenteredSprite
    {
        private Point _center;
        private readonly LinkedList<(Point pt, TargetType tgt)> _targetCycle = new LinkedList<(Point, TargetType)>();
        private LinkedListNode<(Point pt, TargetType tgt)> _targetPositionNode;
        private bool _loop;

        public Unit(Point center, double speed, double size, IReadOnlyList<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(size, size), () => new Ellipse(), sprites, 2, true)
        {
            Speed = speed;
            _center = center;
        }

        // pixels by frame
        public double Speed { get; }

        public (PrimaryResources r, int v)? Shipment { get; set; }

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

        public Point Center
        {
            get { return _center; }
            protected set
            {
                _center = value;
                Move(Center.ComputeSurfaceFromMiddlePoint(Surface.Width, Surface.Height).TopLeft);
            }
        }

        public (bool move, (PrimaryResources r, int v)? ship) CheckForMovement()
        {
            lock (_targetCycle)
            {
                if (_targetPositionNode == null)
                    return (false, null);

                var (pt, tgt) = _targetPositionNode.Value;
                var (x2, y2) = GeometryTools.ComputePointOnLine(Center.X, Center.Y, pt.X, pt.Y, Speed);

                Center = new Point(x2, y2);
                (PrimaryResources, int)? ship = null;
                if (pt == Center)
                {
                    _targetPositionNode = _targetPositionNode.Next;
                    if (_targetPositionNode == null && _loop)
                        _targetPositionNode = _targetCycle.First;
                    if (tgt == TargetType.Market)
                    {
                        ship = Shipment;
                        Shipment = null;
                    }
                    else
                    {
                        var pr = tgt.ToResource();
                        if (pr.HasValue)
                            Shipment = (pr.Value, 10);
                    }
                }

                return (true, ship);
            }
        }

        public void SetCycle(params (Point, TargetType)[] points)
        {
            lock (_targetCycle)
            {
                _targetCycle.Clear();
                var first = true;
                _loop = false;
                LinkedListNode<(Point, TargetType)> node = null;
                foreach (var point in points)
                {
                    _loop = !first;
                    node = first
                        ? _targetCycle.AddFirst(point)
                        : _targetCycle.AddAfter(node, point);
                    first = false;
                }
                _targetPositionNode = _targetCycle.First;
                // lazy: any change on path will reset the shipment...
                Shipment = null;
            }
        }
    }
}
