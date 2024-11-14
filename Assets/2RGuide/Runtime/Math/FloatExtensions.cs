using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    public static class FloatExtensions
    {
        public static float Round(this float value, int digits) 
        {
            var mult = Mathf.Pow(10.0f, digits);
            return Mathf.Round(value * mult) / mult;
        }
    }
}
