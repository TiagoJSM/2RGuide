using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public static class EdgeColliderHelper
    {
        public static IEnumerable<LineSegment2D> GetEdgeSegments(this Collider2D[] colliders, LineSegment2D[] segmentsFromPaths, Collider2D[] otherColliders)
        {
            return
                colliders
                    .Select(c => c as EdgeCollider2D)
                    .Where(c => c != null)
                    .SelectMany(c =>
                        GetSegments(c, segmentsFromPaths, otherColliders));
        }
        private static LineSegment2D[] GetSegments(EdgeCollider2D collider, LineSegment2D[] segments, Collider2D[] colliders)
        {
            var edgeSegments = new List<LineSegment2D>();

            if (collider.pointCount < 1)
            {
                return new LineSegment2D[0];
            }

            var p1 = collider.transform.TransformPoint(collider.points[0]);
            for (var idx = 1; idx < collider.pointCount; idx++)
            {
                var p2 = collider.transform.TransformPoint(collider.points[idx]);
                edgeSegments.Add(new LineSegment2D(p1, p2));
                p1 = p2;
            }

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