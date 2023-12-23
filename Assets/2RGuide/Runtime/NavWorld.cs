using _2RGuide.Helpers;
using _2RGuide.Math;
using UnityEngine;

namespace _2RGuide
{
    //investigate GeometryUtils from https://docs.unity3d.com/Packages/com.unity.reflect@1.0/api/Unity.Labs.Utils.GeometryUtils.html
    public class NavWorld : MonoBehaviour
    {
        public NodeStore nodeStore;
        public NavSegment[] segments;
        public LineSegment2D[] drops;
        public LineSegment2D[] jumps;
        public NavSegment[] uniqueSegments;

        public Node GetClosestNodeInSegment(Vector2 position)
        {
            var navSegment = GetClosestNavSegment(position);

            var closestPoint = navSegment.segment.ClosestPointOnLine(position);

            var node1 = nodeStore.Get(navSegment.segment.P1);
            var node2 = nodeStore.Get(navSegment.segment.P2);

            return Vector2.Distance(closestPoint, node1.Position) < Vector2.Distance(closestPoint, node2.Position) ? node1 : node2;
        }

        public NavSegment GetClosestNavSegment(Vector2 position)
        {
            var navSegment =
                uniqueSegments
                    .MinBy(ns =>
                    {
                        var closestPoint = ns.segment.ClosestPointOnLine(position);
                        return Vector2.Distance(closestPoint, position);
                    });

            return navSegment;
        }

        private void OnDrawGizmos()
        {
            //leave it blank, the gizmo implementation is done in the editor "WorldEditor" class
            //this needs to be here for the option to appear on the "Gizmos" dropdown menu
        }
    }
}