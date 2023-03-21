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
    }
}
