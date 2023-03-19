using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Units
{
    public abstract class Unit : Sprite
    {
        private readonly LinkedList<(Point point, Sprite target)> _pathCycle = new LinkedList<(Point, Sprite)>();
        private LinkedListNode<(Point point, Sprite target)> _currentPathTarget;
        private bool _isPathLoop;

        protected Unit(Point center, IEnumerable<Sprite> sprites)
            : base(center, true, true, sprites)
        { }

        /// <summary>
        /// Check if the unit has to move in this frame
        /// </summary>
        /// <returns>The sprite the unit is on to (if any; and if in the path cycle).</returns>
        public Sprite CheckForMovement(IEnumerable<Structures.Structure> progressingCrafts)
        {
            lock (_pathCycle)
            {
                // no target: no move
                if (_currentPathTarget == null)
                    return null;

                // compte the point on the distance to reach the targer
                // (in straight line)
                var (targetPoint, target) = _currentPathTarget.Value;
                var (x2, y2) = GeometryTools.ComputePointOnLine(Center.X, Center.Y, targetPoint.X, targetPoint.Y, GetDefaultSpeed());

                var newSurface = new Point(x2, y2).ComputeSurfaceFromMiddlePoint(Surface.Size);

                // TODO: allow villagers to help for crafting

                // HACK: when a unit is already on a tangible structure
                // everything is allowed to get out
                // the use case if when a villager finish to craft a wall (he's on the center)
                var currentlyOn = Surface.IntersectIntangibleStructure(Sprites.Concat(progressingCrafts));
                var nextOn = newSurface.IntersectIntangibleStructure(Sprites.Concat(progressingCrafts));
                if (!currentlyOn && nextOn)
                    return null;

                Move(newSurface);
                if (targetPoint == Center)
                {
                    // has reached the target
                    // if last node of the circle, sets loop if enabled
                    _currentPathTarget = _currentPathTarget.Next;
                    if (_currentPathTarget == null && _isPathLoop)
                        _currentPathTarget = _pathCycle.First;
                }
                else
                {
                    // not yet reaching the target
                    target = null;
                }

                return target;
            }
        }

        /// <summary>
        /// Sets a cycle path with the given points and targets
        /// </summary>
        /// <param name="points">
        /// Points can be given without target;
        /// note the other way around though
        /// even if we could compute the point related to target inside the method
        /// </param>
        public void SetPathCycle(params (Point, Sprite)[] points)
        {
            lock (_pathCycle)
            {
                _pathCycle.Clear();
                var first = true;
                _isPathLoop = false;
                LinkedListNode<(Point, Sprite)> node = null;
                foreach (var point in points)
                {
                    _isPathLoop = !first;
                    node = first
                        ? _pathCycle.AddFirst(point)
                        : _pathCycle.AddAfter(node, point);
                    first = false;
                }
                _currentPathTarget = _pathCycle.First;
            }
        }

        /// <summary>
        /// Does the path cycle include the specified sprite?
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public bool IsSpriteOnPath(Sprite sprite)
        {
            return _pathCycle.Any(tc => tc.target == sprite);
        }

        /// <summary>
        /// Gets the next sprite (of the specified subtype) on the cycle path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetNextSpriteOnPath<T>() where T : Sprite
        {
            // BUG: maybe it should be the next according to where the unit is currently are in the cycle
            // not necessarely the next from the start
            return _pathCycle.FirstOrDefault(tc => tc.target.Is<T>()).target as T;
        }

        /// <summary>
        /// Computes the path cycle from the specified point and targets related to point (if any)
        /// </summary>
        /// <param name="originalPoint"></param>
        /// <param name="targets"></param>
        /// <param name="inProgressCrafts"></param>
        public virtual void ComputeCycle(Point originalPoint, IEnumerable<Sprite> targets, IEnumerable<Craft> inProgressCrafts)
        {
            SetPathCycle((originalPoint, null));
        }

        private double GetDefaultSpeed()
        {
            return GetType().GetAttribute<SpeedAttribute>()?.PixelsByFrame ?? 0;
        }

        public static T Instanciate<T>(Point center, IEnumerable<Sprite> sprites) where T : Unit
        {
            return (T)typeof(T)
                .GetConstructor(new[] { typeof(Point), typeof(IEnumerable<Sprite>) })
                .Invoke(new object[] { center, sprites });
        }
    }
}
