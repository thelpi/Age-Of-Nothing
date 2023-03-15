﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Unit : FocusableSprite, ICenteredSprite
    {
        private Point _center;
        private readonly LinkedList<(Point pt, Sprite tgt)> _targetCycle = new LinkedList<(Point, Sprite)>();
        private LinkedListNode<(Point pt, Sprite tgt)> _targetPositionNode;
        private bool _loop;

        protected Unit(Point center, double speed, double size, IEnumerable<FocusableSprite> sprites)
            : base(center.ComputeSurfaceFromMiddlePoint(size, size), sprites, true)
        {
            Speed = speed;
            _center = center;
        }

        // pixels by frame
        public double Speed { get; }

        public Point Center
        {
            get => _center;
            protected set
            {
                _center = value;
                Move(Center.ComputeSurfaceFromMiddlePoint(Surface.Width, Surface.Height).TopLeft);
            }
        }

        public Sprite CheckForMovement()
        {
            lock (_targetCycle)
            {
                if (_targetPositionNode == null)
                    return null;

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

                NotifyMove();
                return tgt;
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

        public bool FocusedOn(Sprite sprite)
        {
            return _targetCycle.Any(tc => tc.tgt == sprite);
        }

        public T FocusedOn<T>() where T : Sprite
        {
            return _targetCycle.FirstOrDefault(tc => tc.tgt.Is<T>()).tgt as T;
        }
    }
}
