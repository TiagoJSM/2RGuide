using Assets.Scripts._2RGuide.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class DropsHelper
    {
        public struct Settings
        {
            public float maxDropHeight;
            public float horizontalDistance;
            public float maxSlope;
        }

        public static LineSegment2D[] BuildDrops(List<Node> nodes, LineSegment2D[] segments, Settings settings)
        {
            var resultSegments = new List<LineSegment2D>();

            foreach (var node in nodes.ToArray())
            {
                var hasLeftSideConnections = HasLeftSideConnection(node, settings);
                if (!hasLeftSideConnections)
                {
                    var originX = node.Position.x - settings.horizontalDistance;
                    var target = FindTargetDropSegment(node, segments, originX, settings);
                    if (target)
                    {
                        AddDropNodeForSegment(target, nodes, node);
                        resultSegments.Add(target);
                    }
                }

                var hasRightSideConnections = HasRightSideConnection(node, settings);
                if (!hasRightSideConnections)
                {
                    var originX = node.Position.x + settings.horizontalDistance;
                    var target = FindTargetDropSegment(node, segments, originX, settings);
                    if (target)
                    {
                        AddDropNodeForSegment(target, nodes, node);
                        resultSegments.Add(target);
                    }
                }
            }

            return resultSegments.ToArray();
        }

        private static void AddDropNodeForSegment(LineSegment2D target, List<Node> nodes, Node dropNode)
        {
            var node = new Node() { Position = target.P2 };

            var connectedNode1 = nodes.FirstOrDefault(n => n.Position == target.P1);
            if(connectedNode1 != null)
            {
                node.Connections.Add(connectedNode1);
            }

            var connectedNode2 = nodes.FirstOrDefault(n => n.Position == target.P2);
            if (connectedNode2 != null)
            {
                node.Connections.Add(connectedNode2);
            }

            dropNode.Connections.Add(node);
            nodes.Add(node);
        }

        private static LineSegment2D FindTargetDropSegment(Node node, LineSegment2D[] segments, float originX, Settings settings)
        {
            var origin = new Vector2(originX, node.Position.y);

            var segment = segments.Where(s =>
            {
                var position = s.PositionInX(originX);
                if(!position.HasValue)
                {
                    return false;
                }
                if(origin.y < position.Value.y)
                {
                    return false;
                }
                return Vector2.Distance(position.Value, origin) <= settings.maxDropHeight;
            })
            .MinBy(s =>
            {
                var position = s.PositionInX(originX);
                return Vector2.Distance(position.Value, origin);
            });
            if (segment)
            {
                return new LineSegment2D(node.Position, segment.PositionInX(originX).Value);
            }
            return default;
        }

        private static bool HasLeftSideConnection(Node node, Settings settings)
        {
            return HasConnection(node, settings, (node, cn) => cn.Position.x < node.Position.x);
        }

        private static bool HasRightSideConnection(Node node, Settings settings)
        {
            return HasConnection(node, settings, (node, cn) => cn.Position.x > node.Position.x);
        }

        private static bool HasConnection(Node node, Settings settings, Func<Node, Node, bool> predicate)
        {
            return node.Connections.Any(cn =>
            {
                var line = new LineSegment2D(node.Position, cn.Position);
                if (Mathf.Abs(line.SlopeRadians) > settings.maxSlope)
                {
                    return false;
                }
                return predicate(node, cn);
            });
        }
    }
}