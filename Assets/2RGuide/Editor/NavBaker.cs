using _2RGuide.Helpers;
using _2RGuide;
using UnityEngine;
using Clipper2Lib;
using _2RGuide.Math;
using System.Collections.Generic;
using System.Linq;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Editor;

namespace _2RGuide.Editor
{
    public static class NavBaker
    {
        private static NodeHelpers.Settings NodePathSettings
        {
            get
            {
                var instance = Nav2RGuideSettingsRegister.GetOrCreateSettings();
                return new NodeHelpers.Settings()
                {
                    segmentDivision = instance.SegmentDivisionDistance,
                    oneWayPlatformMask = instance.OneWayPlatformMask,
                    segmentMaxHeight = instance.SegmentMaxHeight
                };
            }
        }

        private static JumpsHelper.Settings JumpSettings
        {
            get
            {
                var instance = Nav2RGuideSettingsRegister.GetOrCreateSettings();
                return new JumpsHelper.Settings()
                {
                    maxJumpHeight = instance.MaxJumpHeight,
                    minJumpDistanceX = instance.JumpDropHorizontalDistance,
                    maxSlope = instance.MaxSlope,
                };
            }
        }

        private static DropsHelper.Settings DropSettings
        {
            get
            {
                var instance = Nav2RGuideSettingsRegister.GetOrCreateSettings();
                return new DropsHelper.Settings()
                {
                    maxHeight = instance.MaxDropHeight,
                    horizontalDistance = instance.JumpDropHorizontalDistance,
                    maxSlope = instance.MaxSlope,
                };
            }
        }

        public static void BakePathfinding(NavWorld world)
        {
            var colliders = GetColliders(world);
            var navBuildContext = GetNavBuildContext(colliders, NodePathSettings);

            var navResult = NavHelper.Build(navBuildContext, JumpSettings, DropSettings);

            world.nodeStore = navResult.nodeStore;
            world.segments = navResult.segments;
            world.drops = navResult.drops;
            world.jumps = navResult.jumps;
            world.uniqueSegments = navResult.nodeStore.GetUniqueNodeConnections().Select(nc => 
                new NavSegment() 
                { 
                    maxHeight = nc.MaxHeight,
                    oneWayPlatform = nc.ConnectionType == ConnectionType.OneWayPlatformJump,
                    segment = nc.Segment
                }).ToArray();
        }

        private static void CollectSegments(Collider2D collider, LayerMask oneWayPlatformer, ClipperD clipper)
        {
            if (collider is BoxCollider2D box && !oneWayPlatformer.Includes(box.gameObject))
            {
                CollectSegments(box, clipper);
            }
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
            GetColliders(world.gameObject, colliders);
            return colliders.ToArray();
        }

        private static void GetColliders(GameObject go, List<Collider2D> colliders)
        {
            var children = go.transform.childCount;
            for (var idx = 0; idx < children; idx++)
            {
                var child = go.transform.GetChild(idx);
                if (child.gameObject.activeInHierarchy)
                {
                    var collider = child.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        colliders.Add(collider);
                    }
                    GetColliders(child.gameObject, colliders);
                }
            }
        }

        private static NavBuildContext GetNavBuildContext(Collider2D[] colliders, NodeHelpers.Settings nodePathSettings)
        {
            var paths = new PathsD();
            var clipper = ClipperUtils.ConfiguredClipperD();
            
            foreach (var collider in colliders)
            {
                CollectSegments(collider, nodePathSettings.oneWayPlatformMask, clipper);
            }
            
            var closedPath = new PathsD();
            
            var done = clipper.Execute(ClipType.Union, FillRule.NonZero, closedPath);
            
            var closedPathSegments = NavHelper.ConvertClosedPathToSegments(closedPath);

            var otherColliders = colliders.Where(c => 
                c is BoxCollider2D && !nodePathSettings.oneWayPlatformMask.Includes(c.gameObject) || 
                c is PolygonCollider2D).ToArray();

            // Clipper doesn't intersect paths with lines, so the line segments need to be produced separately
            var edgeSegmentsInfo = colliders.GetEdgeSegments(nodePathSettings.oneWayPlatformMask, closedPath).ToArray();
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

            var navSegments = NavHelper.ConvertToNavSegments(result, nodePathSettings.segmentDivision, oneWayEdgeSegments, NodePathSettings.segmentMaxHeight);

            return new NavBuildContext()
            {
                closedPath = closedPath,
                segments = navSegments.ToList(),
            };
        }
    }
}