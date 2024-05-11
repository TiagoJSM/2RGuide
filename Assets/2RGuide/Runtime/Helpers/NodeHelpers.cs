using Assets._2RGuide.Runtime.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace _2RGuide.Helpers
{
    public static class NodeHelpers
    {
        public struct Settings
        {
            public float segmentDivision;
            public LayerMask oneWayPlatformMask;
            public float segmentMaxHeight;
        }

        public static void BuildNodes(NavBuilder navBuilder, IEnumerable<NavSegment> navSegments)
        {
            foreach (var navSegment in navSegments)
            {
                navBuilder.AddNavSegment(navSegment);
            }
        }
    }
}