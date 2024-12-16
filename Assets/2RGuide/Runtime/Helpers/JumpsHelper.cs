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
            foreach (var node in nodes.GetNodes())
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
                            .Select(ss => ss.segment.CutSegmentToTheLeft(node.Position.x - settings.minJumpDistanceX))
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
                            .Select(ss => ss.segment.CutSegmentToTheRight(node.Position.x + settings.minJumpDistanceX))
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

        private static void BuildJumpSegments(NavBuildContext navBuildContext, Node node, RGuideVector2[] closestPoints, NavBuilder navBuilder)
        {
            var jumpSegments =
                closestPoints
                    .Select(p =>
                        new LineSegment2D(node.Position, p))
                    .Where(s =>
                    {
                        var overlaps = s.IsSegmentOverlappingTerrainRaycast(navBuildContext.Polygons, navBuilder);
                        return !overlaps;
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

        private static void BuildOneWayPlatformJumpSegments(NavBuildContext navBuildContext, NavBuilder navBuilder, Settings settings)
        {
            PathBuilderHelper.GetOneWayPlatformSegments(navBuildContext, navBuilder, RGuideVector2.down, settings.maxJumpHeight, settings.maxSlope, ConnectionType.OneWayPlatformJump, new LineSegment2D[0]);
        }
    }
}