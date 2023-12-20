using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using Clipper2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class EdgeColliderHelper
    {
        public static IEnumerable<(LineSegment2D, bool)> GetEdgeSegments(
            this Collider2D[] colliders, 
            LineSegment2D[] segmentsFromPaths, 
            Collider2D[] otherColliders, 
            LayerMask oneWayPlatformMask,
            PathsD closedPaths)
        {
            var linesFromEdgeColliders = GetLineSegmentsFromEdgeColliders(colliders, segmentsFromPaths, otherColliders, oneWayPlatformMask);
            var linesFromBoxColliders = GetLineSegmentsFromBoxColliders(colliders, segmentsFromPaths, otherColliders, oneWayPlatformMask, closedPaths);
            return linesFromEdgeColliders.Concat(linesFromBoxColliders);
        }

        private static IEnumerable<(LineSegment2D, bool)> GetLineSegmentsFromEdgeColliders(Collider2D[] colliders, LineSegment2D[] segmentsFromPaths, Collider2D[] otherColliders, LayerMask oneWayPlatformMask)
        {
            return
                colliders
                    .Select(c => c as EdgeCollider2D)
                    .Where(c => c != null)
                    .SelectMany(c =>
                    {
                        var isOneWay = oneWayPlatformMask.Includes(c.gameObject);
                        return GetSegments(c, segmentsFromPaths, otherColliders).Select(s => (s, isOneWay));
                    });
        }

        private static IEnumerable<(LineSegment2D, bool)> GetLineSegmentsFromBoxColliders(
            Collider2D[] colliders, 
            LineSegment2D[] segmentsFromPaths, 
            Collider2D[] otherColliders, 
            LayerMask oneWayPlatformMask,
            PathsD closedPaths)
        {
            return
                colliders
                    .Where(c => oneWayPlatformMask.Includes(c.gameObject))
                    .Select(c => c as BoxCollider2D)
                    .Where(c => c != null)
                    .SelectMany(c =>
                    {
                        var bounds = c.bounds;
                        var segment = new LineSegment2D(new Vector2(bounds.min.x, bounds.max.y), new Vector2(bounds.max.x, bounds.max.y));
                        var line = Clipper.MakePath(new double[]
                        {
                            segment.P1.x, segment.P1.y,
                            segment.P2.x, segment.P2.y,
                        });

                        var clipper = ClipperUtils.ConfiguredClipperD();
                        clipper.AddPath(line, PathType.Subject, true); // a line is open
                        clipper.AddPaths(closedPaths, PathType.Clip, false); // a polygon is closed
                        var openPath = new PathsD();
                        var closedPath = new PathsD();
                        var res = clipper.Execute(ClipType.Difference, FillRule.NonZero, openPath, closedPath);

                        return NavHelper.ConvertOpenPathToSegments(closedPath).Select(s => (s, true));

                        var segments = new LineSegment2D[]
                        {
                            new LineSegment2D(new Vector2(bounds.min.x, bounds.max.y), new Vector2(bounds.max.x, bounds.max.y))
                        };
                        return SplitSegments(segments, segmentsFromPaths, otherColliders).Select(s => (s, true));
                    });
        }

        private static LineSegment2D[] GetSegments(EdgeCollider2D collider, LineSegment2D[] segments, Collider2D[] colliders)
        {
            var edgeSegments = new List<LineSegment2D>();
            
            if (collider.pointCount < 1)
            {
                return Array.Empty<LineSegment2D>();
            }

            var p1 = collider.transform.TransformPoint(collider.points[0]);
            for (var idx = 1; idx < collider.pointCount; idx++)
            {
                var p2 = collider.transform.TransformPoint(collider.points[idx]);
                edgeSegments.Add(new LineSegment2D(p1, p2));
                p1 = p2;
            }

            return SplitSegments(edgeSegments, segments, colliders);
        }

        private static LineSegment2D[] SplitSegments(IEnumerable<LineSegment2D> edgeSegments, LineSegment2D[] segments, Collider2D[] colliders)
        {
            var splitEdgeSegments =
                edgeSegments
                    .SelectMany(es =>
                    {
                        var intersections = es.GetIntersections(segments);
                        return es.Split(intersections);
                    })
                    .Where(s =>
                        !colliders.Any(c =>
                            c.OverlapPoint(s.P1) && c.OverlapPoint(s.P2)))
                    .ToArray();

            return splitEdgeSegments;
        }

    }
}