using System;
using System.Windows;

namespace Age_Of_Nothing
{
    public static class GeometryTools
    {
        /// <summary>
        /// Compute the next point on a grid of an object moving in straight line between two points.
        /// </summary>
        /// <param name="xSource">The X coordinate of the source point.</param>
        /// <param name="ySource">The Y coordinate of the source point.</param>
        /// <param name="xTarget">The X coordinate of the target point.</param>
        /// <param name="yTarget">The Y coordinate of the target point.</param>
        /// <param name="speed">The speed, in pixels per call, of the moving object.</param>
        /// <returns>The new point; if beyong target, the target is returned.</returns>
        /// <exception cref="InvalidOperationException">The quadratic equation has no solution.</exception>
        public static (double x2, double y2) ComputePointOnLine(
            double xSource, double ySource, double xTarget, double yTarget, double speed)
        {
            double x2, y2;
            if (xTarget - xSource == 0)
            {
                // when not a linear equation
                // aka straight up/down
                x2 = xSource;
                y2 = ySource + speed;
            }
            else
            {
                // linear equation parameters
                var slope = (yTarget - ySource) / (xTarget - xSource);
                var yIntercept = ySource - (slope * xSource);

                // quadratic equation parameters
                // using speed as hypotenuse from Pythagoras
                var aCoeff = (slope * slope) + 1;
                var bCoeff = (2 * slope * yIntercept) - (2 * ySource * slope) - (2 * xSource);
                var cCoeff = (yIntercept * yIntercept) - (yIntercept * ySource) - (ySource * yIntercept) + (ySource * ySource) + (xSource * xSource) - (speed * speed);

                var discriminant = (bCoeff * bCoeff) - (4 * aCoeff * cCoeff);

                var rootSolution1 = (-bCoeff + Math.Sqrt(discriminant)) / (2 * aCoeff);
                var rootSolution2 = (-bCoeff - Math.Sqrt(discriminant)) / (2 * aCoeff);

                // should NEVER occur
                if (rootSolution1 < 0 && rootSolution2 < 0)
                    throw new InvalidOperationException("The quadratic equation has no solution.");

                x2 = rootSolution1 < 0
                    ? rootSolution2
                    : (rootSolution2 < 0
                        ? rootSolution1
                        : (Math.Abs(rootSolution1 - xTarget) < Math.Abs(rootSolution2 - xTarget)
                            ? rootSolution1
                            : rootSolution2));
                y2 = (x2 * slope) + yIntercept;
            }

            var beyondXTarget = (x2 > xSource && x2 > xTarget)
                || (x2 < xSource && x2 < xTarget);
            var beyondYTarget = (y2 > ySource && y2 > yTarget)
                || (y2 < ySource && y2 < yTarget);

            return beyondXTarget || beyondYTarget
                ? (xTarget, yTarget)
                : (x2, y2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Rect ComputeSurfaceFromMiddlePoint(this Point center, double width, double height)
        {
            return new Rect(
                new Point(center.X - width / 2, center.Y - height / 2),
                new Point(center.X + width / 2, center.Y + height / 2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Rect ComputeSurfaceFromMiddlePoint(this Point center, Size size)
        {
            return center.ComputeSurfaceFromMiddlePoint(size.Width, size.Height);
        }
    }
}
