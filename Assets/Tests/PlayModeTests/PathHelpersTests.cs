using _2RGuide;
using _2RGuide.Helpers;
using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using Clipper2Lib;
using NUnit.Framework;
using System;
using System.Linq;

namespace _2RGuide.Tests.PlayModeTests
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
            var navSegments = NavHelper.ConvertToNavSegments(closedPathSegments, 1.0f, Array.Empty<LineSegment2D>(), 50.0f, Enumerable.Empty<LineSegment2D>());

            var navBuildContext = new NavBuildContext()
            {
                segments = navSegments.ToList(),
                closedPath = closedPath
            };

            NodeHelpers.BuildNodes(nodes, navSegments);

            Assert.AreEqual(10, nodes.ToArray().Length);

            var dropSegments = DropsHelper.BuildDrops(navBuildContext, nodes, new LineSegment2D[0], new DropsHelper.Settings() { maxHeight = 20.0f, maxSlope = 60f, horizontalDistance = 0.5f });

            Assert.AreEqual(1, dropSegments.Length);
        }
    }
}