using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Assets.Tests.PlayModeTests.Attributes;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    public class AStarTest
    {
        [Test]
        public void TestAStar2Nodes()
        {
            var store = new NodeStore();

            var n1 = store.NewNode(RGuideVector2.zero);
            var n2 = store.NewNode(RGuideVector2.one);

            n1.AddConnection(ConnectionType.Walk, n2, new LineSegment2D(), float.PositiveInfinity, null);
            n2.AddConnection(ConnectionType.Walk, n1, new LineSegment2D(), float.PositiveInfinity, null);

            var path = AStar.Resolve(n1, n2, 0, 91f, ConnectionType.All, float.PositiveInfinity, Array.Empty<NavTag>(), 0.0f, new ConnectionTypeMultipliers());

            Assert.AreEqual(2, path.Length);
        }

        [Test]
        public void TestAStarConnectedJump()
        {
            var jumpSettings = new JumpsHelper.Settings
            {
                maxJumpHeight = 3.0f,
                maxSlope = 60.0f,
                minJumpDistanceX = 0.5f,
                noJumpsTargetTags = Array.Empty<NavTag>(),
            };

            var dropSettings = new DropsHelper.Settings
            {
                horizontalDistance = 0.5f,
                maxSlope = 60.0f,
                maxHeight = 20.0f,
                noDropsTargetTags = Array.Empty<NavTag>(),
            };

            var shape1Points = new[]
                {
                    new RGuideVector2(0.0f, 3.5f),
                    new RGuideVector2(3.0f, 3.5f),
                    new RGuideVector2(3.0f, 2.5f),
                    new RGuideVector2(0.0f, 2.5f),
                };

            var shape2Points = new[]
                {
                    new RGuideVector2(-1.5f, 1.5f),
                    new RGuideVector2(1.5f, 1.5f),
                    new RGuideVector2(1.5f, -1.5f),
                    new RGuideVector2(-1.5f, -1.5f),
                };
            //var closedPathSegments = NavHelper.ConvertClosedPathToSegments(closedPath);
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

            var navResult = NavHelper.Build(navBuildContext, jumpSettings, dropSettings);

            var start = navResult.nodeStore.Get(new RGuideVector2(1.5f, -1.5f));
            var end = navResult.nodeStore.Get(new RGuideVector2(0.0f, 3.5f));

            var path = AStar.Resolve(start, end, 0, 91f, ConnectionType.All, float.PositiveInfinity, Array.Empty<NavTag>(), 0.0f, new ConnectionTypeMultipliers());

            Assert.AreEqual(4, path.Length);
        }

        [Test]
        public void TestAStarLongDistanceDueToHeight()
        {
            var jumpSettings = new JumpsHelper.Settings
            {
                maxJumpHeight = 10.0f,
                maxSlope = 60.0f,
                minJumpDistanceX = 0.5f,
                noJumpsTargetTags = Array.Empty<NavTag>(),
            };

            var dropSettings = new DropsHelper.Settings
            {
                horizontalDistance = 0.5f,
                maxSlope = 60.0f,
                maxHeight = 20.0f,
                noDropsTargetTags = Array.Empty<NavTag>(),
            };

            var shape1Points = new[]
                {
                    new RGuideVector2(0.0f, 0.0f),
                    new RGuideVector2(30.0f, 0.0f),
                    new RGuideVector2(30.0f, -10.0f),
                    new RGuideVector2(0.0f, -10.0f),
                };

            var shape2Points = new[]
                {
                    new RGuideVector2(4.0f, 4.0f),
                    new RGuideVector2(4.0f, 5.0f),
                    new RGuideVector2(10.0f, 5.0f),
                    new RGuideVector2(10.0f, 4.0f),
                };
            
            var shape1Path = NavHelper.ConvertClosedPathToSegments(shape1Points);
            var shape2Path = NavHelper.ConvertClosedPathToSegments(shape2Points);
            var closedPathSegments = shape1Path.ToList();
            closedPathSegments.AddRange(shape2Path);
            var navSegments = NavHelper.ConvertToNavSegments(closedPathSegments, 0.5f, Array.Empty<LineSegment2D>(), 50.0f, ConnectionType.Walk, Array.Empty<NavTagBoxBounds>());
            var polygons = new PolyTree(new[]
            {
                new Polygon(shape1Points),
                new Polygon(shape2Points)
            });
            var navBuildContext = new NavBuildContext(polygons, navSegments); ;

            var navResult = NavHelper.Build(navBuildContext, jumpSettings, dropSettings);

            var start = navResult.nodeStore.Get(new RGuideVector2(0.0f, 0.0f));
            var end = navResult.nodeStore.Get(new RGuideVector2(30.0f, 0.0f));

            var path = AStar.Resolve(start, end, 10, 90f, ConnectionType.All, float.PositiveInfinity, Array.Empty<NavTag>(), 0.0f, new ConnectionTypeMultipliers());

            Assert.AreEqual(6, path.Length);
        }

        [UnityTest, TestScene("VerifyAStarPathWithObstacle")]
        public IEnumerator VerifyAStarPathWithObstacle()
        {
            yield return null;
            var agentGO = GameObject.Find("Agent");
            Assert.That(agentGO, Is.Not.Null);
            var targetGO = GameObject.Find("Target");
            Assert.That(targetGO, Is.Not.Null);

            var navWorld = NavWorldManager.Instance.NavWorld;
            var startN = navWorld.GetClosestNode(new RGuideVector2(agentGO.transform.position), 100.0f);
            var endN = navWorld.GetClosestNode(new RGuideVector2(targetGO.transform.position), 100.0f);
            var nodes = AStar.Resolve(startN, endN, 0, 180f, ConnectionType.Walk, float.PositiveInfinity, Array.Empty<NavTag>(), 0.0f, new ConnectionTypeMultipliers());

            Assert.AreEqual(4, nodes.Length);
        }
    }
}
