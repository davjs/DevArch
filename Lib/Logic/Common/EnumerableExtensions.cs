using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Common
{
    static class EnumerableExtensions
    {
        public static bool ContainsAnyFrom<T>(this IEnumerable<T> set, IEnumerable<T> set2)
        {
            return set.Intersect(set2).Any();
        }

        public static void RemoveRange<T>(this List<T> list, IReadOnlyList<T> enumerable)
        {
            foreach (var toRemove in enumerable)
                list.Remove(toRemove);
        }
    }
}
