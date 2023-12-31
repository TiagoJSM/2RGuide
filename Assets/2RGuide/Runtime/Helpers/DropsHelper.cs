﻿using _2RGuide.Helpers;
using _2RGuide.Math;
using _2RGuide;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class DropsHelper
    {
        public struct Settings
        {
            public float maxHeight;
            public float horizontalDistance;
            public float maxSlope;
        }

        public static LineSegment2D[] BuildDrops(
            NavBuildContext navBuildContext, 
            NodeStore nodes, 
            LineSegment2D[] jumps,
            Settings settings)
        {
            var resultSegments = new List<LineSegment2D>();

            foreach (var node in nodes.ToArray())
            {
                var canJumpOrDropToLeftSide = node.CanJumpOrDropToLeftSide(settings.maxSlope);
                if (canJumpOrDropToLeftSide)
                {
                    var originX = node.Position.x - settings.horizontalDistance;
                    var target = FindTargetSegment(navBuildContext, node, navBuildContext.segments, jumps, originX, settings);
                    if (target)
                    {
                        PathBuilderHelper.AddTargetNodeForSegment(target, nodes, navBuildContext.segments, node, ConnectionType.Drop, settings.maxSlope, float.PositiveInfinity);
                        resultSegments.Add(target);
                    }
                }

                var canJumpOrDropToRightSide = node.CanJumpOrDropToRightSide(settings.maxSlope);
                if (canJumpOrDropToRightSide)
                {
                    var originX = node.Position.x + settings.horizontalDistance;
                    var target = FindTargetSegment(navBuildContext, node, navBuildContext.segments, jumps, originX, settings);
                    if (target)
                    {
                        PathBuilderHelper.AddTargetNodeForSegment(target, nodes, navBuildContext.segments, node, ConnectionType.Drop, settings.maxSlope, float.PositiveInfinity);
                        resultSegments.Add(target);
                    }
                }
            }

            GetOneWayPlatformSegments(navBuildContext, nodes, settings, jumps, resultSegments);

            return resultSegments.ToArray();
        }

        //ToDo: Check if doesn't collide with any other collider not part of pathfinding
        private static LineSegment2D FindTargetSegment(NavBuildContext navBuildContext, Node node, IEnumerable<NavSegment> navSegments, LineSegment2D[] jumps, float originX, Settings settings)
        {
            var origin = new Vector2(originX, node.Position.y);

            var navSegment = navSegments.Where(ss =>
            {
                var position = ss.segment.PositionInX(originX);
                if (!position.HasValue)
                {
                    return false;
                }
                if (origin.y < position.Value.y)
                {
                    return false;
                }
                return Vector2.Distance(position.Value, origin) <= settings.maxHeight;
            })
            .MinBy(ss =>
            {
                var position = ss.segment.PositionInX(originX);
                return Vector2.Distance(position.Value, origin);
            });

            if (navSegment)
            {
                var segment = new LineSegment2D(node.Position, navSegment.segment.PositionInX(originX).Value);

                var overlaps = segment.IsSegmentOverlappingTerrain(navBuildContext.closedPath, navSegments);

                if (overlaps)
                {
                    return default;
                }

                if (!jumps.Any(rs => rs.IsCoincident(segment)))
                {
                    return segment;
                }
            }
            return default;
        }

        private static void GetOneWayPlatformSegments(NavBuildContext navBuildContext, NodeStore nodes, Settings settings, LineSegment2D[] jumps, List<LineSegment2D> resultSegments)
        {
            PathBuilderHelper.GetOneWayPlatformSegments(navBuildContext, nodes, Vector2.down, settings.maxHeight, settings.maxSlope, ConnectionType.OneWayPlatformDrop, jumps, resultSegments);
        }
    }
}