using Assets.Scripts._2RGuide.Math;
using Clipper2Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    [Serializable]
    public struct NavSegment
    {
        public LineSegment2D segment;
        public float maxHeight;
        public bool oneWayPlatform;

        public static implicit operator bool(NavSegment navSegment)
        {
            return navSegment.segment;
        }
    }

    public static class LineSegmentExtensions
    {
        //ToDo: move this to another file (maybe settings?)
        public static readonly float MaxHeight = 50.0f;  
        public static bool OverMaxSlope(this LineSegment2D segment, float maxSlope)
        {
            var slope = segment.Slope;
            if (slope != null)
            {
                return Mathf.Abs(slope.Value) > maxSlope || segment.NormalVector.y < 0.0f;
            }
            return true;
        }

        public static NavSegment[] DivideSegment(this LineSegment2D segment, float segmentDivision, float heightDeviation, IEnumerable<LineSegment2D> segments)
        {
            var result = new List<NavSegment>();

            var splits = segment.DivideSegment(segmentDivision, segments);

            var index = 0;
            
            while(index < splits.Length)
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
                    maxHeight = referenceHeight
                });

                index += groupedSplits.Length;
            }

            return result.ToArray();
        }

        public static NavSegment[] DivideSegment(this LineSegment2D segment, float segmentDivision, IEnumerable<LineSegment2D> segments)
        {
            var splits = new List<NavSegment>();

            var p1 = segment.P1;
            var normal = segment.NormalVector.normalized;
            var hit = Calculations.Raycast(p1, p1 + normal * MaxHeight, segments);
            var p1Height = hit ? hit.Distance : MaxHeight;
            var divisionStep = segmentDivision;

            while (p1 != segment.P2)
            {
                var p2 = Vector2.MoveTowards(p1, segment.P2, divisionStep);
                hit = Calculations.Raycast(p2, p2 + normal * MaxHeight, segments);
                var p2Height = hit ? hit.Distance : MaxHeight;

                splits.Add(new NavSegment()
                {
                    segment = new LineSegment2D(p1, p2),
                    maxHeight = Mathf.Min(p1Height, p2Height)
                });
                p1 = p2;
                p1Height = p2Height;
            }

            return splits.ToArray();
        }

        public static LineSegment2D[] Split(this LineSegment2D segment, params Vector2[] splitPoints)
        {
            var pointsOnSegment = 
                splitPoints
                    .Where(p => segment.OnSegment(p))
                    .OrderBy(p => Vector2.Distance(segment.P1, p));

            var result = new List<LineSegment2D>();

            var p1 = segment.P1;

            foreach(var splitPoint in splitPoints)
            {
                result.Add(new LineSegment2D(p1, splitPoint));
                p1 = splitPoint;
            }

            result.Add(new LineSegment2D(p1, segment.P2));

            return result.ToArray();
        }

        public static Vector2[] GetIntersections(this LineSegment2D segment, LineSegment2D[] segments)
        {
            return segments
                .Select(s => s.GetIntersection(segment, false))
                .Where(v => v.HasValue)
                .Select(v => v.Value).ToArray();
        }

        public static bool IsJumpSegmentOverlappingTerrain(this LineSegment2D jumpSegment, PathsD closedPaths)
        {
            var line = Clipper.MakePath(new double[]
                {
                    jumpSegment.P1.x, jumpSegment.P1.y,
                    jumpSegment.P2.x, jumpSegment.P2.y,
                });

            var clipper = new ClipperD();
            clipper.AddPath(line, PathType.Subject, true); // a line is open
            clipper.AddPaths(closedPaths, PathType.Clip, false); // a polygon is closed

            var openPath = new PathsD();
            var closedPath = new PathsD();
            var res = clipper.Execute(ClipType.Union, FillRule.NonZero, openPath, closedPath);
            return closedPath.Count == 0;
        }

        public static LineSegment2D GetSegmentWithPosition(this IEnumerable<LineSegment2D> segments, Vector2 position)
        {
            return segments.FirstOrDefault(s => s.OnSegment(position));
        }
    }
}