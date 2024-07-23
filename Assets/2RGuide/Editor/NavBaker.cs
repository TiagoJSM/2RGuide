using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

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

            _bakingCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(BakePathfindingRoutine());
        }

        public static void BakePathfinding(NavWorld world)
        {
            var colliders = GetColliders(world);
            var navTagBounds = UnityEngine.Object.FindObjectsOfType<NavTagBounds>();
            var navBuildContext = GetNavBuildContext(colliders, navTagBounds);
            var navResult = NavHelper.Build(navBuildContext, JumpSettings, DropSettings);
            world.AssignData(navResult);
        }

        private static IEnumerator BakePathfindingRoutine()
        {
            var navWorld = UnityEngine.Object.FindObjectOfType<NavWorld>();
            var navTagBounds = UnityEngine.Object.FindObjectsOfType<NavTagBounds>();

            var colliders = GetColliders(navWorld);

            var nodePathSettings = NodePathSettings;
            var (segments, oneWayEdgeSegments, closedPath) = GetPathDescription(colliders, navTagBounds, nodePathSettings);
            var navTagBoxBounds = navTagBounds.Select(b => new NavTagBoxBounds(ClipperUtils.MakePath(b.Collider), b.NavTag)).ToArray();
            var jumpSettings = JumpSettings;
            var dropSettings = DropSettings;
            var segmentDivision = nodePathSettings.segmentDivision;
            var segmentMaxHeight = nodePathSettings.segmentMaxHeight;
            var navResultTask = Task.Run(() => 
            { 
                var navBuildContext = GetNavBuildContext(segments, oneWayEdgeSegments, closedPath, navTagBoxBounds, segmentDivision, segmentMaxHeight);
                return NavHelper.Build(navBuildContext, jumpSettings, dropSettings); 
            });

            var progressId = Progress.Start("Baking 2D nav", options: Progress.Options.Indefinite);

            while (!navResultTask.IsCompleted)
            {
                yield return null;
            }

            Progress.Remove(progressId);
            
            if(navResultTask.Exception != null)
            {
                Debug.LogException(navResultTask.Exception);
            }
            else if (navWorld != null)
            {
                navWorld.AssignData(navResultTask.Result);
                EditorUtility.SetDirty(navWorld);
            }

            _bakingCoroutine = null;
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

        private static Collider2D[] GetColliders(NavWorld world)
        {
            return world.gameObject.GetComponentsInChildren<Collider2D>(false).Where(c => c.GetComponent<NavTagBounds>() == null).ToArray();
        }

        private static NavBuildContext GetNavBuildContext(Collider2D[] colliders, NavTagBounds[] navTagBounds)
        {
            var nodePathSettings = NodePathSettings;
            var (segments, oneWayEdgeSegments, closedPath) = GetPathDescription(colliders, navTagBounds, nodePathSettings);
            var navTagBoxBounds = navTagBounds.Select(b => new NavTagBoxBounds(ClipperUtils.MakePath(b.Collider), b.NavTag)).ToArray();
            return GetNavBuildContext(segments, oneWayEdgeSegments, closedPath, navTagBoxBounds, nodePathSettings.segmentDivision, nodePathSettings.segmentMaxHeight);
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

        private static NavBuildContext GetNavBuildContext(IEnumerable<LineSegment2D> segments, IEnumerable<LineSegment2D> oneWayEdgeSegments, PathsD closedPath, NavTagBoxBounds[] navTagBoxBounds, float segmentDivision, float segmentMaxHeight)
        {
            var navSegments = NavHelper.ConvertToNavSegments(segments, segmentDivision, oneWayEdgeSegments, segmentMaxHeight, ConnectionType.Walk, navTagBoxBounds);

            return new NavBuildContext()
            {
                closedPath = closedPath,
                segments = navSegments.ToList(),
            };
        }
    }
}