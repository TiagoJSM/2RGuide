using Assets.Scripts._2RGuide.Math;
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

        public static implicit operator bool(NavSegment navSegment)
        {
            return navSegment.segment;
        }
    }

    public static class LineSegmentExtensions
    {
        //ToDo: move this to another file (maybe settings?)
        public static readonly float MaxHeight = 100.0f;  
        public static bool OverMaxSlope(this LineSegment2D segment, float maxSlope)
        {
            var slope = segment.Slope;
            if (slope != null)
            {
                return Mathf.Abs(slope.Value) > maxSlope || segment.NormalVector.y < 0.0f;
            }
            return true;
        }

        public static NavSegment[] Split(this LineSegment2D segment, float segmentDivision, float heightDeviation, IEnumerable<LineSegment2D> segments)
        {
            var result = new List<NavSegment>();

            var splits = segment.SplitSegment(segmentDivision, segments);

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

        public static NavSegment[] SplitSegment(this LineSegment2D segment, float segmentDivision, IEnumerable<LineSegment2D> segments)
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
    }
}