using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Units
{
    public abstract class Unit : Sprite
    {
        private readonly LinkedList<MoveTarget> _pathCycle = new LinkedList<MoveTarget>();
        private LinkedListNode<MoveTarget> _currentPathTarget;
        private bool _isPathLoop;

        protected Unit(Point center, IEnumerable<Sprite> sprites)
            : base(center, true, true, sprites)
        { }

        /// <summary>
        /// Check if the unit has to move in this frame
        /// </summary>
        /// <returns>The destination sprite, if the destination has been reached on this frame.</returns>
        public Sprite CheckForMovement(IEnumerable<Structures.Structure> progressingCrafts)
        {
            lock (_pathCycle)
            {
                // no target: no move
                if (_currentPathTarget == null)
                    return null;

                var currentTargetPoint = _currentPathTarget.Value.TargetPoint;
                var currentTargetSprite = _currentPathTarget.Value.TargetSprite;
                var currentForcedDirection = _currentPathTarget.Value.ForcedDirection;

                // compute the point on the distance to reach the targer
                // (in straight line)
                var newPoint = GeometryTools.ComputePointOnLine(
                    Center.X, Center.Y,
                    currentTargetPoint.X,
                    currentTargetPoint.Y, GetDefaultSpeed());

                // the surface of the sprite once it will be moved
                var newSurface = newPoint.ComputeSurfaceFromMiddlePoint(Surface.Size);

                // TODO: allow villagers to help for crafting

                // does the unit already intersect a structure?
                var alreadyIntersectingStructure = Surface.IntersectIntangibleStructure(Sprites.Concat(progressingCrafts));

                // will the unit intersect a structure with the new surface?
                var willIntersectStructureNext = newSurface.IntersectIntangibleStructure(Sprites.Concat(progressingCrafts));

                // HACK: when alreadyIntersectingStructure is TRUE
                // everything is allowed to get out
                // the use case if when a villager finish to craft a tangible structure (like a wall)
                // at this instant, he's on the center of the structure
                if (!alreadyIntersectingStructure && willIntersectStructureNext)
                {
                    var possibleNewPoints = SystemExtensions.GetEnum<Directions>()
                        .Select(card => (card, Center.GetPointFromCardinal(card, GetDefaultSpeed())))
                        .Where(x => !x.Item2.ComputeSurfaceFromMiddlePoint(Surface.Size).IntersectIntangibleStructure(Sprites.Concat(progressingCrafts)));

                    // TODO: add the check "in the area of the map"
                    (Directions, Point)? bestPoint = null;
                    if (possibleNewPoints.Any(x => x.Item2.X >= 0 && x.Item2.Y >= 0))
                        bestPoint = possibleNewPoints.OrderByDescending(x => x.card == currentForcedDirection).ThenBy(x => Point.Subtract(currentTargetPoint, x.Item2).LengthSquared).First();

                    if (bestPoint.HasValue)
                    {
                        newSurface = bestPoint.Value.Item2.ComputeSurfaceFromMiddlePoint(Surface.Size);
                        _currentPathTarget.Value.ForcedDirection = bestPoint.Value.Item1;
                    }
                    else
                    {
                        _currentPathTarget = null;
                        return null;
                    }
                }
                else
                {
                    // If we can get closer to the target without following a forced direction
                    // The current one (if any) is disabled
                    _currentPathTarget.Value.ForcedDirection = null;
                }

                // Proceed to move
                Move(newSurface);

                if (currentTargetPoint == Center)
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
                    currentTargetSprite = null;
                }

                return currentTargetSprite;
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
        public void SetPathCycle(params MoveTarget[] points)
        {
            lock (_pathCycle)
            {
                _pathCycle.Clear();
                var first = true;
                _isPathLoop = false;
                LinkedListNode<MoveTarget> node = null;
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
            return _pathCycle.Any(tc => tc.TargetSprite == sprite);
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
            return _pathCycle.FirstOrDefault(tc => tc.TargetSprite.Is<T>())?.TargetSprite as T;
        }

        /// <summary>
        /// Computes the path cycle from the specified point and targets related to point (if any)
        /// </summary>
        /// <param name="originalPoint"></param>
        /// <param name="targets"></param>
        /// <param name="inProgressCrafts"></param>
        public virtual void ComputeCycle(Point originalPoint, IEnumerable<Sprite> targets, IEnumerable<Craft> inProgressCrafts)
        {
            SetPathCycle(new MoveTarget(originalPoint));
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
