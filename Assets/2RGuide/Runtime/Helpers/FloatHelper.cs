using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class FloatHelper
    {
        public static bool NearlyEqual(float a, float b, float epsilon)
        {
            return Mathf.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Determines if the float value is less than or equal to the float parameter according to the defined precision.
        /// </summary>
        /// <param name="float1">The float1.</param>
        /// <param name="float2">The float2.</param>
        /// <returns></returns>
        public static bool LessThan(this float float1, float float2)
        {
            return (System.Math.Round(float1 - float2, Constants.RoundingDecimalPrecision) < 0);
        }

        /// <summary>
        /// Determines if the float value is less than or equal to the float parameter according to the defined precision.
        /// </summary>
        /// <param name="float1">The float1.</param>
        /// <param name="float2">The float2.</param>
        /// <returns></returns>
        public static bool LessThanOrEquals(this float float1, float float2)
        {
            return (System.Math.Round(float1 - float2, Constants.RoundingDecimalPrecision) <= 0);
        }

        /// <summary>
        /// Determines if the float value is greater than (>) the float parameter according to the defined precision.
        /// </summary>
        /// <param name="float1">The float1.</param>
        /// <param name="float2">The float2.</param>
        /// <returns></returns>
        public static bool GreaterThan(this float float1, float float2)
        {
            return (System.Math.Round(float1 - float2, Constants.RoundingDecimalPrecision) > 0);
        }

        /// <summary>
        /// Determines if the float value is greater than or equal to (>=) the float parameter according to the defined precision.
        /// </summary>
        /// <param name="float1">The float1.</param>
        /// <param name="float2">The float2.</param>
        /// <returns></returns>
        public static bool GreaterThanOrEquals(this float float1, float float2)
        {
            return (System.Math.Round(float1 - float2, Constants.RoundingDecimalPrecision) >= 0);
        }

        /// <summary>
        /// Determines if the float value is equal to (==) the float parameter according to the defined precision.
        /// </summary>
        /// <param name="float1">The float1.</param>
        /// <param name="float2">The float2.</param>
        /// <returns></returns>
        public static bool Approximately(this float float1, float float2)
        {
            return (System.Math.Round(float1 - float2, Constants.RoundingDecimalPrecision) == 0);
        }

        public static float Round(this float value, int digits)
        {
            var mult = Mathf.Pow(10.0f, digits);
            return Mathf.Round(value * mult) / mult;
        }
    }
}