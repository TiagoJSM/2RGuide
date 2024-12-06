using Assets._2RGuide.Runtime.Math;
using System;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class NodeExtensions
    {
        public static bool CanJumpOrDropToLeftSide(this Node node, float maxSlope)
        {
            return node.HasWalkConnection(maxSlope) && !node.HasLeftSideWalkConnection(maxSlope);
        }

        public static bool CanJumpOrDropToRightSide(this Node node, float maxSlope)
        {
            return node.HasWalkConnection(maxSlope) && !node.HasRightSideWalkConnection(maxSlope);
        }

        public static bool HasLeftSideWalkConnection(this Node node, float maxSlope)
        {
            return HasConnection(node, maxSlope, (node, cn) => cn.Position.x < node.Position.x);
        }

        public static bool HasRightSideWalkConnection(this Node node, float maxSlope)
        {
            return HasConnection(node, maxSlope, (node, cn) => cn.Position.x > node.Position.x);
        }

        public static NodeConnection? GetWalkableConnectionForPosition(this Node node, RGuideVector2 position, float segmentProximityMaxDistance, float maxSlopeDegrees, float stepHeight)
        {
            var eligibleConnections =
                node.Connections
                    .Where(c => c.IsWalkable(maxSlopeDegrees) || c.CanWalkOnStep(stepHeight));

            // Get the closest point in a segment
            if (eligibleConnections.Any())
            {
                var eligibleConnection =
                    eligibleConnections
                        .MinBy(c =>
                        {
                            var closestPoint = c.Segment.ClosestPointOnLine(position);
                            return RGuideVector2.Distance(closestPoint, position);
                        });

                // Add the closest point to the results
                var closestPoint = eligibleConnection.Segment.ClosestPointOnLine(position);
                if (RGuideVector2.Distance(closestPoint, position) < segmentProximityMaxDistance)
                {
                    return eligibleConnection;
                }
            }

            return null;
        }

        public static bool CanWalkOnStep(this NodeConnection neighbor, float stepHeight)
        {
            return neighbor.Segment.Lenght < stepHeight;
        }

        public static bool IsWalkable(this NodeConnection nc, float maxSlopeDegrees)
        {
            if (nc.ConnectionType != ConnectionType.Walk)
            {
                return false;
            }
            if (Mathf.Abs(nc.Segment.SlopeDegrees) > maxSlopeDegrees)
            {
                return false;
            }
            return true;
        }

        private static bool HasWalkConnection(this Node node, float maxSlope)
        {
            return node.Connections.Any(c =>
                    c.ConnectionType == ConnectionType.Walk &&
                    !c.Segment.OverMaxSlope(maxSlope));
        }

        private static bool HasConnection(Node node, float maxSlope, Func<Node, Node, bool> predicate)
        {
            return node
                .Connections
                .Where(c => c.ConnectionType == ConnectionType.Walk)
                .Any(cn =>
                {
                    var line = cn.Segment;
                    if (Mathf.Abs(line.SlopeRadians) > maxSlope)
                    {
                        return false;
                    }
                    return predicate(node, cn.Node);
                });
        }
    }
}