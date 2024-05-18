using System.Collections.Generic;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefaultValue<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}