using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class JumpsHelper
    {
        public static LineSegment2D[] BuildJumps(NavBuildContext navBuildContext, NodeStore nodes, AirConnectionHelper.Settings settings)
        {
            return AirConnectionHelper.Build(navBuildContext, nodes, new LineSegment2D[0], ConnectionType.Jump, ConnectionType.OneWayPlatformJump, settings);
            //var resultSegments = new List<LineSegment2D>();

            //foreach (var node in nodes.ToArray())
            //{
            //    var jumpRadius = new Circle(node.Position, settings.maxJumpHeight);
            //    var segmentsInRange = navBuildContext.segments.Where(ss => !ss.segment.OverMaxSlope(settings.maxSlope) && ss.segment.IntersectsCircle(jumpRadius)).ToArray();

            //    if(node.CanJumpOrDropToLeftSide(settings.maxSlope))
            //    {
            //        var originX = node.Position.x - settings.horizontalDistance;
            //        var target = FindTargetJumpSegment(navBuildContext, node, navBuildContext.segments, originX, settings);
            //        if (target)
            //        {
            //            PathBuilderHelper.AddTargetNodeForSegment(target, nodes, navBuildContext.segments, node, ConnectionType.Jump, settings.maxSlope, float.PositiveInfinity);
            //            resultSegments.Add(target);
            //        }
            //        //var closestPoints =
            //        //    segmentsInRange
            //        //        .Select(ss => CutSegmentToTheLeft(ss.segment, node.Position.x - settings.minJumpDistanceX))
            //        //        .Where(s => s)
            //        //        .Select(s =>
            //        //            s.ClosestPointOnLine(node.Position))
            //        //        .Where(p =>
            //        //            !p.Approximately(node.Position))
            //        //        .ToArray();

            //        //GetJumpSegments(navBuildContext, node, closestPoints, nodes, navBuildContext.segments, settings.maxSlope, resultSegments);
            //    }

            //    if (node.CanJumpOrDropToRightSide(settings.maxSlope))
            //    {
            //        var originX = node.Position.x + settings.horizontalDistance;
            //        var target = FindTargetJumpSegment(navBuildContext, node, navBuildContext.segments, originX, settings);
            //        if (target)
            //        {
            //            PathBuilderHelper.AddTargetNodeForSegment(target, nodes, navBuildContext.segments, node, ConnectionType.Jump, settings.maxSlope, float.PositiveInfinity);
            //            resultSegments.Add(target);
            //        }
            //    }
            //}

            //GetOneWayPlatformJumpSegments(navBuildContext, nodes, settings, resultSegments);

            //return resultSegments.ToArray();
        }

        //private static void GetJumpSegments(NavBuildContext navBuildContext, Node node, Vector2[] closestPoints, NodeStore nodes, List<NavSegment> navSegments, float maxSlope, List<LineSegment2D> resultSegments)
        //{
        //    var jumpSegments =
        //        closestPoints
        //            .Select(p =>
        //                new LineSegment2D(node.Position, p))
        //            .Where(l =>
        //                !navSegments.Any(ss =>
        //                    !ss.segment.OnSegment(l.P2) && ss.segment.DoLinesIntersect(l, false)))
        //            .Where(s =>
        //            {
        //                if (node.Position.Approximately(new Vector2(27.965f, 4.833f)))
        //                {
        //                    Debug.Log("");
        //                }

        //                var overlaps = !s.IsSegmentOverlappingTerrain(navBuildContext.closedPath, navSegments);
        //                return overlaps;
        //            });

        //    foreach (var jumpSegment in jumpSegments)
        //    {
        //        PathBuilderHelper.AddTargetNodeForSegment(jumpSegment, nodes, navSegments, node, ConnectionType.Jump, maxSlope, float.PositiveInfinity);
        //    }

        //    resultSegments.AddRange(
        //        jumpSegments.Where(js => !resultSegments.Any(rs => rs.IsCoincident(js)))
        //    );
        //}

        //ToDo: Check if doesn't collide with any other collider not part of pathfinding
        //private static LineSegment2D FindTargetJumpSegment(NavBuildContext navBuildContext, Node node, IEnumerable<NavSegment> navSegments, float originX, Settings settings)
        //{
        //    var origin = new Vector2(originX, node.Position.y);

        //    var navSegment = navSegments.Where(ss =>
        //    {
        //        var position = ss.segment.PositionInX(originX);
        //        if (!position.HasValue)
        //        {
        //            return false;
        //        }
        //        if (origin.y < position.Value.y)
        //        {
        //            return false;
        //        }
        //        return Vector2.Distance(position.Value, origin) <= settings.maxJumpHeight;
        //    })
        //    .MinBy(ss =>
        //    {
        //        var position = ss.segment.PositionInX(originX);
        //        return Vector2.Distance(position.Value, origin);
        //    });

        //    if (navSegment)
        //    {
        //        var jumpSegment = new LineSegment2D(node.Position, navSegment.segment.PositionInX(originX).Value);

        //        var overlaps = jumpSegment.IsSegmentOverlappingTerrain(navBuildContext.closedPath, navSegments);

        //        if (overlaps)
        //        {
        //            return default;
        //        }
        //    }
        //    return default;
        //}

        //private static LineSegment2D CutSegmentToTheLeft(LineSegment2D segment, float x)
        //{
        //    if(segment.P1.x > x && segment.P2.x > x)
        //    {
        //        return new LineSegment2D();
        //    }

        //    segment.P1.x = Mathf.Min(x, segment.P1.x);
        //    segment.P2.x = Mathf.Min(x, segment.P2.x);

        //    return segment;
        //}

        //private static LineSegment2D CutSegmentToTheRight(LineSegment2D segment, float x)
        //{
        //    if (segment.P1.x < x && segment.P2.x < x)
        //    {
        //        return new LineSegment2D();
        //    }

        //    segment.P1.x = Mathf.Max(x, segment.P1.x);
        //    segment.P2.x = Mathf.Max(x, segment.P2.x);
        //    return segment;
        //}

        //private static void GetOneWayPlatformJumpSegments(NavBuildContext navBuildContext, NodeStore nodes, Settings settings, List<LineSegment2D> resultSegments)
        //{
        //    PathBuilderHelper.GetOneWayPlatformSegments(navBuildContext, nodes, Vector2.down, settings.maxJumpHeight, settings.maxSlope, ConnectionType.OneWayPlatformJump, new LineSegment2D[0], resultSegments);
        //}
    }
}