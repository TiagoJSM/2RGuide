using Assets.Scripts._2RGuide.Math;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class LineSegmentExtensions
    {
        public static bool OverMaxSlope(this LineSegment2D segment, float maxSlope)
        {
            var slope = segment.Slope;
            if (slope != null)
            {
                return Mathf.Abs(slope.Value) > maxSlope || segment.NormalVector.y < 0.0f;
            }
            return true;
        }
    }
}