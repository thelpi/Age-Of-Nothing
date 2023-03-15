using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
