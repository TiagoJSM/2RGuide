using _2RGuide.Helpers;
using System.Collections;
using UnityEngine;

namespace RTree
{
    public class NavSegmentPoint : ISpatialData
    {
        private NavSegment _navSegment;

        public Envelope Envelope
        {
            get
            {
                var segment = _navSegment.segment;
                return new Envelope(
                    Mathf.Min(segment.P1.x, segment.P2.x),
                    Mathf.Min(segment.P1.y, segment.P2.y),
                    Mathf.Max(segment.P1.x, segment.P2.x),
                    Mathf.Max(segment.P1.y, segment.P2.y));
            }
        }

        public NavSegmentPoint(NavSegment navSegment)
        {
            _navSegment = navSegment;
        }

        public NavSegment NavSegment => _navSegment;
    }
}