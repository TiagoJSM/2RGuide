using _2RGuide.Math;
using System;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Helpers
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

        private static bool HasWalkConnection(this Node node, float maxSlope)
        {
            return node.Connections.Any(c =>
                    c.connectionType == ConnectionType.Walk &&
                    !c.segment.OverMaxSlope(maxSlope));
        }

        private static bool HasConnection(Node node, float maxSlope, Func<Node, Node, bool> predicate)
        {
            return node
                .Connections
                .Where(c => c.connectionType == ConnectionType.Walk)
                .Any(cn =>
                {
                    var line = new LineSegment2D(node.Position, cn.node.Position);
                    if (Mathf.Abs(line.SlopeRadians) > maxSlope)
                    {
                        return false;
                    }
                    return predicate(node, cn.node);
                });
        }
    }
}