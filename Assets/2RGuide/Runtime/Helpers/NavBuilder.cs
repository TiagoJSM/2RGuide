using Assets._2RGuide.Runtime.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    [Serializable]
    public struct NavSegment
    {
        public LineSegment2D segment;
        public float maxHeight;
        public bool oneWayPlatform;
        public NavTag navTag;
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
            if(HasCoincidentSegment(navSegment))
            {
                return;
            }

            _navSegments.Add(navSegment);

            var navSegmentContainingP1 = GetWalkNavSegmentContaining(navSegment.segment.P1);
            var navSegmentContainingP2 = GetWalkNavSegmentContaining(navSegment.segment.P2);

            var n1 = navSegmentContainingP1 ? SplitWalkSegment(navSegment.segment.P1) : _nodeStore.NewNodeOrExisting(navSegment.segment.P1);
            var n2 = navSegmentContainingP2 ? SplitWalkSegment(navSegment.segment.P2) : _nodeStore.NewNodeOrExisting(navSegment.segment.P2);

            switch (navSegment.connectionType)
            {
                case ConnectionType.Walk:
                case ConnectionType.Jump:
                case ConnectionType.OneWayPlatformJump:
                    _nodeStore.ConnectNodes(n1, n2, navSegment.maxHeight, navSegment.connectionType, navSegment.navTag);
                    break;
                case ConnectionType.Drop:
                case ConnectionType.OneWayPlatformDrop:
                    n1.AddConnection(navSegment.connectionType, n2, new LineSegment2D(n1.Position, n2.Position), navSegment.maxHeight, navSegment.navTag);
                    break;
            }
        }

        public Node SplitSegment(NavSegment navSegment, Vector2 point)
        {
            var existingNode = _nodeStore.Get(point);
            if (existingNode != null)
            {
                return existingNode;
            }

            var newNode = _nodeStore.SplitSegmentAt(navSegment.segment, point);
            _navSegments.Remove(navSegment);
            var nav1 = navSegment;
            var nav2 = navSegment;

            nav1.segment = new LineSegment2D(nav1.segment.P1, point);
            nav2.segment = new LineSegment2D(point, nav2.segment.P2);

            _navSegments.Add(nav1);
            _navSegments.Add(nav2);

            return newNode;
        }

        public NavSegment GetNavSegmentWithPoint(Vector2 point)
        {
            return _navSegments.FirstOrDefault(ns => ns.segment.Contains(point));
        }

        private NavSegment GetWalkNavSegmentContaining(Vector2 p)
        {
            return _navSegments.FirstOrDefault(ns =>
                ns.connectionType == ConnectionType.Walk && ns.segment.Contains(p) && !ns.segment.P1.Approximately(p) && !ns.segment.P2.Approximately(p));
        }

        private bool HasCoincidentSegment(NavSegment navSegment)
        {
            return _navSegments.FindIndex(ns => ns.segment.IsCoincident(navSegment.segment)) != -1;
        }

        private Node SplitWalkSegment(Vector2 point)
        {
            var existingNode = _nodeStore.Get(point);
            if (existingNode != null)
            {
                return existingNode;
            }

            var index = _navSegments.FindIndex(ns => ns.connectionType == ConnectionType.Walk && ns.segment.Contains(point));

            if (index == -1)
            {
                return null;
            }

            var navSegment = _navSegments[index];

            var newNode = _nodeStore.SplitSegmentAt(navSegment.segment, point);
            _navSegments.Remove(navSegment);
            var nav1 = navSegment;
            var nav2 = navSegment;

            nav1.segment = new LineSegment2D(nav1.segment.P1, point);
            nav2.segment = new LineSegment2D(point, nav2.segment.P2);

            _navSegments.Add(nav1);
            _navSegments.Add(nav2);

            return newNode;
        }
    }
}