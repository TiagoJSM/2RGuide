using Assets._2RGuide.Runtime.Math;
using System.Collections.Generic;
using System.ComponentModel;
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
                    new AgentSegment(firstClosestPoint, lastWalkableConnection.Value.ConnectionType, lastWalkableConnection.Value.IsWalkableStep(stepHeight, maxSlopeDegrees)),
                    new AgentSegment(lastClosestPoint, lastWalkableConnection.Value.ConnectionType, lastWalkableConnection.Value.IsWalkableStep(stepHeight, maxSlopeDegrees)),
                };
            }

            var agentSegments = new List<AgentSegment>();

            for (var nodeIndex = 0; nodeIndex < path.Length; nodeIndex++)
            {
                var connectionType = firstWalkableConnection.Value.ConnectionType;
                var isStep = firstWalkableConnection.Value.IsWalkableStep(stepHeight, maxSlopeDegrees);

                if(nodeIndex > 0)
                {
                    var connection = path[nodeIndex - 1].ConnectionWith(path[nodeIndex]);
                    connectionType = connection.Value.ConnectionType;
                    isStep = connection.Value.IsWalkableStep(stepHeight, maxSlopeDegrees);
                }
                
                agentSegments.Add(new AgentSegment(path[nodeIndex].Position, connectionType, isStep));
            }

            if (path.Length > 1 && path[0].ConnectionWith(path[1]).Value.IsCoincident(firstWalkableConnection.Value))
            {
                var agentSegment = agentSegments[0];
                var newAgentSegment = new AgentSegment(firstClosestPoint, agentSegment.ConnectionType, agentSegment.IsStep);
                agentSegments[0] = newAgentSegment;
            }
            else
            {
                agentSegments.Insert(0, new AgentSegment(firstClosestPoint, firstWalkableConnection.Value.ConnectionType, firstWalkableConnection.Value.IsWalkableStep(stepHeight, maxSlopeDegrees)));
            }

            var lastSegment = new LineSegment2D(agentSegments[agentSegments.Count - 2].Position, agentSegments[agentSegments.Count - 1].Position);
            
            if (lastSegment.Contains(lastClosestPoint))
            {
                var agentSegment = agentSegments[agentSegments.Count - 1];
                var newAgentSegment = new AgentSegment(lastClosestPoint, agentSegment.ConnectionType, agentSegment.IsStep);
                agentSegments[agentSegments.Count - 1] = newAgentSegment;
            }
            else
            {
                var connectionType = lastWalkableConnection.Value.ConnectionType;
                var isStep = lastWalkableConnection.Value.IsWalkableStep(stepHeight, maxSlopeDegrees);
                agentSegments.Add(new AgentSegment(lastClosestPoint, connectionType, isStep));
            }

            return agentSegments.DistinctBy(s => s.Position).ToArray();
        }
    }
}