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
            var firstWalkableConnection = path[0].GetWalkableConnectionWithPosition(startPosition, segmentProximityMaxDistance, maxSlopeDegrees, stepHeight);
            var firstClosestPoint = firstWalkableConnection.Value.Segment.ClosestPointOnLine(startPosition);

            var lastWalkableConnection = path.Last().GetWalkableConnectionWithPosition(targetPosition, float.MaxValue, maxSlopeDegrees, stepHeight);
            var lastClosestPoint = lastWalkableConnection.Value.Segment.ClosestPointOnLine(targetPosition);

            if (firstWalkableConnection.Value.IsCoincident(lastWalkableConnection.Value))
            {
                return new [] {
                    new AgentSegment(firstClosestPoint, lastWalkableConnection.Value.ConnectionType),
                    new AgentSegment(lastClosestPoint, lastWalkableConnection.Value.ConnectionType),
                };
            }

            var agentSegments = new List<AgentSegment>();

            for (var nodeIndex = 0; nodeIndex < path.Length; nodeIndex++)
            {
                var connectionType =
                    nodeIndex == 0
                    ? ConnectionType.Walk
                    : path[nodeIndex - 1].ConnectionWith(path[nodeIndex]).Value.ConnectionType;

                agentSegments.Add(new AgentSegment() { position = path[nodeIndex].Position, connectionType = connectionType });
            }

            if (path.Length > 1 && path[0].ConnectionWith(path[1]).Value.IsCoincident(firstWalkableConnection.Value))
            {
                var agentSegment = agentSegments[0];
                agentSegment.position = firstClosestPoint;
                agentSegments[0] = agentSegment;
            }
            else
            {
                agentSegments.Insert(0, new AgentSegment() { position = firstClosestPoint, connectionType = ConnectionType.Walk });
            }

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
    }
}