using Assets.Scripts._2RGuide.Math;
using Clipper2Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

        public static LineSegment2D[] BuildJumps(NavBuildContext navBuildContext, NodeStore nodes, NavSegment[] navSegments, Settings settings)
        {
            var resultSegments = new List<LineSegment2D>();

            foreach (var node in nodes.ToArray())
            {
                var jumpRadius = new Circle(node.Position, settings.maxJumpDistance);
                var segmentsInRange = navSegments.Where(ss => !ss.segment.OverMaxSlope(settings.maxSlope) && ss.segment.IntersectsCircle(jumpRadius)).ToArray();

                if(node.CanJumpOrDropToLeftSide(settings.maxSlope))
                {
                    var closestPoints =
                        segmentsInRange
                            .Select(ss => CutSegmentToTheLeft(ss.segment, node.Position.x - settings.minJumpDistanceX))
                            .Where(s => s)
                            .Select(s =>
                                s.ClosestPointOnLine(node.Position))
                            .Where(p =>
                                !p.Approximately(node.Position))
                            .ToArray();

                    GetJumpSegments(navBuildContext, node, closestPoints, nodes, navSegments, settings.maxSlope, resultSegments);
                }

                if (node.CanJumpOrDropToRightSide(settings.maxSlope))
                {
                    var closestPoints =
                        segmentsInRange
                            .Select(ss => CutSegmentToTheRight(ss.segment, node.Position.x + settings.minJumpDistanceX))
                            .Where(s => s)
                            .Select(s =>
                                s.ClosestPointOnLine(node.Position))
                            .Where(p =>
                                !p.Approximately(node.Position))
                            .ToArray();

                    GetJumpSegments(navBuildContext, node, closestPoints, nodes, navSegments, settings.maxSlope, resultSegments);
                }
            }

            return resultSegments.ToArray();
        }

        private static void GetJumpSegments(NavBuildContext navBuildContext, Node node, Vector2[] closestPoints, NodeStore nodes, NavSegment[] navSegments, float maxSlope, List<LineSegment2D> resultSegments)
        {
            var jumpSegments =
                closestPoints
                    .Select(p =>
                        new LineSegment2D(node.Position, p))
                    .Where(l =>
                        !navSegments.Any(ss =>
                            !ss.segment.OnSegment(l.P2) && ss.segment.DoLinesIntersect(l, false)))
                    .Where(s => 
                        !s.IsJumpSegmentOverlappingTerrain(navBuildContext.closedPath));

            foreach (var jumpSegment in jumpSegments)
            {
                PathBuilderHelper.AddTargetNodeForSegment(jumpSegment, nodes, navSegments, node, ConnectionType.Jump, maxSlope, float.PositiveInfinity);
            }

            resultSegments.AddRange(
                jumpSegments.Where(js => !resultSegments.Any(rs => rs.IsCoincident(js)))
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