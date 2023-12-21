using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class FloatHelper
    {
        public static bool NearlyEqual(float a, float b, float epsilon)
        {
            return Mathf.Abs(a - b) < epsilon;
        }

        public static bool GreaterThanOrEquals(this float value, float other)
        {
            return value > other || Mathf.Approximately(value, other);
        }

        public static bool LessThanOrEquals(this float value, float other)
        {
            return value < other || Mathf.Approximately(value, other);
        }
    }
}