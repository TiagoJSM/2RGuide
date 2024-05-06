using _2RGuide;
using _2RGuide.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    [Serializable]
    public struct NavSegment
    {
        public LineSegment2D segment;
        public float maxHeight;
        public bool oneWayPlatform;
        public bool obstacle;
        public bool isBidirectional;
        public ConnectionType connectionType;

        public static implicit operator bool(NavSegment navSegment)
        {
            return navSegment.segment;
        }
    }

    public class NavBuilder
    {
        private List<NavSegment> _navSegments;
        private NodeStore _nodeStore;

        public IEnumerable<NavSegment> NavSegments => _navSegments;

        public NavBuilder(NodeStore nodeStore)
        {
            _nodeStore = nodeStore;
            _navSegments = new List<NavSegment>();
        }

        public void AddNavSegment(NavSegment navSegment)
        {
            _navSegments.Add(navSegment);

            var n1 = _nodeStore.NewNodeOrExisting(navSegment.segment.P1);
            var n2 = _nodeStore.NewNodeOrExisting(navSegment.segment.P2);

            switch (navSegment.connectionType)
            {
                case ConnectionType.Walk:
                case ConnectionType.Jump:
                    if (navSegment.isBidirectional)
                    {
                        _nodeStore.ConnectNodes(n1, n2, navSegment.maxHeight, ConnectionType.Walk, navSegment.obstacle);
                    }
                    else
                    {
                        n1.AddConnection(navSegment.connectionType, n2, new LineSegment2D(n1.Position, n2.Position), navSegment.maxHeight, navSegment.obstacle);
                    }
                    break;
                case ConnectionType.Drop:
                    n1.AddConnection(navSegment.connectionType, n2, new LineSegment2D(n1.Position, n2.Position), navSegment.maxHeight, navSegment.obstacle);
                    break;
            }
        }
    }
}