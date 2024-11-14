using Assets._2RGuide.Runtime.Math;
using System.Collections.Generic;
using System.Linq;
using static Assets._2RGuide.Runtime.AgentOperations;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class AgentSegmentPathBuilder
    {
        public static AgentSegment[] BuildPathFrom(RGuideVector2 startPosition, RGuideVector2 targetPosition, Node[] path, float segmentProximityMaxDistance, float maxSlopeDegrees, float stepHeight)
        {
            var agentSegments = new List<AgentSegment>();

            for (var nodeIndex = 0; nodeIndex < path.Length; nodeIndex++)
            {
                var connectionType =
                    nodeIndex == 0
                    ? ConnectionType.Walk
                    : path[nodeIndex - 1].ConnectionWith(path[nodeIndex]).Value.ConnectionType;

                agentSegments.Add(new AgentSegment() { position = path[nodeIndex].Position, connectionType = connectionType });
            }

            var firstWalkableConnection = path[0].GetWalkableConnectionForPosition(startPosition, segmentProximityMaxDistance, maxSlopeDegrees, stepHeight);
            var firstClosestPoint = firstWalkableConnection.Value.Segment.ClosestPointOnLine(startPosition);

            if(path.Length > 1 && IsPositionInBetweenConnection(path, 0, firstClosestPoint, segmentProximityMaxDistance))
            {
                var agentSegment = agentSegments[0];
                agentSegment.position = firstClosestPoint;
                agentSegments[0] = agentSegment;
            }
            else
            {
                agentSegments.Insert(0, new AgentSegment() { position = firstClosestPoint, connectionType = ConnectionType.Walk });
            }

            var lastWalkableConnection = path.Last().GetWalkableConnectionForPosition(targetPosition, float.MaxValue, maxSlopeDegrees, stepHeight);
            var lastClosestPoint = lastWalkableConnection.Value.Segment.ClosestPointOnLine(targetPosition);

            var lastSegment = new LineSegment2D(agentSegments[agentSegments.Count - 2].position, agentSegments[agentSegments.Count - 1].position);
            
            if (lastSegment.Contains(lastClosestPoint))
            {
                var agentSegment = agentSegments[agentSegments.Count - 1];
                agentSegment.position = lastClosestPoint;
                agentSegments[agentSegments.Count - 1] = agentSegment;
            }
            else
            {
                agentSegments.Add(new AgentSegment() { position = lastClosestPoint, connectionType = ConnectionType.Walk });
            }

            return agentSegments.DistinctBy(s => s.position).ToArray();
        }

        private static bool IsPositionInBetweenConnection(Node[] path, int index, RGuideVector2 position, float segmentProximityMaxDistance)
        {
            var segment = path[index].ConnectionWith(path[index + 1]).Value.Segment;
            var closestPoint = segment.ClosestPointOnLine(position);
            return RGuideVector2.Distance(position, closestPoint) < segmentProximityMaxDistance;
        }
    }
}