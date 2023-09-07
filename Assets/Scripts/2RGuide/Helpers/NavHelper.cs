using Assets.Scripts._2RGuide.Math;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts._2RGuide.Helpers
{
    public struct NavResult
    {
        public Node[] nodes;
        public LineSegment2D[] jumps;
        public LineSegment2D[] drops;
    }

    public static class NavHelper
    {
        public static NavResult Build(LineSegment2D[] segments, JumpsHelper.Settings jumpSettings, DropsHelper.Settings dropSettings)
        {
            var nodes = NodeHelpers.BuildNodes(segments);
            var jumps = JumpsHelper.BuildJumps(nodes, segments, jumpSettings);
            var drops = DropsHelper.BuildDrops(nodes, segments, jumps, dropSettings);

            return new NavResult()
            {
                nodes = nodes.ToArray(),
                jumps = jumps,
                drops = drops
            };
        }
    }
}