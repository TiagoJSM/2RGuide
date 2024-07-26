using Assets._2RGuide.Runtime.Math;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class PathBuilderHelper
    {
        public static void GetOneWayPlatformSegments(NavBuildContext navBuildContext, NavBuilder navBuilder, Vector2 raycastDirection, float distance, float maxSlope, ConnectionType connectionType, LineSegment2D[] existingConnections)
        {
            var oneWayPlatforms = navBuildContext.segments.Where(s => s.oneWayPlatform && !s.segment.OverMaxSlope(maxSlope)).ToArray();
            //var segments = navBuildContext.segments.Select(s => s.segment).ToArray();
            //var oneWayPlatforms = navBuilder.NavSegments.Where(s => s.oneWayPlatform && !s.segment.OverMaxSlope(maxSlope)).ToArray();
            var segments = navBuilder.NavSegments.Select(s => s.segment).ToArray();

            foreach (var oneWayPlatform in oneWayPlatforms)
            {
                var start = oneWayPlatform.segment.HalfPoint;
                var hit = Calculations.Raycast(oneWayPlatform.segment.HalfPoint, start + raycastDirection * distance, segments.Except(new LineSegment2D[] { oneWayPlatform.segment }).ToArray());
                if (!hit)
                {
                    continue;
                }

                var targetPlatformSegment = hit.LineSegment;

                var segmentAlreadyPresent = existingConnections.Any(s => s.IsCoincident(new LineSegment2D(oneWayPlatform.segment.HalfPoint, hit.HitPosition.Value)));
                if (segmentAlreadyPresent)
                {
                    continue;
                }

                var n1 = navBuilder.SplitSegment(oneWayPlatform.segment.HalfPoint);
                var n2 = navBuilder.SplitSegment(hit.HitPosition.Value);

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