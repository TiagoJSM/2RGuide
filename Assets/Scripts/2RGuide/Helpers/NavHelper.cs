using Assets.Scripts._2RGuide.Math;
using Clipper2Lib;
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

    public struct NavBuildContext
    {
        public PathsD closedPath;
        public LineSegment2D[] segments;
    }

    public static class NavHelper
    {
        public static NavResult Build(NavBuildContext navBuildContext, NodeHelpers.Settings nodePathSettings, JumpsHelper.Settings jumpSettings, DropsHelper.Settings dropSettings)
        {
            var nodeStore = new NodeStore();

            var navSegments = navBuildContext.segments.SelectMany(s =>
                s.DivideSegment(nodePathSettings.segmentDivision, 1.0f, navBuildContext.segments.Except(new LineSegment2D[] { s }))).ToArray();

            NodeHelpers.BuildNodes(nodeStore, navSegments, nodePathSettings);
            var jumps = JumpsHelper.BuildJumps(navBuildContext, nodeStore, navSegments, jumpSettings);
            var drops = DropsHelper.BuildDrops(navBuildContext, nodeStore, navSegments, jumps, dropSettings);

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