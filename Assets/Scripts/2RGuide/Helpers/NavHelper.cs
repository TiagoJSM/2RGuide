using Assets.Scripts._2RGuide.Math;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public struct NavResult
    {
        public Node[] nodes;
        public NavSegment[] segments;
        public LineSegment2D[] jumps;
        public LineSegment2D[] drops;
    }

    public static class NavHelper
    {
        public static NavResult Build(LineSegment2D[] segments, NodeHelpers.Settings nodePathSettings, JumpsHelper.Settings jumpSettings, DropsHelper.Settings dropSettings)
        {
            var nodeStore = new NodeStore();

            var navSegments = segments.SelectMany(s =>
                s.Split(nodePathSettings.segmentDivision, 1.0f, segments.Except(new LineSegment2D[] { s }))).ToArray();

            NodeHelpers.BuildNodes(nodeStore, navSegments, nodePathSettings);
            var jumps = JumpsHelper.BuildJumps(nodeStore, navSegments, jumpSettings);
            var drops = DropsHelper.BuildDrops(nodeStore, navSegments, jumps, dropSettings);

            return new NavResult()
            {
                nodes = nodeStore.ToArray(),
                segments = navSegments,
                jumps = jumps,
                drops = drops
            };
        }
    }
}