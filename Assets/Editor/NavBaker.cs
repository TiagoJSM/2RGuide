using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Clipper2Lib;
using Assets.Scripts._2RGuide.Math;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Editor
{
    public static class NavBaker
    {
        private static NodeHelpers.Settings NodePathSettings
        {
            get
            {
                var instance = Nav2RGuideSettings.instance;
                return new NodeHelpers.Settings()
                {
                    segmentDivision = instance.SegmentDivision
                };
            }
        }

        private static JumpsHelper.Settings JumpSettings
        {
            get
            {
                var instance = Nav2RGuideSettings.instance;
                return new JumpsHelper.Settings()
                {
                    maxJumpDistance = instance.MaxJumpDistance,
                    maxSlope = instance.MaxSlope,
                    minJumpDistanceX = instance.HorizontalDistance
                };
            }
        }

        private static DropsHelper.Settings DropSettings
        {
            get
            {
                var instance = Nav2RGuideSettings.instance;
                return new DropsHelper.Settings()
                {
                    maxDropHeight = instance.MaxDropHeight,
                    horizontalDistance = instance.HorizontalDistance,
                    maxSlope = instance.MaxSlope
                };
            }
        }

        public static void BakePathfinding(NavWorld world)
        {
            var colliders = GetColliders(world);
            var segments = CollectSegments(colliders);

            var navResult = NavHelper.Build(segments, NodePathSettings, JumpSettings, DropSettings);

            world.nodes = navResult.nodes;
            world.segments = navResult.segments;
            world.drops = navResult.drops;
            world.jumps = navResult.jumps;
        }

        private static PathsD UnionShapes(PathsD shapes)
        {
            var fillrule = FillRule.NonZero;
            return Clipper.Union(shapes, fillrule);
        }

        private static LineSegment2D[] ConvertToSegments(PathsD paths)
        {
            var segments = new List<LineSegment2D>();

            foreach (var path in paths)
            {
                // Clippy paths are created in the reverse order
                path.Reverse();

                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)p2.x, (float)p2.y)));
                    p1 = p2;
                }
                var start = path[0];
                segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)start.x, (float)start.y)));
            }

            return segments.ToArray();
        }

        private static void CollectSegments(Collider2D collider, PathsD paths)
        {
            if (collider is BoxCollider2D box)
            {
                CollectSegments(box, paths);
            }
            //else if(collider is EdgeCollider2D edge)
            //{
            //    CollectSegments(edge, paths);
            //}
            else if (collider is PolygonCollider2D polygon)
            {
                CollectSegments(polygon, paths);
            }
            else if (collider is CompositeCollider2D composite)
            {
                CollectSegments(composite, paths);
            }
        }

        private static void CollectSegments(BoxCollider2D collider, PathsD paths)
        {
            var bounds = collider.bounds;

            var shape = Clipper.MakePath(new double[]
                {
                    bounds.min.x, bounds.min.y,
                    bounds.min.x, bounds.max.y,
                    bounds.max.x, bounds.max.y,
                    bounds.max.x, bounds.min.y,
                });

            paths.Add(shape);
        }

        private static LineSegment2D[] GetSegments(EdgeCollider2D collider, LineSegment2D[] segments, Collider2D[] colliders)
        {
            var edgeSegments = new List<LineSegment2D>();

            if(collider.pointCount < 1)
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

        private static void CollectSegments(PolygonCollider2D collider, PathsD paths)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = collider.GetPath(pathIdx);

                if (path.Length < 1)
                {
                    return;
                }

                var points = path.SelectMany(p =>
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                var shape = Clipper.MakePath(points.ToArray());

                paths.Add(shape);
            }
        }

        private static void CollectSegments(CompositeCollider2D collider, PathsD paths)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = new List<Vector2>();
                var _ = collider.GetPath(pathIdx, path);

                if (path.Count < 1)
                {
                    return;
                }

                var points = path.SelectMany(p =>
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                var shape = Clipper.MakePath(points.ToArray());

                paths.Add(shape);
            }
        }

        private static Collider2D[] GetColliders(NavWorld world)
        {
            var colliders = new List<Collider2D>();

            var children = world.transform.childCount;
            for (var idx = 0; idx < children; idx++)
            {
                var child = world.transform.GetChild(idx);
                if (child.gameObject.activeInHierarchy)
                {
                    var collider = child.GetComponent<Collider2D>();
                    if(collider != null)
                    {
                        colliders.Add(collider);
                    }
                }
            }

            return colliders.ToArray();
        }

        private static LineSegment2D[] CollectSegments(Collider2D[] colliders)
        {
            var paths = new PathsD();

            foreach (var collider in colliders)
            {
                CollectSegments(collider, paths);
            }
            paths = UnionShapes(paths);

            var segmentsFromPaths = ConvertToSegments(paths);

            var otherColliders = colliders.Where(c => c is BoxCollider2D || c is PolygonCollider2D).ToArray();
            var edgeSegments = 
                colliders
                    .Select(c => c as EdgeCollider2D)
                    .Where(c => c != null)
                    .SelectMany(c => 
                        GetSegments(c, segmentsFromPaths, otherColliders));

            var result = new List<LineSegment2D>();
            result.AddRange(segmentsFromPaths);
            result.AddRange(edgeSegments);

            //ToDo: Move this code to a function and split "segmentsFromPaths" if segment from "result" intersect with them

            return result.ToArray();
        }
    }
}