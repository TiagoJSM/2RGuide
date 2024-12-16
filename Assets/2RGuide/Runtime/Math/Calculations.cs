using Assets._2RGuide.Runtime.Helpers;
using System.Collections.Generic;
using System.Linq;

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
                        (segment: s, intersection: ray.GetIntersection(s)))
                    .Where(v => 
                        v.intersection.HasValue && !v.intersection.Value.Approximately(origin))
                    .MinBy(v =>
                        RGuideVector2.Distance(v.Item2.Value, origin));

            return min.Item2.HasValue ? new CalculationRaycastHit(min.segment, min.intersection, RGuideVector2.Distance(min.intersection.Value, origin)) : new CalculationRaycastHit();
        }
    }
}