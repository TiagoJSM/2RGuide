using System;
using System.Collections;
using UnityEditor.Graphs;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class FloatHelper
    {
        public static bool NearlyEqual(float a, float b, float epsilon)
        {
            return Mathf.Abs(a - b) < epsilon;
        }
    }
}