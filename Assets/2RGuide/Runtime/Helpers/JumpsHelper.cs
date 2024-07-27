using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class JumpsHelper
    {
        public struct Settings
        {
            public float maxJumpHeight;
            public float maxSlope;
            public float minJumpDistanceX;
            public NavTag[] noJumpsTargetTags;
        }

        public static void BuildJumps(NavBuildContext navBuildContext, NodeStore nodes, NavBuilder navBuilder, Settings settings)
        {
            foreach (var node in nodes.ToArray())
            {
                var jumpRadius = new Circle(node.Position, settings.maxJumpHeight);
                var segmentsInRange = 
                    navBuilder.NavSegments.Where(ss => 
                        !ss.segment.OverMaxSlope(settings.maxSlope) && 
                        !settings.noJumpsTargetTags.Contains(ss.navTag) && 
                        ss.segment.IntersectsCircle(jumpRadius) &&
                        ss.connectionType == ConnectionType.Walk)
                    .ToArray();

                if (node.CanJumpOrDropToLeftSide(settings.maxSlope))
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

                    BuildJumpSegments(navBuildContext, node, closestPoints, navBuilder);
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

                    BuildJumpSegments(navBuildContext, node, closestPoints, navBuilder);
                }
            }

            BuildOneWayPlatformJumpSegments(navBuildContext, navBuilder, settings);
        }

        private static void BuildJumpSegments(NavBuildContext navBuildContext, Node node, Vector2[] closestPoints, NavBuilder navBuilder)
        {
            var jumpSegments =
                closestPoints
                    .Select(p =>
                        new LineSegment2D(node.Position, p))
                    .Where(s =>
                    {
                        var overlaps = !s.IsSegmentOverlappingTerrain(navBuildContext.closedPath);
                        return overlaps;
                    })
                    .ToArray();

            foreach (var jumpSegment in jumpSegments)
            {
                var ns = new NavSegment()
                {
                    segment = jumpSegment,
                    maxHeight = float.PositiveInfinity,
                    oneWayPlatform = false,
                    navTag = null,
                    connectionType = ConnectionType.Jump
                };
                navBuilder.AddNavSegment(ns);
            }
        }

        private static LineSegment2D CutSegmentToTheLeft(LineSegment2D segment, float x)
        {
            if (segment.P1.x > x && segment.P2.x > x)
            {
                return new LineSegment2D();
            }

            var result = segment;

            result.P1.x = Mathf.Min(x, segment.P1.x);
            result.P1.y = segment.YWhenXIs(result.P1.x).Value;
            result.P2.x = Mathf.Min(x, segment.P2.x);
            result.P2.y = segment.YWhenXIs(result.P2.x).Value;

            return result;
        }

        private static LineSegment2D CutSegmentToTheRight(LineSegment2D segment, float x)
        {
            if (segment.P1.x < x && segment.P2.x < x)
            {
                return new LineSegment2D();
            }

            var result = segment;

            result.P1.x = Mathf.Max(x, segment.P1.x);
            result.P1.y = segment.YWhenXIs(result.P1.x).Value;
            result.P2.x = Mathf.Max(x, segment.P2.x);
            result.P2.y = segment.YWhenXIs(result.P2.x).Value;

            return result;
        }

        private static void BuildOneWayPlatformJumpSegments(NavBuildContext navBuildContext, NavBuilder navBuilder, Settings settings)
        {
            PathBuilderHelper.GetOneWayPlatformSegments(navBuildContext, navBuilder, Vector2.down, settings.maxJumpHeight, settings.maxSlope, ConnectionType.OneWayPlatformJump, new LineSegment2D[0]);
        }
    }
}