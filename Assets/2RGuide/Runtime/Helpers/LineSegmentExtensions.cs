using Assets._2RGuide.Runtime.Math;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public static RGuideVector2[] GetIntersections(this LineSegment2D segment, IEnumerable<LineSegment2D> segments)
        {
            return segments
                .Select(s => s.GetIntersection(segment, true))
                .Where(v => v.HasValue)
                .Select(v => v.Value).ToArray();
        }

        public static bool IsSegmentOverlappingTerrainRaycast(this LineSegment2D segment, PolyTree polygons, NavBuilder navBuilder)
        {
            // Instead of using the minimum possible unit use the minimum * 10, otherwise due to how the rounding and float differences work the move towards result may not change
            var p1Test = RGuideVector2.MoveTowards(segment.P1, segment.P2, Constants.RGuideEpsilon * 10.0f);
            var p2Test = RGuideVector2.MoveTowards(segment.P2, segment.P1, Constants.RGuideEpsilon * 10.0f);
            if (polygons.IsPointInside(p1Test) || polygons.IsPointInside(p2Test))
            {
                return true;    
            }

            var intersectsOtherSegments = 
                navBuilder
                    .WalkNavSegments
                        .Any(ns => segment.GetIntersection(ns.segment, false) != null);

            return intersectsOtherSegments;
        }

        public static (IEnumerable<LineSegment2D> resultOutsidePath, IEnumerable<LineSegment2D> resultInsidePath) SplitLineSegment(this LineSegment2D segment, IEnumerable<NavTagBoxBounds> navTags)
        {
            var polygons = navTags.Select(o => o.Polygon);
            var intersections = polygons.SelectMany(p => p.Intersections(segment));
            var orderedIntersections = intersections.OrderBy(p => RGuideVector2.Distance(segment.P1, p));
            var segmentsPonts = orderedIntersections.ToList();
            segmentsPonts.Insert(0, segment.P1);
            segmentsPonts.Add(segment.P2);
            segmentsPonts = segmentsPonts.Distinct().ToList();

            var lines = segmentsPonts.ToLines();

            var resultsInsidePath = lines.Where(l => polygons.Any(p => p.IsPointInPolygon(l.HalfPoint))).Merge();
            var resultOutsidePath = lines.Except(resultsInsidePath).Merge();

            return (resultOutsidePath, resultsInsidePath);
        }

        public static (IEnumerable<LineSegment2D> resultOutsidePath, IEnumerable<LineSegment2D> resultInsidePath) SplitLineSegments(this IEnumerable<LineSegment2D> segments, IEnumerable<NavTagBoxBounds> navTags)
        {
            var resultOutsidePath = new List<LineSegment2D>();
            var resultInsidePath = new List<LineSegment2D>();

            foreach (var segment in segments)
            {
                var splits = segment.SplitLineSegment(navTags);
                resultOutsidePath.AddRange(splits.resultOutsidePath);
                resultInsidePath.AddRange(splits.resultInsidePath);
            }

            return (resultOutsidePath, resultInsidePath);
        }

        public static IEnumerable<LineSegment2D> Merge(this IEnumerable<LineSegment2D> segments)
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

                if (segment.Slope == currentLineSegment.Slope && currentLineSegment.P2.Approximately(segment.P1))
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
                if (result[0].Slope == result.Last().Slope && result[0].P1.Approximately(result.Last().P2))
                {
                    result[0] = new LineSegment2D(result.Last().P1, result[0].P2);
                    result.RemoveAt(result.Count - 1);
                }
            }

            return result;
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