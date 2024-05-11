using _2RGuide.Helpers;
using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using RTree;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace _2RGuide
{
    //investigate GeometryUtils from https://docs.unity3d.com/Packages/com.unity.reflect@1.0/api/Unity.Labs.Utils.GeometryUtils.html
    public class NavWorld : MonoBehaviour
    {
        private RTree<NavSegmentPoint> _tree;

        [SerializeField]
        private NodeStore _nodeStore;
        [SerializeField]
        private NavSegment[] _segments;
        [SerializeField]
        private LineSegment2D[] _drops;
        [SerializeField]
        private LineSegment2D[] _jumps;
        [SerializeField]
        private NavSegment[] _uniqueSegments;

        public Node[] Nodes => _nodeStore.ToArray();
        public NavSegment[] Segments => _segments;
        public LineSegment2D[] Drops => _drops;
        public LineSegment2D[] Jumps => _jumps;

        public Node GetClosestNodeInSegment(Vector2 position)
        {
            var navSegment = GetClosestNavSegment(position);

            var closestPoint = navSegment.segment.ClosestPointOnLine(position);

            var node1 = _nodeStore.Get(navSegment.segment.P1);
            var node2 = _nodeStore.Get(navSegment.segment.P2);

            return Vector2.Distance(closestPoint, node1.Position) < Vector2.Distance(closestPoint, node2.Position) ? node1 : node2;
        }

        public NavSegment GetClosestNavSegment(Vector2 position)
        {
            var navSegment =
                _uniqueSegments
                    .MinBy(ns =>
                    {
                        var closestPoint = ns.segment.ClosestPointOnLine(position);
                        return Vector2.Distance(closestPoint, position);
                    });

            return navSegment;
        }

        public IEnumerable<NavSegmentPoint> Search(Envelope envelope)
        {
            return _tree.Search(envelope);
        }

        public void AssignData(NavResult navResult)
        {
            _nodeStore = navResult.nodeStore;
            _segments = navResult.segments;
            _drops = navResult.drops;
            _jumps = navResult.jumps;
            _uniqueSegments = navResult.nodeStore.GetUniqueNodeConnections().Select(nc =>
                new NavSegment()
                {
                    maxHeight = nc.MaxHeight,
                    oneWayPlatform = nc.ConnectionType == ConnectionType.OneWayPlatformJump,
                    segment = nc.Segment
                }).ToArray();

            PopulateRTree();
        }

        private void Awake()
        {
            PopulateRTree();
        }

        private void OnDrawGizmos()
        {
            //leave it blank, the gizmo implementation is done in the editor "WorldEditor" class
            //this needs to be here for the option to appear on the "Gizmos" dropdown menu
        }

        private void PopulateRTree()
        {
            if (_segments != null)
            {
                _tree = new RTree<NavSegmentPoint>(maxEntries: 16);
                var points = _segments.Select(s => new NavSegmentPoint(s));
                _tree.BulkLoad(points);
            }
        }
    }
}