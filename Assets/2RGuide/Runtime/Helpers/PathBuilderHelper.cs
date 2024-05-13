using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class PathBuilderHelper
    {
        public static void AddTargetNodeForSegment(LineSegment2D target, NavBuilder navBuilder, ConnectionType connectionType, float maxSlope, float maxHeight)
        {
            var dropTargetSegment = navBuilder.NavSegments.FirstOrDefault(ss => !ss.segment.OverMaxSlope(maxSlope) && ss.segment.OnSegment(target.P2));

            if (!dropTargetSegment)
            {
                return;
            }

            var targetNode = navBuilder.SplitSegment(dropTargetSegment, target.P2);

            if (!dropTargetSegment.segment.P1.Approximately(targetNode.Position))
            {
                var ns = new NavSegment()
                {
                    segment = new LineSegment2D(dropTargetSegment.segment.P1, targetNode.Position),
                    maxHeight = maxHeight,
                    oneWayPlatform = dropTargetSegment.oneWayPlatform,
                    navTag = dropTargetSegment.navTag,
                    connectionType = connectionType
                };
                navBuilder.AddNavSegment(ns);
            }
            if (!targetNode.Position.Approximately(dropTargetSegment.segment.P2))
            {
                var ns = new NavSegment()
                {
                    segment = new LineSegment2D(targetNode.Position, dropTargetSegment.segment.P2),
                    maxHeight = maxHeight,
                    oneWayPlatform = dropTargetSegment.oneWayPlatform,
                    navTag = dropTargetSegment.navTag,
                    connectionType = connectionType
                };
                navBuilder.AddNavSegment(ns);
            }
        }

        public static void GetOneWayPlatformSegments(NavBuildContext navBuildContext, NavBuilder navBuilder, Vector2 raycastDirection, float distance, float maxSlope, ConnectionType connectionType, LineSegment2D[] existingConnections)
        {
            var oneWayPlatforms = navBuildContext.segments.Where(s => s.oneWayPlatform && !s.segment.OverMaxSlope(maxSlope)).ToArray();
            var segments = navBuildContext.segments.Select(s => s.segment);

            foreach (var oneWayPlatform in oneWayPlatforms)
            {
                var start = oneWayPlatform.segment.HalfPoint;
                var hit = Calculations.Raycast(oneWayPlatform.segment.HalfPoint, start + raycastDirection * distance, segments.Except(new LineSegment2D[] { oneWayPlatform.segment }));
                if (!hit)
                {
                    continue;
                }

                var hitNavSegment = navBuildContext.segments.First(ns => ns.segment == hit.LineSegment);

                var oneWayPlatformSegment = segments.GetSegmentWithPosition(oneWayPlatform.segment.HalfPoint);
                var targetPlatformSegment = hit.LineSegment;
                
                var segmentAlreadyPresent = existingConnections.Any(s => s.IsCoincident(new LineSegment2D(oneWayPlatform.segment.HalfPoint, hit.HitPosition.Value)));
                if(segmentAlreadyPresent)
                {
                    continue;
                }

                var n1 = navBuilder.SplitSegment(oneWayPlatform, oneWayPlatform.segment.HalfPoint);
                var n2 = navBuilder.SplitSegment(hitNavSegment, hit.HitPosition.Value);
                navBuilder.AddNavSegment(new NavSegment() 
                    {
                        segment = new LineSegment2D() { P1 = n1.Position, P2 = n2.Position },
                        connectionType = connectionType,
                        maxHeight = float.PositiveInfinity,
                        navTag = null,
                        oneWayPlatform = true
                    });

                //var oneWayPlatformNode = nodes.SplitSegmentAt(oneWayPlatform.segment, oneWayPlatform.segment.HalfPoint);
                //var targetNode = nodes.SplitSegmentAt(targetPlatformSegment, hit.HitPosition.Value);

                //var segment = nodes.ConnectNodes(oneWayPlatformNode, targetNode, float.PositiveInfinity, connectionType, false);

                //resultSegments.Add(segment);
            }
        }
    }
}