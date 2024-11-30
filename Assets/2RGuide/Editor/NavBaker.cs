using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Assets._2RGuide.Runtime.Helpers.EdgeColliderHelper;

namespace Assets._2RGuide.Editor
{
    public static class NavBaker
    {
        private static EditorCoroutine _bakingCoroutine;

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
                    noJumpsTargetTags = instance.NoDropsOrJumpsTargetTags,
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
                    noDropsTargetTags = instance.NoDropsOrJumpsTargetTags,
                };
            }
        }

        public static bool ReadyToBakeInBackground => _bakingCoroutine == null;

        public static void BakePathfindingInBackground()
        {
            if (!ReadyToBakeInBackground)
            {
                return;
            }

            _bakingCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(HandleBakePathfindingRoutineError());
        }

        public static void BakePathfinding(NavWorld world)
        {
            var nodePathSettings = NodePathSettings;
            var composite = ColliderGeneration.GenerateComposite(world.gameObject, nodePathSettings.oneWayPlatformMask);
            var colliders = GetColliders(world.gameObject);
            var navTagBounds = UnityEngine.Object.FindObjectsOfType<NavTagBounds>();
            var (segments, oneWayEdgeSegments, polygons) = GetPathDescription(composite, navTagBounds, nodePathSettings);
            var navTagBoxBounds = navTagBounds.Select(b => new NavTagBoxBounds(b)).ToArray();
            var jumpSettings = JumpSettings;
            var dropSettings = DropSettings;
            var segmentDivision = nodePathSettings.segmentDivision;
            var segmentMaxHeight = nodePathSettings.segmentMaxHeight;
            var navBuildContext = GetNavBuildContext(segments, oneWayEdgeSegments, polygons, navTagBoxBounds, segmentDivision, segmentMaxHeight);

            var navResult = NavHelper.Build(navBuildContext, JumpSettings, DropSettings);
            world.AssignData(navResult);
            ColliderGeneration.DestroyRootComposiveGameObject();
        }

        private static IEnumerator HandleBakePathfindingRoutineError()
        {
            try
            {
                yield return BakePathfindingRoutine();
            }
            finally
            {
                ColliderGeneration.DestroyRootComposiveGameObject();
                _bakingCoroutine = null;
            }
        }

        private static IEnumerator BakePathfindingRoutine()
        {
            var navWorld = UnityEngine.Object.FindObjectOfType<NavWorld>();
            var navTagBounds = UnityEngine.Object.FindObjectsOfType<NavTagBounds>();
            var nodePathSettings = NodePathSettings;

            var composite = ColliderGeneration.GenerateComposite(navWorld.gameObject, nodePathSettings.oneWayPlatformMask);
            
            var (segments, oneWayEdgeSegments, polygons) = GetPathDescription(composite, navTagBounds, nodePathSettings);
            var navTagBoxBounds = navTagBounds.Select(b => new NavTagBoxBounds(b)).ToArray();
            var jumpSettings = JumpSettings;
            var dropSettings = DropSettings;
            var segmentDivision = nodePathSettings.segmentDivision;
            var segmentMaxHeight = nodePathSettings.segmentMaxHeight;
            var navResultTask = Task.Run(() =>
            {
                var navBuildContext = GetNavBuildContext(segments, oneWayEdgeSegments, polygons, navTagBoxBounds, segmentDivision, segmentMaxHeight);
                return NavHelper.Build(navBuildContext, jumpSettings, dropSettings);
            });

            var progressId = Progress.Start("Baking 2D nav", options: Progress.Options.Indefinite);

            while (!navResultTask.IsCompleted)
            {
                yield return null;
            }

            Progress.Remove(progressId);

            if (navResultTask.Exception != null)
            {
                Debug.LogException(navResultTask.Exception);
            }
            else if (navWorld != null)
            {
                navWorld.AssignData(navResultTask.Result);
                EditorUtility.SetDirty(navWorld);
            }
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
            var shape = ClipperUtils.MakePath(collider);
            clipper.AddPath(shape, PathType.Subject);
        }

        private static void CollectSegments(PolygonCollider2D collider, ClipperD clipper)
        {
            var paths = ClipperUtils.MakePaths(collider);
            clipper.AddPaths(paths, PathType.Subject);
        }

        private static void CollectSegments(CompositeCollider2D collider, ClipperD clipper)
        {
            var paths = ClipperUtils.MakePaths(collider);
            clipper.AddPaths(paths, PathType.Subject);
        }

        private static Collider2D[] GetColliders(GameObject root)
        {
            return root.GetComponentsInChildren<Collider2D>(false).Where(c => c.GetComponent<NavTagBounds>() == null).ToArray();
        }

        private static NavBuildContext GetNavBuildContext(Collider2D[] colliders, NavTagBounds[] navTagBounds)
        {
            var nodePathSettings = NodePathSettings;
            var (segments, oneWayEdgeSegments, closedPath) = GetPathDescription(colliders, navTagBounds, nodePathSettings);
            var navTagBoxBounds = navTagBounds.Select(b => new NavTagBoxBounds(b)).ToArray();
            return GetNavBuildContext(segments, oneWayEdgeSegments, new PolyTree(Array.Empty<Polygon>()), navTagBoxBounds, nodePathSettings.segmentDivision, nodePathSettings.segmentMaxHeight);
        }

        private static (IEnumerable<LineSegment2D>, IEnumerable<LineSegment2D>, PolyTree) GetPathDescription(CompositeCollider2D composite, NavTagBounds[] navTagBounds, NodeHelpers.Settings nodePathSettings)
        {
            var closedSegmentPaths = GetClosedSegmentPaths(composite);
            var polyTree = GetPolygons(composite);
            var edgeSegmentInfos = GetEdgeSegments(composite, nodePathSettings.oneWayPlatformMask, closedSegmentPaths, polyTree);

            var edgeSegments = edgeSegmentInfos.Select(s => s.edgeSegment).ToArray();

            closedSegmentPaths =
                closedSegmentPaths
                    .SelectMany(sp =>
                    {
                        var intersections = sp.GetIntersections(edgeSegments);
                        return sp.Split(intersections);
                    })
                    .ToArray();

            var allSegments = closedSegmentPaths.ToList();
            allSegments.AddRange(edgeSegmentInfos.Select(es => es.edgeSegment));

            var oneWaySegments = edgeSegmentInfos.Where(es => es.oneWay).Select(es => es.edgeSegment);

            return (allSegments, oneWaySegments, polyTree);
        }

        private static IEnumerable<LineSegment2D> GetClosedSegmentPaths(CompositeCollider2D composite)
        {
            var segmentPath = new List<LineSegment2D>();

            for (var pathIndex = 0; pathIndex < composite.pathCount; pathIndex++)
            {
                var path = new List<Vector2>();
                composite.GetPath(pathIndex, path);
                path.Reverse();

                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segmentPath.Add(new LineSegment2D(new RGuideVector2(p1), new RGuideVector2(p2)));
                    p1 = p2;
                }
                var start = path[0];
                segmentPath.Add(new LineSegment2D(new RGuideVector2(p1), new RGuideVector2(start)));
            }

            return segmentPath.Merge();
        }

        private static IEnumerable<EdgeSegmentInfo> GetEdgeSegments(
            CompositeCollider2D composite, 
            LayerMask oneWayPlatformMask, 
            IEnumerable<LineSegment2D> closedSegmentPaths,
            PolyTree polygons)
        {
            var colliders = GetColliders(composite.gameObject);
            var edgeSegmentsInfo = colliders.GetEdgeSegments(oneWayPlatformMask).ToArray();
            return edgeSegmentsInfo.SelectMany(es => SeparateFromClosedPaths(es, closedSegmentPaths, polygons));
        }

        private static IEnumerable<EdgeSegmentInfo> SeparateFromClosedPaths(EdgeSegmentInfo edgeSegmentInfo, IEnumerable<LineSegment2D> closedSegmentPaths, PolyTree polygons)
        {
            var separations = new List<EdgeSegmentInfo>();
            var intersections = new List<RGuideVector2>() { edgeSegmentInfo.edgeSegment.P1 };
            intersections.AddRange(
                edgeSegmentInfo.edgeSegment.GetIntersections(closedSegmentPaths));

            intersections.Add(edgeSegmentInfo.edgeSegment.P2);
            intersections = intersections.Distinct().ToList();

            if (intersections.Count == 0)
            {
                return separations;
            }

            var isInsideTerrain = polygons.IsPointInside(intersections[0]);

            for(var startingIndex = isInsideTerrain ? 2 : 1; startingIndex < intersections.Count; startingIndex += 2)
            {
                separations.Add(
                    new EdgeSegmentInfo(
                        new LineSegment2D(
                            intersections[startingIndex - 1], intersections[startingIndex]),
                        edgeSegmentInfo.oneWay));
            }

            return separations;
        }

        private static PolyTree GetPolygons(CompositeCollider2D composite)
        {
            var polygonCollection = new List<Polygon>();

            for (var pathIndex = 0; pathIndex < composite.pathCount; pathIndex++)
            {
                var path = new List<Vector2>();
                composite.GetPath(pathIndex, path);
                path.Reverse();

                polygonCollection.Add(new Polygon(path.Select(p => new RGuideVector2(p))));
            }

            return new PolyTree(polygonCollection);
        }

        private static IEnumerable<LineSegment2D> Merge(this IEnumerable<LineSegment2D> segments)
        {
            var result = new List<LineSegment2D>();
            var currentLineSegment = new LineSegment2D();

            foreach (var segment in segments)
            {
                if (!currentLineSegment)
                {
                    currentLineSegment = segment;
                    continue;
                }

                if (segment.Slope == currentLineSegment.Slope)
                {
                    currentLineSegment = new LineSegment2D(currentLineSegment.P1, segment.P2);
                }
                else
                {
                    result.Add(currentLineSegment);
                    currentLineSegment = segment;
                }
            }

            if (currentLineSegment)
            {
                result.Add(currentLineSegment);
            }

            if (result.Count > 1)
            {
                if (result[0].Slope == result.Last().Slope)
                {
                    result[0] = new LineSegment2D(result.Last().P1, result[0].P2);
                    result.RemoveAt(result.Count - 1);
                }
            }

            return result;
        }

        private static (IEnumerable<LineSegment2D>, IEnumerable<LineSegment2D>, PathsD) GetPathDescription(Collider2D[] colliders, NavTagBounds[] navTagBounds, NodeHelpers.Settings nodePathSettings)
        {
            var clipper = ClipperUtils.ConfiguredClipperD();

            foreach (var collider in colliders)
            {
                CollectSegments(collider, nodePathSettings.oneWayPlatformMask, clipper);
            }

            var closedPath = new PathsD();

            var done = clipper.Execute(ClipType.Union, FillRule.NonZero, closedPath);

            var closedPathSegments = NavHelper.ConvertClosedPathToSegments(closedPath);

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

            var segments = new List<LineSegment2D>();
            segments.AddRange(closedPathSegments);
            segments.AddRange(edgeSegments);

            var splits = segments.SplitLineSegments(navTagBounds);
            segments = new List<LineSegment2D>();
            segments.AddRange(splits.Item1);
            segments.AddRange(splits.Item2);

            var oneWayEdgeSegments = edgeSegmentsInfo.Where(s => s.Item2).Select(s => s.Item1);

            return (segments, oneWayEdgeSegments, closedPath);
        }

        private static NavBuildContext GetNavBuildContext(IEnumerable<LineSegment2D> segments, IEnumerable<LineSegment2D> oneWayEdgeSegments, PolyTree polygons, NavTagBoxBounds[] navTagBoxBounds, float segmentDivision, float segmentMaxHeight)
        {
            var navSegments = NavHelper.ConvertToNavSegments(segments, segmentDivision, oneWayEdgeSegments, segmentMaxHeight, ConnectionType.Walk, navTagBoxBounds);

            return new NavBuildContext()
            {
                polygons = polygons,
                segments = navSegments.ToList(),
            };
        }
    }
}