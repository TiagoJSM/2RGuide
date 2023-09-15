using _2RGuide.Helpers;
using _2RGuide;
using UnityEngine;
using Clipper2Lib;
using _2RGuide.Math;
using System.Collections.Generic;
using System.Linq;

namespace _2RGuide.Editor
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
                    segmentDivision = instance.SegmentDivision,
                    oneWayPlatformMask = instance.OneWayPlatformMask
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
            var navBuildContext = GetNavBuildContext(colliders, NodePathSettings);

            var navResult = NavHelper.Build(navBuildContext, JumpSettings, DropSettings);

            world.nodes = navResult.nodes;
            world.segments = navResult.segments;
            world.drops = navResult.drops;
            world.jumps = navResult.jumps;
        }

        private static LineSegment2D[] ConvertOpenPathToSegments(PathsD paths)
        {
            var segments = new List<LineSegment2D>();

            foreach (var path in paths)
            {
                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)p2.x, (float)p2.y)));
                    p1 = p2;
                }
            }

            return segments.ToArray();
        }

        private static void CollectSegments(Collider2D collider, ClipperD clipper)
        {
            if (collider is BoxCollider2D box)
            {
                CollectSegments(box, clipper);
            }
            //if (collider is EdgeCollider2D edge)
            //{
            //    CollectSegments(edge, clipper);
            //}
            else if (collider is PolygonCollider2D polygon)
            {
                CollectSegments(polygon, clipper);
            }
            else if (collider is CompositeCollider2D composite)
            {
                CollectSegments(composite, clipper);
            }
        }

        private static void CollectSegments(BoxCollider2D collider, ClipperD clipper)
        {
            var bounds = collider.bounds;

            var shape = Clipper.MakePath(new double[]
                {
                    bounds.min.x, bounds.min.y,
                    bounds.min.x, bounds.max.y,
                    bounds.max.x, bounds.max.y,
                    bounds.max.x, bounds.min.y,
                });
            clipper.AddPath(shape, PathType.Subject);
        }

        private static void CollectSegments(EdgeCollider2D collider, ClipperD clipper)
        {
            var points = new List<double>();
            var p1 = collider.transform.TransformPoint(collider.points[0]);
            points.Add(p1.x);
            points.Add(p1.y);

            for (var idx = 1; idx < collider.pointCount; idx++)
            {
                var p2 = collider.transform.TransformPoint(collider.points[idx]);
                points.Add(p2.x);
                points.Add(p2.y);
            }
            var shape = Clipper.MakePath(points.ToArray());
            clipper.AddPath(shape, PathType.Subject, true);
        }

        private static void CollectSegments(PolygonCollider2D collider, ClipperD clipper)
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
                clipper.AddPath(shape, PathType.Subject);
            }
        }

        private static void CollectSegments(CompositeCollider2D collider, ClipperD clipper)
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
                clipper.AddPath(shape, PathType.Subject);
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

        private static NavBuildContext GetNavBuildContext(Collider2D[] colliders, NodeHelpers.Settings nodePathSettings)
        {
            var paths = new PathsD();
            var clipper = new ClipperD();
            
            foreach (var collider in colliders)
            {
                CollectSegments(collider, clipper);
            }
            
            var closedPath = new PathsD();
            
            var done = clipper.Execute(ClipType.Union, FillRule.NonZero, closedPath);
            
            var closedPathSegments = NavHelper.ConvertClosedPathToSegments(closedPath);

            var otherColliders = colliders.Where(c => c is BoxCollider2D || c is PolygonCollider2D).ToArray();

            // Clipper doesn't intersect paths with lines, so the line segments need to be produced separately
            var edgeSegmentsInfo = colliders.GetEdgeSegments(closedPathSegments, otherColliders, nodePathSettings.oneWayPlatformMask).ToArray();
            var edgeSegments = edgeSegmentsInfo.Select(s => s.Item1).ToArray();

            // Once the edge line segments are produced the segments from polygons need to be split to created all the possible connections
            closedPathSegments =
                closedPathSegments
                    .SelectMany(sp =>
                    {
                        var intersections = sp.GetIntersections(edgeSegments);
                        return sp.Split(intersections);
                    })
                    .ToArray();

            var result = new List<LineSegment2D>();
            result.AddRange(closedPathSegments);
            result.AddRange(edgeSegments);

            var oneWayEdgeSegments = edgeSegmentsInfo.Where(s => s.Item2).Select(s => s.Item1);

            var navSegments = NavHelper.ConvertToNavSegments(result, nodePathSettings.segmentDivision, oneWayEdgeSegments);

            return new NavBuildContext()
            {
                closedPath = closedPath,
                segments = navSegments
            };
        }
    }
}