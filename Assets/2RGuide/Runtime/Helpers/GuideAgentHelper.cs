using Assets._2RGuide.Runtime.Math;
using System.Linq;
using static Assets._2RGuide.Runtime.AgentOperations;
using static Assets._2RGuide.Runtime.GuideAgent;

namespace Assets._2RGuide.Runtime.Helpers
{
    public class GuideAgentHelper
    {
        public struct PathfindingResult
        {
            public PathStatus pathStatus;
            public AgentSegment[] segmentPath;
        }

        public static PathfindingResult PathfindingTask(
            RGuideVector2 start, 
            RGuideVector2 end, 
            float maxHeight, 
            float maxSlopeDegrees, 
            ConnectionType allowedConnectionTypes, 
            float pathfindingMaxDistance, 
            float segmentProximityMaxDistance,
            NavTag[] navTagCapable, 
            float stepHeight,
            ConnectionTypeMultipliers connectionMultipliers)
        {
            var navWorld = NavWorldReference.Instance.NavWorld;
            var startN = navWorld.GetClosestNodeFromClosestSegment(start, ConnectionType.Walk, segmentProximityMaxDistance);
            var endN = navWorld.GetClosestNodeFromClosestSegment(end, ConnectionType.Walk);
            var nodes = AStar.Resolve(startN, endN, maxHeight, maxSlopeDegrees, allowedConnectionTypes, pathfindingMaxDistance, navTagCapable, stepHeight, connectionMultipliers);

            if (nodes == null || nodes.Length == 0)
            {
                return new PathfindingResult()
                {
                    segmentPath = null,
                    pathStatus = PathStatus.Invalid
                };
            }

            var segmentPath = AgentSegmentPathBuilder.BuildPathFrom(start, end, nodes, segmentProximityMaxDistance, maxSlopeDegrees, stepHeight);

            var distanceFromTarget = RGuideVector2.Distance(segmentPath.Last().position, end);
            var pathStatus = distanceFromTarget < segmentProximityMaxDistance ? PathStatus.Complete : PathStatus.Incomplete;

            return new PathfindingResult()
            {
                segmentPath = segmentPath,
                pathStatus = pathStatus
            };
        }
    }
}