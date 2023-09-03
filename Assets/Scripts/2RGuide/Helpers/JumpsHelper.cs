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
            public float minJumpDistanceX;
        }

        public static LineSegment2D[] BuildJumps(List<Node> nodes, LineSegment2D[] segments, Settings settings)
        {
            var resultSegments = new List<LineSegment2D>();

            foreach (var node in nodes.ToArray())
            {
                var jumpRadius = new Circle(node.Position, settings.maxJumpDistance);
                var segmentsInRange = segments.Where(s => !OverMaxSlope(s, settings.maxSlope) && s.IntersectsCircle(jumpRadius)).ToArray();

                //var closestPoints = 
                //    segmentsInRange
                //        .Where(s => CutSegmentToTheLeft(s, node.Position.x - 0.5f))
                //        .Select(s => 
                //            s.ClosestPointOnLine(node.Position))
                //        .Where(p => 
                //            !p.Approximately(node.Position));

                if(node.CanJumpToLeftSide(settings.maxSlope))
                {
                    var closestPoints =
                        segmentsInRange
                            .Select(s => CutSegmentToTheLeft(s, node.Position.x - settings.minJumpDistanceX))
                            .Where(s => s)
                            .Select(s =>
                                s.ClosestPointOnLine(node.Position))
                            .Where(p =>
                                !p.Approximately(node.Position))
                            .ToArray();

                    GetJumpSegments(node, closestPoints, segments, resultSegments);
                }

                if (node.CanJumpToRightSide(settings.maxSlope))
                {
                    var closestPoints =
                        segmentsInRange
                            .Select(s => CutSegmentToTheRight(s, node.Position.x + settings.minJumpDistanceX))
                            .Where(s => s)
                            .Select(s =>
                                s.ClosestPointOnLine(node.Position))
                            .Where(p =>
                                !p.Approximately(node.Position))
                            .ToArray();

                    GetJumpSegments(node, closestPoints, segments, resultSegments);
                }
            }

            return resultSegments.ToArray();
        }

        private static void GetJumpSegments(Node node, Vector2[] closestPoints, LineSegment2D[] segments, List<LineSegment2D> resultSegments)
        {
            resultSegments.AddRange(
                closestPoints
                    .Select(p =>
                        new LineSegment2D(node.Position, p))
                    .Where(l =>
                        !segments.Any(s =>
                            !s.OnSegment(l.P2) && s.DoLinesIntersect(l, false)))
            );
        }

        private static LineSegment2D CutSegmentToTheLeft(LineSegment2D segment, float x)
        {
            if(segment.P1.x > x && segment.P2.x > x)
            {
                return new LineSegment2D();
            }

            segment.P1.x = Mathf.Min(x, segment.P1.x);
            segment.P2.x = Mathf.Min(x, segment.P2.x);

            return segment;
        }

        private static LineSegment2D CutSegmentToTheRight(LineSegment2D segment, float x)
        {
            if (segment.P1.x < x && segment.P2.x < x)
            {
                return new LineSegment2D();
            }

            segment.P1.x = Mathf.Max(x, segment.P1.x);
            segment.P2.x = Mathf.Max(x, segment.P2.x);
            return segment;
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