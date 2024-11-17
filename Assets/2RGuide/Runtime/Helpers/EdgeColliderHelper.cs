using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class EdgeColliderHelper
    {
        public struct EdgeSegmentInfo
        {
            public LineSegment2D edgeSegment;
            public bool oneWay;

            public EdgeSegmentInfo(LineSegment2D edgeSegment, bool oneWay)
            {
                this.edgeSegment = edgeSegment;
                this.oneWay = oneWay;
            }
        }

        public static IEnumerable<EdgeSegmentInfo> GetEdgeSegments(
            this Collider2D[] colliders,
            LayerMask oneWayPlatformMask)
        {
            var linesFromEdgeColliders = GetLineSegmentsFromEdgeColliders(colliders, oneWayPlatformMask);
            var linesFromBoxColliders = GetLineSegmentsFromBoxColliders(colliders, oneWayPlatformMask);
            return linesFromEdgeColliders.Concat(linesFromBoxColliders);
        }

        public static IEnumerable<(LineSegment2D, bool)> GetEdgeSegments(
            this Collider2D[] colliders,
            LayerMask oneWayPlatformMask,
            PathsD closedPaths)
        {
            var linesFromEdgeColliders = GetLineSegmentsFromEdgeColliders(colliders, oneWayPlatformMask, closedPaths);
            var linesFromBoxColliders = GetLineSegmentsFromBoxColliders(colliders, oneWayPlatformMask, closedPaths);
            return linesFromEdgeColliders.Concat(linesFromBoxColliders);
        }

        private static IEnumerable<EdgeSegmentInfo> GetLineSegmentsFromEdgeColliders(
            Collider2D[] colliders,
            LayerMask oneWayPlatformMask)
        {
            return
                colliders
                    .Select(c => c as EdgeCollider2D)
                    .Where(c => c != null)
                    .SelectMany(c =>
                    {
                        var isOneWay = oneWayPlatformMask.Includes(c.gameObject);
                        return GetSegments(c).Select(s => new EdgeSegmentInfo(s, isOneWay));
                    });
        }

        private static IEnumerable<(LineSegment2D, bool)> GetLineSegmentsFromEdgeColliders(
            Collider2D[] colliders,
            LayerMask oneWayPlatformMask,
            PathsD closedPaths)
        {
            return
                colliders
                    .Select(c => c as EdgeCollider2D)
                    .Where(c => c != null)
                    .SelectMany(c =>
                    {
                        var isOneWay = oneWayPlatformMask.Includes(c.gameObject);
                        return GetSegments(c, closedPaths).Select(s => (s, isOneWay));
                    });
        }

        private static IEnumerable<EdgeSegmentInfo> GetLineSegmentsFromBoxColliders(
            Collider2D[] colliders,
            LayerMask oneWayPlatformMask)
        {
            return
                colliders
                    .Where(c => oneWayPlatformMask.Includes(c.gameObject))
                    .Select(c => c as BoxCollider2D)
                    .Where(c => c != null)
                    .Select(c =>
                    {
                        var bounds = new Box(c);
                        var segment = new LineSegment2D(bounds.TopLeft, bounds.TopRight);
                        return new EdgeSegmentInfo(segment, true);
                    });
        }

        private static IEnumerable<(LineSegment2D, bool)> GetLineSegmentsFromBoxColliders(
            Collider2D[] colliders,
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
                        var bounds = new Box(c);
                        var segment = new LineSegment2D(bounds.TopLeft, bounds.TopRight);
                        var resultOpenPath = ClipperUtils.GetSubtractedPathFromClosedPaths(segment, closedPaths);
                        return NavHelper.ConvertOpenPathToSegments(resultOpenPath).Select(s => (s, true));
                    });
        }

        private static LineSegment2D[] GetSegments(EdgeCollider2D collider)
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
                edgeSegments.Add(new LineSegment2D(new RGuideVector2(p1), new RGuideVector2(p2)));
                p1 = p2;
            }

            return edgeSegments.ToArray();
        }

        private static LineSegment2D[] GetSegments(EdgeCollider2D collider, PathsD closedPaths)
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
                edgeSegments.Add(new LineSegment2D(new RGuideVector2(p1), new RGuideVector2(p2)));
                p1 = p2;
            }

            return edgeSegments.SelectMany(s =>
            {
                var resultOpenPath = ClipperUtils.GetSubtractedPathFromClosedPaths(s, closedPaths);
                return NavHelper.ConvertOpenPathToSegments(resultOpenPath);
            })
            .ToArray();
        }
    }
}