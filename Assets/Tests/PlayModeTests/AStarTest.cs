using System;
using System.Linq;
using _2RGuide;
using _2RGuide.Helpers;
using _2RGuide.Math;
using Assets._2RGuide.Runtime.Helpers;
using Clipper2Lib;
using NUnit.Framework;
using UnityEngine;

namespace _2RGuide.Tests.PlayModeTests
{
    public class AStarTest
    {
        [Test]
        public void TestAStar2Nodes()
        {
            var store = new NodeStore();

            var n1 = store.NewNode(Vector2.zero);
            var n2 = store.NewNode(Vector2.one);

            n1.AddConnection(ConnectionType.Walk, n2, new LineSegment2D(), float.PositiveInfinity);
            n2.AddConnection(ConnectionType.Walk, n1, new LineSegment2D(), float.PositiveInfinity);

            var path = AStar.Resolve(n1, n2, 0, 90f, ConnectionType.All, float.PositiveInfinity);

            Assert.AreEqual(2, path.Length);
        }

        [Test]
        public void TestAStarConnectedJump()
        {
            var jumpSettings = new JumpsHelper.Settings
            {
                maxJumpHeight = 3.0f,
                maxSlope = 60.0f,
                minJumpDistanceX = 0.5f
            };

            var dropSettings = new DropsHelper.Settings
            {
                horizontalDistance = 0.5f,
                maxSlope = 60.0f,
                maxHeight = 20.0f
            };

            var clipper = new ClipperD();
            var closedPath = new PathsD();

            var shape1 = Clipper.MakePath(new double[]
                {
                    0.0, 3.5,
                    3.0, 3.5,
                    3.0, 2.5,
                    0.0, 2.5,
                });

            var shape2 = Clipper.MakePath(new double[]
                {
                    -1.5, 1.5,
                    1.5, 1.5,
                    1.5, -1.5,
                    -1.5, -1.5,
                });

            clipper.AddPath(shape1, PathType.Subject);
            clipper.AddPath(shape2, PathType.Subject);

            var done = clipper.Execute(ClipType.Union, FillRule.NonZero, closedPath);
            var closedPathSegments = NavHelper.ConvertClosedPathToSegments(closedPath);
            var navSegments = NavHelper.ConvertToNavSegments(closedPathSegments, 1.0f, Array.Empty<LineSegment2D>(), 50.0f);

            var navBuildContext = new NavBuildContext()
            {
                segments = navSegments.ToList(),
                closedPath = closedPath
            };

            var navResult = NavHelper.Build(navBuildContext, jumpSettings, dropSettings);

            var start = navResult.nodeStore.Get(new Vector2(1.5f, -1.5f));
            var end = navResult.nodeStore.Get(new Vector2(0.0f, 3.5f));

            var path = AStar.Resolve(start, end, 0, 90f, ConnectionType.All, float.PositiveInfinity);

            Assert.AreEqual(4, path.Length);
        }

        [Test]
        public void TestAStarLongDistanceDueToHeight()
        {
            var jumpSettings = new JumpsHelper.Settings
            {
                maxJumpHeight = 10.0f,
                maxSlope = 60.0f,
                minJumpDistanceX = 0.5f
            };

            var dropSettings = new DropsHelper.Settings
            {
                horizontalDistance = 0.5f,
                maxSlope = 60.0f,
                maxHeight = 20.0f
            };

            var clipper = new ClipperD();
            var closedPath = new PathsD();

            var shape1 = Clipper.MakePath(new double[]
                {
                    0.0, 0.0,
                    30.0, 0.0,
                    30.0, -10.0,
                    0.0, -10.0,
                });

            var shape2 = Clipper.MakePath(new double[]
                {
                    4.0, 4.0,
                    4.0, 5.0,
                    10.0, 5.0,
                    10.0, 4.0,
                });

            clipper.AddPath(shape1, PathType.Subject);
            clipper.AddPath(shape2, PathType.Subject);

            var done = clipper.Execute(ClipType.Union, FillRule.NonZero, closedPath);
            var closedPathSegments = NavHelper.ConvertClosedPathToSegments(closedPath);
            var navSegments = NavHelper.ConvertToNavSegments(closedPathSegments, 0.5f, Array.Empty<LineSegment2D>(), 50.0f);

            var navBuildContext = new NavBuildContext()
            {
                segments = navSegments.ToList(),
                closedPath = closedPath,
            };

            var navResult = NavHelper.Build(navBuildContext, jumpSettings, dropSettings);

            var start = navResult.nodeStore.Get(new Vector2(0.0f, 0.0f));
            var end = navResult.nodeStore.Get(new Vector2(30.0f, 0.0f));

            var path = AStar.Resolve(start, end, 10, 90f, ConnectionType.All, float.PositiveInfinity);

            Assert.AreEqual(6, path.Length);
        }
    }
}
