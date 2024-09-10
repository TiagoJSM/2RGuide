using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using RTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime
{
    //investigate GeometryUtils from https://docs.unity3d.com/Packages/com.unity.reflect@1.0/api/Unity.Labs.Utils.GeometryUtils.html
    public class NavWorld : MonoBehaviour
    {
        private RTree<NavSegmentPoint> _navSegmentTree;
        private RTree<NodePoint> _nodeTree;

#if !TWOR_GUIDE_DEBUG
        [HideInInspector]
#endif
        [SerializeField]
        private NodeStore _nodeStore;
#if !TWOR_GUIDE_DEBUG
        [HideInInspector]
#endif
        [SerializeField]
        private NavSegment[] _segments;
#if !TWOR_GUIDE_DEBUG
        [HideInInspector]
#endif
        [SerializeField]
        private LineSegment2D[] _drops;
#if !TWOR_GUIDE_DEBUG
        [HideInInspector]
#endif
        [SerializeField]
        private LineSegment2D[] _jumps;
#if !TWOR_GUIDE_DEBUG
        [HideInInspector]
#endif
        [SerializeField]
        private NavSegment[] _uniqueSegments;

        public Node[] Nodes => _nodeStore.GetNodes();
        public NavSegment[] Segments => _segments;
        public LineSegment2D[] Drops => _drops;
        public LineSegment2D[] Jumps => _jumps;

        public Node GetClosestNode(Vector2 position, float? segmentProximityMaxDistance = null)
        {
            if(_nodeStore.NodeCount == 0)
            {
                return null;
            }

            if(segmentProximityMaxDistance.HasValue)
            {
                var min = position - new Vector2(segmentProximityMaxDistance.Value, segmentProximityMaxDistance.Value);
                var max = position + new Vector2(segmentProximityMaxDistance.Value, segmentProximityMaxDistance.Value);
                var results = SearchNode(new Envelope(min.x, min.y, max.x, max.y));

                if (!results.Any())
                {
                    return null;
                }

                return results.MinBy(n => Vector2.Distance(n.Node.Position, position)).Node;
            }
            else
            {
                return _nodeStore.GetNodes().MinBy(n => Vector2.Distance(n.Position, position));
            }
        }

        public Node GetClosestNodeFromClosestSegment(Vector2 position, float segmentProximityMaxDistance)
        {
            var navSegment = GetClosestNavSegment(position, segmentProximityMaxDistance);
            var closestPoint = navSegment.segment.ClosestPointOnLine(position);

            var node1 = _nodeStore.Get(navSegment.segment.P1);
            var node2 = _nodeStore.Get(navSegment.segment.P2);

            return Vector2.Distance(closestPoint, node1.Position) < Vector2.Distance(closestPoint, node2.Position) ? node1 : node2;
        }

        public NavSegment GetClosestNavSegment(Vector2 position, float? segmentProximityMaxDistance = null)
        {
            if (segmentProximityMaxDistance.HasValue)
            {
                var min = position - new Vector2(segmentProximityMaxDistance.Value, segmentProximityMaxDistance.Value);
                var max = position + new Vector2(segmentProximityMaxDistance.Value, segmentProximityMaxDistance.Value);
                var results = SearchNavSegment(new Envelope(min.x, min.y, max.x, max.y));

                if (!results.Any())
                {
                    return default;
                }

                return results.MinBy(ns =>
                {
                    var closestPoint = ns.NavSegment.segment.ClosestPointOnLine(position);
                    return Vector2.Distance(closestPoint, position);
                }).NavSegment;
            }
            else
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
        }

        public IEnumerable<NavSegmentPoint> SearchNavSegment(Envelope envelope)
        {
            return _navSegmentTree.Search(envelope);
        }

        public IEnumerable<NodePoint> SearchNode(Envelope envelope)
        {
            return _nodeTree.Search(envelope);
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

            PopulateRTrees();
        }

        private void Awake()
        {
            PopulateRTrees();
        }

        private void OnDrawGizmos()
        {
            //leave it blank, the gizmo implementation is done in the editor "WorldEditor" class
            //this needs to be here for the option to appear on the "Gizmos" dropdown menu
        }

        private void PopulateRTrees()
        {
            if (_segments != null)
            {
                _navSegmentTree = new RTree<NavSegmentPoint>(maxEntries: 16);
                var points = _segments.Select(s => new NavSegmentPoint(s));
                _navSegmentTree.BulkLoad(points);
            }
            if(_nodeStore != null)
            {
                _nodeTree = new RTree<NodePoint>(maxEntries: 16);
                var points = _nodeStore.GetNodes().Select(n => new NodePoint(n));
                _nodeTree.BulkLoad(points);
            }
        }
    }
}