using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public struct NavResult
    {
        public NodeStore nodeStore;
        public NavSegment[] segments;
        public LineSegment2D[] jumps;
        public LineSegment2D[] drops;
    }

    public struct NavBuildContext
    {
        public PathsD closedPath;
        public List<NavSegment> segments;
    }

    public static class NavHelper
    {
        public static NavResult Build(NavBuildContext navBuildContext, JumpsHelper.Settings jumpSettings, DropsHelper.Settings dropSettings)
        {
            var nodeStore = new NodeStore();

            var navSegments = navBuildContext.segments;

            var builder = new NavBuilder(nodeStore);
            NodeHelpers.BuildNodes(builder, navSegments);
            JumpsHelper.BuildJumps(navBuildContext, nodeStore, builder, jumpSettings);
            var jumps = builder.NavSegments.Where(ns => ns.connectionType == ConnectionType.Jump || ns.connectionType == ConnectionType.OneWayPlatformJump).Select(ns => ns.segment).ToArray();
            DropsHelper.BuildDrops(navBuildContext, nodeStore, builder, jumps, dropSettings);
            var drops = builder.NavSegments.Where(ns => ns.connectionType == ConnectionType.Drop || ns.connectionType == ConnectionType.OneWayPlatformDrop).Select(ns => ns.segment).ToArray();
            var segments = builder.NavSegments.Where(ns => ns.connectionType == ConnectionType.Walk).ToArray();

            return new NavResult()
            {
                nodeStore = nodeStore,
                segments = segments,
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

        public static LineSegment2D[] ConvertOpenPathToSegments(PathsD paths)
        {
            var segments = new List<LineSegment2D>();

            foreach (var path in paths)
            {
                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segments.Add(new LineSegment2D(new Vector2((float)p1.x, (float)p1.y), new Vector2((float)p2.x, (float)p2.y)));
                    p1 = p2;
                }
            }

            return segments.ToArray();
        }

        public static NavSegment[] ConvertToNavSegments(IEnumerable<LineSegment2D> segments, float segmentDivision, IEnumerable<LineSegment2D> edgeSegments, float maxHeight, IEnumerable<LineSegment2D> navTaggedSegments, ConnectionType connectionType, NavTagBounds[] navTagBounds)
        {
            return
                segments
                    .SelectMany(s =>
                    {
                        var dividedSegs = s.DivideSegment(segmentDivision, 1.0f, segments.Except(new LineSegment2D[] { s }), maxHeight, connectionType);
                        for (var idx = 0; idx < dividedSegs.Length; idx++)
                        {
                            var overlappingNavTagBounds = navTagBounds.FirstOrDefault(b => b.Contains(dividedSegs[idx].segment));

                            dividedSegs[idx].oneWayPlatform = edgeSegments.Contains(s);
                            dividedSegs[idx].navTag = overlappingNavTagBounds == null ? null : overlappingNavTagBounds.NavTag;
                        }
                        return dividedSegs;
                    })
                    .ToArray();
        }
    }
}