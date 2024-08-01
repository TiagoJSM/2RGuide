using System;
using System.Collections.Generic;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class IEnumerableExtensions
    {
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
        {
            var hasMin = false;
            TSource min = default;
            foreach (var value in source)
            {
                if (!hasMin)
                {
                    hasMin = true;
                    min = value;
                    continue;
                }

                if (keySelector(min).CompareTo(keySelector(value)) > 0)
                {
                    min = value;
                }
            }

            return min;
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
        {
            var hasMax = false;
            TSource max = default;
            foreach (var value in source)
            {
                if (!hasMax)
                {
                    hasMax = true;
                    max = value;
                    continue;
                }

                if (keySelector(max).CompareTo(keySelector(value)) < 0)
                {
                    max = value;
                }
            }

            return max;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}