using _2RGuide.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Math
{
    public struct CalculationRaycastHit
    {
        public LineSegment2D LineSegment { get; private set; }
        public float Distance { get; private set; }
        public Vector2? HitPosition { get; private set; }

        public CalculationRaycastHit(LineSegment2D lineSegment, Vector2? hitPosition, float distance)
        {
            LineSegment = lineSegment;
            HitPosition = hitPosition;
            Distance = distance;
        }

        public bool HitLineEnd
        {
            get
            {
                if(!HitPosition.HasValue)
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
        public static CalculationRaycastHit Raycast(Vector2 origin, Vector2 end, IEnumerable<LineSegment2D> segments)
        {
            var ray = new LineSegment2D(origin, end);

            var min = 
                segments
                    .Select(s =>
                        (s, ray.GetIntersection(s)))
                    .Where(v => 
                        v.Item2.HasValue)
                    .MinBy(v => 
                        Vector2.Distance(v.Item2.Value, origin));

            return min.Item2.HasValue ? new CalculationRaycastHit(min.Item1, min.Item2, Vector2.Distance(min.Item2.Value, origin)) : new CalculationRaycastHit();
        }
    }
}