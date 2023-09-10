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
            CollectSegments(world);

            var navResult = NavHelper.Build(world.segments, NodePathSettings, JumpSettings, DropSettings);

            world.nodes = navResult.nodes;
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

        //private void CollectSegments(EdgeCollider2D collider, PathsD paths)
        //{
        //    if(collider.pointCount < 1)
        //    {
        //        return;
        //    }

        //    var p1 = collider.transform.TransformPoint(collider.points[0]);
        //    for (var idx = 1; idx < collider.pointCount; idx++)
        //    {
        //        var p2 = collider.transform.TransformPoint(collider.points[idx]);
        //        buffer.Add(new Segment { p1 = p1, p2 = p2 });
        //        p1 = p2;
        //    }
        //}

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

        private static void AssignSegments(NavWorld world, LineSegment2D[] segments)
        {
            world.segments = segments;
        }

        private static void CollectSegments(NavWorld world)
        {
            var paths = new PathsD();
            var children = world.transform.childCount;
            for (var idx = 0; idx < children; idx++)
            {
                var child = world.transform.GetChild(idx);
                if (child.gameObject.activeInHierarchy)
                {
                    var collider = child.GetComponent<Collider2D>();
                    CollectSegments(collider, paths);
                }
            }
            paths = UnionShapes(paths);
            var segments = ConvertToSegments(paths);
            AssignSegments(world, segments);
        }
    }
}