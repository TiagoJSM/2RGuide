using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using NUnit.Framework;
using System;
using System.Linq;

namespace Assets.Tests.EditModeTests
{
    public class PathHelpersTests
    {
        [Test]
        public void Test2SquaresWith1Drop()
        {
            var nodes = new NodeStore();

            var clipper = new ClipperD();
            var closedPath = new PathsD();

            var shape1 = Clipper.MakePath(new double[]
                {
                    0.0, 0.0,
                    10.0, 0.0,
                    10.0, 10.0,
                    0.0, 10.0,
                });

            var shape2 = Clipper.MakePath(new double[]
                {
                    5.0f, 20.0,
                    15.0, 20.0,
                    15.0, 21.0,
                    5.0, 21.0,
                });

            clipper.AddPath(shape1, PathType.Subject);
            clipper.AddPath(shape2, PathType.Subject);

            var done = clipper.Execute(ClipType.Union, FillRule.NonZero, closedPath);
            var closedPathSegments = NavHelper.ConvertClosedPathToSegments(closedPath);
            var navSegments = NavHelper.ConvertToNavSegments(closedPathSegments, 1.0f, Array.Empty<LineSegment2D>(), 50.0f, ConnectionType.Walk, Array.Empty<NavTagBoxBounds>());

            var navBuildContext = new NavBuildContext()
            {
                segments = navSegments.ToList(),
                closedPath = closedPath
            };

            var navBuilder = new NavBuilder(nodes);
            NodeHelpers.BuildNodes(navBuilder, navSegments);

            Assert.AreEqual(10, nodes.GetNodes().Length);

            DropsHelper.BuildDrops(navBuildContext, nodes, navBuilder, new LineSegment2D[0], new DropsHelper.Settings() { maxHeight = 20.0f, maxSlope = 60f, horizontalDistance = 0.5f, noDropsTargetTags = Array.Empty<NavTag>() });
            var dropSegments = navBuilder.NavSegments.Where(ns => ns.connectionType == ConnectionType.Drop || ns.connectionType == ConnectionType.OneWayPlatformDrop).Select(ns => ns.segment).ToArray();

            Assert.AreEqual(1, dropSegments.Length);
        }
    }
}