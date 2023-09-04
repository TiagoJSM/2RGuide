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
                var segmentsInRange = segments.Where(s => !s.OverMaxSlope(settings.maxSlope) && s.IntersectsCircle(jumpRadius)).ToArray();

                if(node.CanJumpOrDropToLeftSide(settings.maxSlope))
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

                if (node.CanJumpOrDropToRightSide(settings.maxSlope))
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
    }
}