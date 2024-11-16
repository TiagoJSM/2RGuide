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
                var mult = Mathf.Pow(10.0f, RoundingDecimalPrecision);
                return 1.0f / mult;
            }
        }
        public const int RoundingDecimalPrecision = 4;
    }
}
