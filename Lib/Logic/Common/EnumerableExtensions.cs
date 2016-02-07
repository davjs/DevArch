using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Common
{
    public static class EnumerableExtensions
    {
        public static bool ContainsAnyFrom<T>(this IEnumerable<T> set, IEnumerable<T> set2)
        {
            return set.Intersect(set2).Any();
        }

        public static IReadOnlyCollection<TResult> SelectList<TResult,TSource>(this IEnumerable<TSource> input,  Func<TSource,TResult> selector)
        {
            return input.Select(selector).ToList();
        } 


        public static IEnumerable<T> Except<T>(this IEnumerable<T> set, T t)
        {
            return set.Where(x => !Equals(x, t));
        }
        

        public static void RemoveRange<T>(this List<T> list, IReadOnlyList<T> enumerable)
        {
            foreach (var toRemove in enumerable)
                list.Remove(toRemove);
        }
    }

    public static class HashetExtensions
    {
        
    }
}
