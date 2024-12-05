using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    public static class Constants
    {
        public static float RGuideEpsilon
        {
            get
            {
                return CalculateMinimumPrecisionValue(RoundingDecimalPrecision);
            }
        }
        public static float RGuideDelta
        {
            get
            {
                return CalculateMinimumPrecisionValue(RoundingDecimalPrecision - 1);
            }
        }
        public const int RoundingDecimalPrecision = 4;

        private static float CalculateMinimumPrecisionValue(int precision)
        {
            var mult = Mathf.Pow(10.0f, precision);
            return 1.0f / mult;
        }
    }
}
