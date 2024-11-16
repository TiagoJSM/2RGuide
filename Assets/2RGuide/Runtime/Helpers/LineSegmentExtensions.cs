﻿using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class LineSegmentExtensions
    {
        public static bool OverMaxSlope(this LineSegment2D segment, float maxSlope)
        {
            var slope = segment.Slope;
            if (slope != null)
            {
                return Mathf.Abs(slope.Value) > maxSlope || segment.NormalizedNormalVector.y < 0.0f;
            }
            return true;
        }

        public static NavSegment[] DivideSegment(this LineSegment2D segment, float divisionStep, float heightDeviation, IEnumerable<LineSegment2D> segments, float maxHeight, ConnectionType connectionType)
        {
            var result = new List<NavSegment>();

            var splits = segment.DivideSegment(divisionStep, segments, maxHeight, connectionType);

            var index = 0;

            while (index < splits.Length)
            {
                var segmentSplits = splits.Skip(index);
                var referenceHeight = segmentSplits.First().maxHeight;
                var groupedSplits = segmentSplits.TakeWhile((sr, i) =>
                {
                    var segmentBreak = !FloatHelper.NearlyEqual(referenceHeight, sr.maxHeight, heightDeviation);
                    if (!segmentBreak)
                    {
                        referenceHeight = Mathf.Min(referenceHeight, sr.maxHeight);
                    }
                    return !segmentBreak;
                })
                .ToArray();

                var first = groupedSplits.First();
                var last = groupedSplits.Last();

                result.Add(new NavSegment()
                {
                    segment = new LineSegment2D(first.segment.P1, last.segment.P2),
                    maxHeight = referenceHeight,
                    connectionType = connectionType,
                });

                index += groupedSplits.Length;
            }

            return result.ToArray();
        }

        public static NavSegment[] DivideSegment(this LineSegment2D segment, float divisionStep, IEnumerable<LineSegment2D> segments, float maxHeight, ConnectionType connectionType)
        {
            var splits = new List<NavSegment>();
            var divisionPoints = segment.GetDivisionPoints(divisionStep).ToArray();

            var normal = segment.NormalizedNormalVector;
            var p1 = divisionPoints[0];
            var raycastPointStart = RGuideVector2.MoveTowards(p1, divisionPoints.Last(), Constants.RGuideEpsilon);
            var hit = Calculations.Raycast(raycastPointStart, raycastPointStart + normal * maxHeight, segments);
            var p1Height = hit ? hit.Distance : maxHeight;

            var previousHeight = p1Height;

            for(var idx = 1; idx < divisionPoints.Length; idx++)
            {
                var previousPoint = divisionPoints[idx - 1];
                var currentPoint = raycastPointStart = divisionPoints[idx];

                if(idx == (divisionPoints.Length - 1))
                {
                    raycastPointStart = RGuideVector2.MoveTowards(divisionPoints.Last(), divisionPoints[0], Constants.RGuideEpsilon);
                }

                hit = Calculations.Raycast(raycastPointStart, raycastPointStart + normal * maxHeight, segments);
                var currentHeight = hit ? hit.Distance : maxHeight;

                splits.Add(new NavSegment()
                {
                    segment = new LineSegment2D(previousPoint, currentPoint),
                    maxHeight = Mathf.Min(previousHeight, currentHeight),
                    connectionType = connectionType,
                });
                previousHeight = currentHeight;
            }

            return splits.ToArray();
        }

        public static LineSegment2D[] Split(this LineSegment2D segment, params RGuideVector2[] splitPoints)
        {
            var pointsOnSegment =
                splitPoints
                    .Where(p => segment.Contains(p))
                    .OrderBy(p => RGuideVector2.Distance(segment.P1, p));

            var result = new List<LineSegment2D>();

            var p1 = segment.P1;
            var orderedSplitPoints = splitPoints.OrderBy(value => RGuideVector2.Distance(p1, value));

            foreach (var splitPoint in orderedSplitPoints)
            {
                result.Add(new LineSegment2D(p1, splitPoint));
                p1 = splitPoint;
            }

            result.Add(new LineSegment2D(p1, segment.P2));

            return result.ToArray();
        }

        public static RGuideVector2[] GetIntersections(this LineSegment2D segment, LineSegment2D[] segments)
        {
            return segments
                .Select(s => s.GetIntersection(segment, true))
                .Where(v => v.HasValue)
                .Select(v => v.Value).ToArray();
        }

        public static bool IsSegmentOverlappingTerrain(this LineSegment2D segment, PathsD closedPaths)
        {
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
            var res = clipper.Execute(ClipType.Union, FillRule.NonZero, openPath, closedPath);

            if (closedPath.Count == 0)
            {
                return true;
            }

            if (closedPath.Count == 1)
            {
                var clipped = new LineSegment2D(new RGuideVector2((float)closedPath[0][0].x, (float)closedPath[0][0].y), new RGuideVector2((float)closedPath[0][1].x, (float)closedPath[0][1].y));
                return !clipped.IsCoincident(segment);
            }

            return true;
        }

        public static bool IsSegmentOverlappingTerrainRaycast(this LineSegment2D segment, NavBuilder navBuilder)
        {
            var intersectsOtherSegments = 
                navBuilder
                    .WalkNavSegments
                        .Any(ns => segment.GetIntersection(ns.segment, false) != null);

            return intersectsOtherSegments;
        }

        //public static bool IsSegmentOverlappingTerrainRaycast(this LineSegment2D segment, CompositeCollider2D composite)
        //{
        //    var filter = new ContactFilter2D() { useTriggers = false };
        //    var results = new List<RaycastHit2D>();
        //    Physics2D.Linecast(segment.P1.ToVector2(), segment.P2.ToVector2(), filter, results);

        //    var compositeColliderResults = results.Where(hit => hit.collider == composite).ToList();
        //    compositeColliderResults.Sort((a, b) => (int)(a.distance - b.distance));

        //    foreach (var result in results)
        //    {
        //        var point = result.point;
        //        if (!segment.P1.Approximately(new RGuideVector2(point)) && !segment.P2.Approximately(new RGuideVector2(point)))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        public static (IEnumerable<LineSegment2D>, IEnumerable<LineSegment2D>) SplitLineSegment(this LineSegment2D segment, IEnumerable<NavTagBounds> navTags)
        {
            var paths = new PathsD(navTags.Select(o => ClipperUtils.MakePath(o.Collider)));
            var (resultOutsidePath, resultInsidePath) = ClipperUtils.SplitPath(segment, paths);
            return (
                NavHelper.ConvertOpenPathToSegments(resultOutsidePath),
                NavHelper.ConvertOpenPathToSegments(resultInsidePath));
        }

        public static (IEnumerable<LineSegment2D>, IEnumerable<LineSegment2D>) SplitLineSegments(this IEnumerable<LineSegment2D> segments, IEnumerable<NavTagBounds> navTags)
        {
            var resultOutsidePath = new List<LineSegment2D>();
            var resultInsidePath = new List<LineSegment2D>();

            foreach (var segment in segments)
            {
                var splits = segment.SplitLineSegment(navTags);
                resultOutsidePath.AddRange(splits.Item1);
                resultInsidePath.AddRange(splits.Item2);
            }

            return (resultOutsidePath, resultInsidePath);
        }

        private static bool SameDirection(RGuideVector2 s1P1, RGuideVector2 s1P2, LineSegment2D s, RGuideVector2 hitPosition)
        {
            var s2P1 = hitPosition.Approximately(s.P1) ? s.P1 : s.P2;
            var s2P2 = hitPosition.Approximately(s.P1) ? s.P2 : s.P1;

            var s1Dir = (s1P2 - s1P1).normalized;
            var s2Dir = (s2P2 - s2P1).normalized;
            return RGuideVector2.Dot(s1Dir, s2Dir) > 0.0f;
        }

        private static IEnumerable<RGuideVector2> GetDivisionPoints(this LineSegment2D segment, float divisionStep)
        {
            var p1 = segment.P1;
            var p2 = segment.P2;

            while (p1 != p2)
            {
                yield return p1;
                p1 = RGuideVector2.MoveTowards(p1, p2, divisionStep);
            }
            yield return p2;
        }
    }
}