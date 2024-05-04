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

        public static void BuildNodes(NodeStore nodeStore, IEnumerable<NavSegment> navSegments)
        {
            foreach (var navSegment in navSegments)
            {
                var n1 = nodeStore.NewNodeOrExisting(navSegment.segment.P1);
                var n2 = nodeStore.NewNodeOrExisting(navSegment.segment.P2);
                nodeStore.ConnectNodes(n1, n2, navSegment.maxHeight, ConnectionType.Walk, navSegment.obstacle);
            }
        }
    }
}