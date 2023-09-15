using Assets.Scripts._2RGuide.Math;
using Clipper2Lib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

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
        public NavSegment[] segments;
    }

    public static class NavHelper
    {
        public static NavResult Build(NavBuildContext navBuildContext, NodeHelpers.Settings nodePathSettings, JumpsHelper.Settings jumpSettings, DropsHelper.Settings dropSettings)
        {
            var nodeStore = new NodeStore();

            var navSegments = navBuildContext.segments;

            NodeHelpers.BuildNodes(nodeStore, navSegments);
            var jumps = JumpsHelper.BuildJumps(navBuildContext, nodeStore, jumpSettings);
            var drops = DropsHelper.BuildDrops(navBuildContext, nodeStore, jumps, dropSettings);

            return new NavResult()
            {
                nodes = nodeStore.ToArray(),
                segments = navSegments,
                jumps = jumps,
                drops = drops
            };
        }

        public static LineSegment2D[] ConvertClosedPathToSegments(PathsD paths)
        {
            var segments = new List<LineSegment2D>();

            foreach (var path in paths)
            {
                // Clippy paths are created in the reverse order
                path.Reverse();

                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)p2.x, (float)p2.y)));
                    p1 = p2;
                }
                var start = path[0];
                segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)start.x, (float)start.y)));
            }

            return segments.ToArray();
        }

        public static NavSegment[] ConvertToNavSegments(IEnumerable<LineSegment2D> segments, float segmentDivision, IEnumerable<LineSegment2D> edgeSegments)
        {
            return
                segments
                    .SelectMany(s =>
                    {
                        var dividedSegs = s.DivideSegment(segmentDivision, 1.0f, segments.Except(new LineSegment2D[] { s }));
                        for (var idx = 0; idx < dividedSegs.Length; idx++)
                        {
                            dividedSegs[idx].oneWayPlatform = edgeSegments.Contains(s);
                        }
                        return dividedSegs;
                    })
                    .ToArray();
        }
    }
}