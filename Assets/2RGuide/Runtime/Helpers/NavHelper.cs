using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assets._2RGuide.Runtime.Helpers
{
    public struct NavResult
    {
        public NodeStore nodeStore;
        public NavSegment[] walkSegments;
        public LineSegment2D[] jumps;
        public LineSegment2D[] drops;
    }

    public struct NavBuildContext
    {
        public PolyTree Polygons { get; }
        public IEnumerable<NavSegment> Segments { get; }

        public NavBuildContext(PolyTree polygons, IEnumerable<NavSegment> segments)
        {
            Polygons = polygons;
            Segments = segments;
        }
    }

    public static class NavHelper
    {
        public static NavResult Build(NavBuildContext navBuildContext, JumpsHelper.Settings jumpSettings, DropsHelper.Settings dropSettings)
        {
            var nodeStore = new NodeStore();

            var navSegments = navBuildContext.Segments;

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
                walkSegments = segments,
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

                var segmentPath = new List<LineSegment2D>();
                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segmentPath.Add(new LineSegment2D(new RGuideVector2((float)p1.x, (float)p1.y), new RGuideVector2((float)p2.x, (float)p2.y)));
                    p1 = p2;
                }
                var start = path[0];
                segmentPath.Add(new LineSegment2D(new RGuideVector2((float)p1.x, (float)p1.y), new RGuideVector2((float)start.x, (float)start.y)));

                segments.AddRange(segmentPath.Merge());
            }

            return segments.ToArray();
        }

        public static IEnumerable<LineSegment2D> ConvertClosedPathToSegments(IEnumerable<RGuideVector2> points)
        {
            var segmentPath = new List<LineSegment2D>();

            var p1 = points.ElementAt(0);
            for (var idx = 1; idx < points.Count(); idx++)
            {
                var p2 = points.ElementAt(idx);
                segmentPath.Add(new LineSegment2D(p1, p2));
                p1 = p2;
            }
            var start = points.ElementAt(0);
            segmentPath.Add(new LineSegment2D(p1, start));

            return segmentPath;
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
                    segments.Add(new LineSegment2D(new RGuideVector2((float)p1.x, (float)p1.y), new RGuideVector2((float)p2.x, (float)p2.y)));
                    p1 = p2;
                }
            }

            return segments.ToArray();
        }

        public static NavSegment[] ConvertToNavSegments(IEnumerable<LineSegment2D> segments, float segmentDivision, IEnumerable<LineSegment2D> edgeSegments, float maxHeight, ConnectionType connectionType, NavTagBoxBounds[] navTagBounds)
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