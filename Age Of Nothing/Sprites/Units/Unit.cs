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

                // does the unit already intersect a structure?
                var alreadyIntersectingStructure = Surface.IntersectIntangibleStructure(Sprites.Concat(progressingCrafts));

                // will the unit intersect a structure with the new surface?
                var intersectionsNext = newSurface.GetIntangibleStructureIntersections(Sprites.Concat(progressingCrafts));

                // checks if the single intersection is the current target
                // and a craft in progress
                // unit has to be a villager
                var intersectionIsTargetAndCraft = intersectionsNext.Count() == 1
                    && intersectionsNext.First() == currentTargetSprite
                    && progressingCrafts.Contains(currentTargetSprite)
                    && Is<Villager>();

                // HACK: when alreadyIntersectingStructure is TRUE
                // everything is allowed to get out
                // the use case if when a villager finish to craft a tangible structure (like a wall)
                // at this instant, he's on the center of the structure
                if (!alreadyIntersectingStructure && intersectionsNext.Any() && !intersectionIsTargetAndCraft)
                {
                    if (currentTargetSprite == null)
                    {
                        // The target is not a sprite but just a point
                        // (or at least was no when set)
                        // but the target point, with unit surface, collides with a tangible structure
                        // is this sprite the same sprite as the one who collides now ?
                        // if so, we stop
                        var intersections = currentTargetPoint
                            .ComputeSurfaceFromMiddlePoint(Surface.Size)
                            .GetIntangibleStructureIntersections(Sprites);
                        if (intersectionsNext.Any(x => intersections.Contains(x)))
                        {
                            _currentPathTarget = null;
                            return null;
                        }
                    }

                    var bestPoint = UsePathFindingForNextPoint(currentTargetPoint, progressingCrafts, ref currentForcedDirection);
                    if (bestPoint.HasValue)
                    {
                        newSurface = bestPoint.Value.ComputeSurfaceFromMiddlePoint(Surface.Size);
                        _currentPathTarget.Value.ForcedDirection = currentForcedDirection;
                    }
                    else
                    {
                        // no solution: we cancel the target completely
                        // it avoids a recomputing at each frame for nothing
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

        private Point? UsePathFindingForNextPoint(
            Point currentTargetPoint,
            IEnumerable<Structures.Structure> progressingCrafts,
            ref Directions? direction)
        {
            Point? bestPoint = null;
            var distance = double.MaxValue;

            // if we're already in forced direction, we compute the point in this direction
            if (direction.HasValue)
                bestPoint = GetNextPointFromDirection(direction.Value, progressingCrafts);

            if (bestPoint.HasValue)
                return bestPoint;

            // no forced direction, or forced direction unavaible: try other directions
            foreach (var localDirection in SystemExtensions.GetEnum<Directions>())
            {
                // already done above
                if (localDirection == direction)
                    continue;

                // compute if point avaiable for the current direction in the loop
                var localBestPoint = GetNextPointFromDirection(localDirection, progressingCrafts);
                if (localBestPoint.HasValue)
                {
                    // compute the distance between this point and the target
                    var localDistance = Point.Subtract(localBestPoint.Value, currentTargetPoint).LengthSquared;
                    if (localDistance < distance)
                    {
                        // if closer distance,  that's the point we keep
                        bestPoint = localBestPoint;
                        direction = localDirection;
                        distance = localDistance;
                    }
                }
            }

            return bestPoint;
        }

        private Point? GetNextPointFromDirection(Directions direction, IEnumerable<Structures.Structure> progressingCrafts)
        {
            var nextCenter = Center.GetPointFromCardinal(direction, GetDefaultSpeed());

            // TODO: add the check "in the area of the map"
            if (nextCenter.X < 0 || nextCenter.Y < 0)
                return null;

            var intersect = nextCenter
                .ComputeSurfaceFromMiddlePoint(Surface.Size)
                .IntersectIntangibleStructure(Sprites.Concat(progressingCrafts));
            
            return intersect ? default(Point?) : nextCenter;
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
