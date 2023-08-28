using Assets.Scripts._2RGuide.Math;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class NodeExtensions
    {
        public static bool HasLeftSideWalkConnection(this Node node, float maxSlope)
        {
            return HasConnection(node, maxSlope, (node, cn) => cn.Position.x < node.Position.x);
        }

        public static bool HasRightSideWalkConnection(this Node node, float maxSlope)
        {
            return HasConnection(node, maxSlope, (node, cn) => cn.Position.x > node.Position.x);
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