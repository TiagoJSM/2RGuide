using Assets.Scripts._2RGuide.Math;
using System;
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
                //if(node.Connections.All(c => 
                //    OverMaxSlope(c.segment, settings.maxSlope)))
                //{
                //    continue;
                //}

                var canJumpToLeftSide = node.CanJumpToLeftSide(settings.maxSlope);
                if (canJumpToLeftSide)
                {
                    var target = FindClosestNodeToJumpToTheLeft(node, nodes, segments, settings);
                    if (target != null)
                    {
                        var segment = new LineSegment2D() { P1 = node.Position, P2 = target.Position };
                        target.Connections.Add(NodeConnection.Jump(node, segment));
                        node.Connections.Add(NodeConnection.Jump(target, segment));
                        resultSegments.Add(segment);
                    }
                }

                var canJumpToRightSide = node.CanJumpToRightSide(settings.maxSlope);
                if (canJumpToRightSide)
                {
                    var target = FindClosestNodeToJumpToTheRight(node, nodes, segments, settings);
                    if (target != null)
                    {
                        var segment = new LineSegment2D() { P1 = node.Position, P2 = target.Position };
                        target.Connections.Add(NodeConnection.Jump(node, segment));
                        node.Connections.Add(NodeConnection.Jump(target, segment));
                        resultSegments.Add(segment);
                    }
                }
            }

            return resultSegments.ToArray();
        }

        private static bool IsJumpNode(this Node node, float maxSlope)
        {
            return node.Connections.Any(c =>
                    c.connectionType == ConnectionType.Walk &&
                    !OverMaxSlope(c.segment, maxSlope));
        }

        private static bool CanJumpToLeftSide(this Node node, float maxSlope)
        {
            return node.IsJumpNode(maxSlope) && !node.HasLeftSideWalkConnection(maxSlope);
        }

        private static bool CanJumpToRightSide(this Node node, float maxSlope)
        {
            return node.IsJumpNode(maxSlope) && !node.HasRightSideWalkConnection(maxSlope);
        }

        private static Node FindClosestNodeToJumpToTheLeft(Node node, List<Node> nodes, LineSegment2D[] segments, Settings settings)
        {
            return FindClosestNodeToJumpTo(
                node, 
                nodes,
                segments,
                settings, 
                n => n.Position.x < node.Position.x
            );
        }

        private static Node FindClosestNodeToJumpToTheRight(Node node, List<Node> nodes, LineSegment2D[] segments, Settings settings)
        {
            return FindClosestNodeToJumpTo(
                node, 
                nodes,
                segments,
                settings, 
                n => n.Position.x > node.Position.x
            );
        }

        private static Node FindClosestNodeToJumpTo(Node node, List<Node> nodes, LineSegment2D[] segments, Settings settings, Func<Node, bool> predicate)
        {
            return 
                nodes
                    .Where(n => n != node)
                    .Where(n => 
                        n.Connections.Any(c => 
                            c.connectionType == ConnectionType.Walk && !OverMaxSlope(c.segment, settings.maxSlope)))
                    .Where(predicate)
                    .Where(n => !segments.Any(s => s.DoLinesIntersect(new LineSegment2D(node.Position, n.Position), false)))
                    .Where(n => Vector2.Distance(n.Position, node.Position) < settings.maxJumpDistance)
                    .MinBy(n => Vector2.Distance(n.Position, node.Position));
        }

        private static bool OverMaxSlope(LineSegment2D segment, float maxSlope)
        {
            var slope = segment.Slope;
            if (slope != null)
            {
                return Mathf.Abs(slope.Value) > maxSlope || segment.NormalVector.y < 0.0f;
            }
            return true;
        }
    }
}