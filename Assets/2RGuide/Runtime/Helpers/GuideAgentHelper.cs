﻿using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
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
           Vector2 start, 
           Vector2 end, 
           float maxHeight, 
           float maxSlopeDegrees, 
           ConnectionType allowedConnectionTypes, 
           float pathfindingMaxDistance, 
           float segmentProximityMaxDistance,
           NavTag[] navTagCapable)
        {
            var navWorld = NavWorldReference.Instance.NavWorld;
            var startN = navWorld.GetClosestNode(start, segmentProximityMaxDistance);
            var endN = navWorld.GetClosestNode(end);
            var nodes = AStar.Resolve(startN, endN, maxHeight, maxSlopeDegrees, allowedConnectionTypes, pathfindingMaxDistance, navTagCapable);
            var pathStatus = PathStatus.Invalid;

            if (nodes == null || nodes.Length == 0)
            {
                return new PathfindingResult()
                {
                    segmentPath = null,
                    pathStatus = pathStatus
                };
            }

            var segmentPath = AgentSegmentPathBuilder.BuildPathFrom(start, end, nodes, segmentProximityMaxDistance, maxSlopeDegrees);

            var distanceFromTarget = Vector2.Distance(segmentPath.Last().position, end);
            pathStatus = distanceFromTarget < segmentProximityMaxDistance ? PathStatus.Complete : PathStatus.Incomplete;

            return new PathfindingResult()
            {
                segmentPath = segmentPath,
                pathStatus = pathStatus
            };
        }
    }
}