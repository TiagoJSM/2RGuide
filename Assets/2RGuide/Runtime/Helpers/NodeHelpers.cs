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
        }

        public static void BuildNodes(NodeStore nodeStore, IEnumerable<NavSegment> navSegments)
        {
            foreach (var navSegment in navSegments)
            {
                nodeStore.NewNode(navSegment.segment.P1);
                nodeStore.NewNode(navSegment.segment.P2);
            }

            CreateNodeConnections(navSegments, nodeStore);
        }

        private static void CreateNodeConnections(IEnumerable<NavSegment> navSegments, NodeStore nodeStore)
        {
            foreach (var navSegment in navSegments)
            {
                var n1 = nodeStore.Get(navSegment.segment.P1);
                var n2 = nodeStore.Get(navSegment.segment.P2);

                n1.AddConnection(ConnectionType.Walk, n2, navSegment.segment, navSegment.maxHeight);
                n2.AddConnection(ConnectionType.Walk, n1, navSegment.segment, navSegment.maxHeight);
            }
        }
    }
}