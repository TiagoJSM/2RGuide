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
    }
}