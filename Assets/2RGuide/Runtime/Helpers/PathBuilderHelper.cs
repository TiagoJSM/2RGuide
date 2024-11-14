using Assets._2RGuide.Runtime.Math;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class PathBuilderHelper
    {
        public static void GetOneWayPlatformSegments(NavBuildContext navBuildContext, NavBuilder navBuilder, RGuideVector2 raycastDirection, float distance, float maxSlope, ConnectionType connectionType, LineSegment2D[] existingConnections)
        {
            var dropPoints = navBuildContext.segments.Where(s => s.oneWayPlatform && !s.segment.OverMaxSlope(maxSlope)).Select(ns => ns.segment.HalfPoint).ToArray();

            foreach (var dropPoint in dropPoints)
            {
                var segments = navBuilder.NavSegments.Select(s => s.segment).ToArray();
                var oneWayPlatform = navBuilder.GetNavSegmentWithPoint(dropPoint);
                var start = dropPoint;
                var hit = Calculations.Raycast(dropPoint, dropPoint + raycastDirection * distance, segments.Except(new LineSegment2D[] { oneWayPlatform.segment }).ToArray());
                if (!hit)
                {
                    continue;
                }

                var targetPlatformSegment = hit.LineSegment;

                var segmentAlreadyPresent = existingConnections.Any(s => s.IsCoincident(new LineSegment2D(dropPoint, hit.HitPosition.Value)));
                if (segmentAlreadyPresent)
                {
                    continue;
                }
#if TWOR_GUIDE_DEBUG
                Debug.Log($"Split at {dropPoint.ToString("F6")}");
                Debug.Log($"Split at {hit.HitPosition.Value.ToString("F6")}");
#endif
                var n1 = navBuilder.SplitSegment(oneWayPlatform, dropPoint);
                var hitNavSegment = navBuilder.NavSegments.First(ns => ns.segment.IsCoincident(hit.LineSegment));
                var n2 = navBuilder.SplitSegment(hitNavSegment, hit.HitPosition.Value);

                navBuilder.AddNavSegment(new NavSegment()
                {
                    segment = new LineSegment2D() { P1 = n1.Position, P2 = n2.Position },
                    connectionType = connectionType,
                    maxHeight = float.PositiveInfinity,
                    navTag = null,
                    oneWayPlatform = true
                });
            }
        }
    }
}