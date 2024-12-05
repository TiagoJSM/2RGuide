using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
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

        private static Collider2D[] GetColliders(GameObject root)
        {
            return root.GetComponentsInChildren<Collider2D>(false).Where(c => c.GetComponent<NavTagBounds>() == null).ToArray();
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

            var splits = allSegments.SplitLineSegments(navTagBounds.Select(b => new NavTagBoxBounds(b)));
            allSegments = new List<LineSegment2D>();
            allSegments.AddRange(splits.resultOutsidePath);
            allSegments.AddRange(splits.resultInsidePath);

            var oneWaySegments = edgeSegmentInfos.Where(es => es.oneWay).Select(es => es.edgeSegment);

            return (allSegments, oneWaySegments, polyTree);
        }

        private static IEnumerable<LineSegment2D> GetClosedSegmentPaths(CompositeCollider2D composite)
        {
            var segmentPath = new List<LineSegment2D>();

            for (var pathIndex = 0; pathIndex < composite.pathCount; pathIndex++)
            {
                var points = new List<Vector2>();
                composite.GetPath(pathIndex, points);
                points.Reverse();

                var path = NavHelper.ConvertClosedPathToSegments(points.Select(p => new RGuideVector2(p)));
                segmentPath.AddRange(path);
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
            var origin = edgeSegmentInfo.edgeSegment.P1;
            var intersections = new List<RGuideVector2>() { origin };
            intersections.AddRange(
                edgeSegmentInfo.edgeSegment.GetIntersections(closedSegmentPaths));

            intersections.Add(edgeSegmentInfo.edgeSegment.P2);
            intersections = intersections.Distinct().OrderBy(intersection => RGuideVector2.Distance(origin, intersection)).ToList();

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

        private static NavBuildContext GetNavBuildContext(IEnumerable<LineSegment2D> segments, IEnumerable<LineSegment2D> oneWayEdgeSegments, PolyTree polygons, NavTagBoxBounds[] navTagBoxBounds, float segmentDivision, float segmentMaxHeight)
        {
            var navSegments = NavHelper.ConvertToNavSegments(segments, segmentDivision, oneWayEdgeSegments, segmentMaxHeight, ConnectionType.Walk, navTagBoxBounds);

            return new NavBuildContext(polygons, navSegments);
        }
    }
}