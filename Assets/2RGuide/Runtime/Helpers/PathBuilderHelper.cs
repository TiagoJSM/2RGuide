using _2RGuide.Math;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class PathBuilderHelper
    {
        public static void AddTargetNodeForSegment(LineSegment2D target, NodeStore nodeStore, List<NavSegment> navSegments, Node startNode, ConnectionType connectionType, float maxSlope, float maxHeight)
        {
            var dropTargetSegment = navSegments.FirstOrDefault(ss => !ss.segment.OverMaxSlope(maxSlope) && ss.segment.OnSegment(target.P2));

            if (!dropTargetSegment)
            {
                return;
            }

            var targetNode = nodeStore.SplitSegmentAt(dropTargetSegment.segment, target.P2);
            navSegments.Remove(dropTargetSegment);

            if (!dropTargetSegment.segment.P1.Approximately(targetNode.Position))
            {
                navSegments.Add(new NavSegment()
                {
                    segment = new LineSegment2D(dropTargetSegment.segment.P1, targetNode.Position),
                    maxHeight = dropTargetSegment.maxHeight,
                    oneWayPlatform = dropTargetSegment.oneWayPlatform,
                    obstacle = dropTargetSegment.obstacle,
                });
            }
            if (!targetNode.Position.Approximately(dropTargetSegment.segment.P2))
            {
                navSegments.Add(new NavSegment()
                {
                    segment = new LineSegment2D(targetNode.Position, dropTargetSegment.segment.P2),
                    maxHeight = dropTargetSegment.maxHeight,
                    oneWayPlatform = dropTargetSegment.oneWayPlatform,
                    obstacle = dropTargetSegment.obstacle,
                });
            }

            AddConnection(startNode, targetNode, connectionType, maxHeight);
        }

        public static void GetOneWayPlatformSegments(NavBuildContext navBuildContext, NodeStore nodes, Vector2 raycastDirection, float distance, float maxSlope, ConnectionType connectionType, LineSegment2D[] existingConnections, List<LineSegment2D> resultSegments)
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

                var oneWayPlatformSegment = segments.GetSegmentWithPosition(oneWayPlatform.segment.HalfPoint);
                var targetPlatformSegment = hit.LineSegment;
                
                var segmentAlreadyPresent = existingConnections.Any(s => s.IsCoincident(new LineSegment2D(oneWayPlatform.segment.HalfPoint, hit.HitPosition.Value)));
                if(segmentAlreadyPresent)
                {
                    continue;
                }

                var oneWayPlatformNode = nodes.SplitSegmentAt(oneWayPlatform.segment, oneWayPlatform.segment.HalfPoint);
                var targetNode = nodes.SplitSegmentAt(targetPlatformSegment, hit.HitPosition.Value);

                var segment = nodes.ConnectNodes(oneWayPlatformNode, targetNode, float.PositiveInfinity, connectionType, false);

                resultSegments.Add(segment);
            }
        }

        private static void AddConnection(Node startNode, Node endNode, ConnectionType connectionType, float maxHeight)
        {
            switch (connectionType)
            {
                case ConnectionType.Walk:
                    startNode.AddConnection(ConnectionType.Walk, endNode, new LineSegment2D(startNode.Position, endNode.Position), maxHeight, false);
                    break;
                case ConnectionType.Drop:
                    startNode.AddConnection(ConnectionType.Drop, endNode, new LineSegment2D(startNode.Position, endNode.Position), maxHeight, false);
                    break;
                case ConnectionType.Jump:
                    startNode.AddConnection(ConnectionType.Jump, endNode, new LineSegment2D(startNode.Position, endNode.Position), maxHeight, false);
                    endNode.AddConnection(ConnectionType.Jump, startNode, new LineSegment2D(endNode.Position, startNode.Position), maxHeight, false);
                    break;
            }
        }
    }
}