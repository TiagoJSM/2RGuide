using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class JumpsHelper
    {
        public struct Settings
        {
            public float maxJumpDistance;
            public float maxSlope;
        }

        public static LineSegment2D[] BuildJumps(List<Node> nodes, LineSegment2D[] segments, Settings settings)
        {
            var resultSegments = new List<LineSegment2D>();

            foreach (var node in nodes.ToArray())
            {
                var hasLeftSideConnections = node.HasLeftSideWalkConnection(settings.maxSlope);
                if (!hasLeftSideConnections)
                {
                    //var originX = node.Position.x - settings.horizontalDistance;
                    var target = FindClosestNodeToJumpTo(node, nodes);
                    if (target != null)
                    {
                        target.Connections.Add(NodeConnection.Jump(node));
                        node.Connections.Add(NodeConnection.Jump(target));
                        resultSegments.Add(new LineSegment2D() { P1 = node.Position, P2 = target.Position });
                    }
                }

                var hasRightSideConnections = node.HasRightSideWalkConnection(settings.maxSlope);
                if (!hasRightSideConnections)
                {
                    var target = FindClosestNodeToJumpTo(node, nodes);
                    if (target != null)
                    {
                        target.Connections.Add(NodeConnection.Jump(node));
                        node.Connections.Add(NodeConnection.Jump(target));
                        resultSegments.Add(new LineSegment2D() { P1 = node.Position, P2 = target.Position });
                    }
                }
            }

            return resultSegments.ToArray();
        }

        private static Node FindClosestNodeToJumpTo(Node node, List<Node> nodes)
        {
            return 
                nodes
                    .Where(n => n != node)
                    .MinBy(n => Vector2.Distance(n.Position, node.Position));
        }
    }
}