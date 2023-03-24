using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Age_Of_Nothing
{
    public static class SystemExtensions
    {
        public static bool FirstIfNotNull<T>(this IEnumerable<T> collection, Func<T, bool> predicate, out T item)
            where T : class
        {
            item = collection.FirstOrDefault(predicate);
            return item != null;
        }

        public static TAttr GetAttribute<TAttr>(this Type targetType)
            where TAttr : Attribute
        {
            return Attribute.GetCustomAttribute(targetType, typeof(TAttr)) as TAttr;
        }

        public static IEnumerable<T> GetEnum<T>()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Type should be enumerable.");

            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static Point RescaleBase10(this Point point)
        {
            return new Point(point.X.RescaleBase10(), point.Y.RescaleBase10());
        }

        public static bool RealIntersectsWith(this Rect mainSurface, Rect secondSurface)
        {
            var size = Rect.Intersect(mainSurface, secondSurface).Size;
            return size.Width > 0 && size.Height > 0;
        }

        private static double RescaleBase10(this double value)
        {
            var valuePow10 = value / 10;
            var valuePow10Floot = Math.Floor(valuePow10);
            var decimalPow10 = valuePow10 - valuePow10Floot;
            return decimalPow10 <= 0.5
                ? valuePow10Floot * 10
                : (valuePow10Floot + 1) * 10;
        }
    }
}
