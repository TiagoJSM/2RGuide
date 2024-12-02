using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
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

            var shape1Points = new[]
            {
                new RGuideVector2(0f, 0f),
                new RGuideVector2(0f, 10f),
                new RGuideVector2(10f, 10f),
                new RGuideVector2(10f, 0f),
            };
            var shape2Points = new[]
            {
                new RGuideVector2(5f, 20f),
                new RGuideVector2(5f, 21f),
                new RGuideVector2(15f, 21f),
                new RGuideVector2(15f, 20f),
            };
            var shape1Path = NavHelper.ConvertClosedPathToSegments(shape1Points);
            var shape2Path = NavHelper.ConvertClosedPathToSegments(shape2Points);
            var closedPathSegments = shape1Path.ToList();
            closedPathSegments.AddRange(shape2Path);
            var navSegments = NavHelper.ConvertToNavSegments(closedPathSegments, 1.0f, Array.Empty<LineSegment2D>(), 50.0f, ConnectionType.Walk, Array.Empty<NavTagBoxBounds>());
            var polygons = new PolyTree(new[]
            {
                new Polygon(shape1Points),
                new Polygon(shape2Points)
            });
            var navBuildContext = new NavBuildContext(polygons, navSegments);

            var navBuilder = new NavBuilder(nodes);
            NodeHelpers.BuildNodes(navBuilder, navSegments);

            Assert.AreEqual(10, nodes.GetNodes().Length);

            DropsHelper.BuildDrops(navBuildContext, nodes, navBuilder, new LineSegment2D[0], new DropsHelper.Settings() { maxHeight = 20.0f, maxSlope = 60f, horizontalDistance = 0.5f, noDropsTargetTags = Array.Empty<NavTag>() });
            var dropSegments = navBuilder.NavSegments.Where(ns => ns.connectionType == ConnectionType.Drop || ns.connectionType == ConnectionType.OneWayPlatformDrop).Select(ns => ns.segment).ToArray();

            Assert.AreEqual(1, dropSegments.Length);
        }
    }
}