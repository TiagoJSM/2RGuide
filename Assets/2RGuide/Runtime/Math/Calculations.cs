using _2RGuide.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Math
{
    public struct CalculationRaycastHit
    {
        public float Distance { get; private set; }
        public Vector2? HitPosition { get; private set; }

        public CalculationRaycastHit(Vector2? hitPosition, float distance)
        {
            HitPosition = hitPosition;
            Distance = distance;
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
                        ray.GetIntersection(s, false))
                    .Where(v => 
                        v.HasValue)
                    .MinBy(v => 
                        Vector2.Distance(v.Value, origin));

            return min.HasValue ? new CalculationRaycastHit(min, Vector2.Distance(min.Value, origin)) : new CalculationRaycastHit();
        }
    }
}