using Assets._2RGuide.Runtime.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    public struct CalculationRaycastHit
    {
        public LineSegment2D LineSegment { get; private set; }
        public float Distance { get; private set; }
        public RGuideVector2? HitPosition { get; private set; }

        public CalculationRaycastHit(LineSegment2D lineSegment, RGuideVector2? hitPosition, float distance)
        {
            LineSegment = lineSegment;
            HitPosition = hitPosition;
            Distance = distance;
        }

        public bool HitLineEnd
        {
            get
            {
                if (!HitPosition.HasValue)
                {
                    return false;
                }

                return LineSegment.P1.Approximately(HitPosition.Value) || LineSegment.P2.Approximately(HitPosition.Value);
            }
        }

        public static implicit operator bool(CalculationRaycastHit hit)
        {
            return hit.HitPosition.HasValue;
        }
    }


    public static class Calculations
    {
        public static CalculationRaycastHit Raycast(RGuideVector2 origin, RGuideVector2 end, IEnumerable<LineSegment2D> segments)
        {
            var ray = new LineSegment2D(origin, end);

            var min =
                segments
                    .Select(s =>
                        (s, ray.GetIntersection(s)))
                    .Where(v => 
                        v.Item2.HasValue && !v.Item2.Value.Approximately(origin))
                    .MinBy(v =>
                        RGuideVector2.Distance(v.Item2.Value, origin));

            return min.Item2.HasValue ? new CalculationRaycastHit(min.Item1, min.Item2, RGuideVector2.Distance(min.Item2.Value, origin)) : new CalculationRaycastHit();
        }


        /// <summary>
        /// Determines if the given point is inside the polygon, taken from https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsPointInPolygon4(RGuideVector2[] polygon, RGuideVector2 testPoint)
        {
            var result = false;
            var j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y ||
                    polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
                {
                    if (polygon[i].x + (testPoint.y - polygon[i].y) /
                       (polygon[j].y - polygon[i].y) *
                       (polygon[j].x - polygon[i].x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}