using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts._2RGuide;
using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    public class AStarTest
    {
        [Test]
        public void TestAStar2Nodes()
        {
            var n1 = new Node() { Position = Vector3.zero };
            var n2 = new Node() { Position = Vector2.one };

            n1.AddConnection(ConnectionType.Walk, n2, new LineSegment2D(), float.PositiveInfinity);
            n2.AddConnection(ConnectionType.Walk, n1, new LineSegment2D(), float.PositiveInfinity);

            var path = AStar.Resolve(n1, n2);

            Assert.AreEqual(2, path.Length);
        }

        [Test]
        public void TestAStarConnectedJump()
        {
            var nodePathSettings = new NodeHelpers.Settings
            {
                segmentDivision = float.MaxValue
            };

            var jumpSettings = new JumpsHelper.Settings
            {
                maxJumpDistance = 3.0f,
                maxSlope = 60.0f,
                minJumpDistanceX = 0.5f
            };

            var dropSettings = new DropsHelper.Settings
            {
                horizontalDistance = 0.5f,
                maxSlope = 60.0f,
                maxDropHeight = 20.0f
            };

            var segments = new LineSegment2D[]
            {
                new LineSegment2D(new Vector2(0.0f, 3.5f), new Vector2(3.0f, 3.5f)),
                new LineSegment2D(new Vector2(3.0f, 3.5f), new Vector2(3.0f, 2.5f)),
                new LineSegment2D(new Vector2(3.0f, 2.5f), new Vector2(0.0f, 2.5f)),
                new LineSegment2D(new Vector2(0.0f, 2.5f), new Vector2(0.0f, 3.5f)),

                new LineSegment2D(new Vector2(-1.5f, 1.5f), new Vector2(1.5f, 1.5f)),
                new LineSegment2D(new Vector2(1.5f, 1.5f), new Vector2(1.5f, -1.5f)),
                new LineSegment2D(new Vector2(1.5f, -1.5f), new Vector2(-1.5f, -1.5f)),
                new LineSegment2D(new Vector2(-1.5f, -1.5f), new Vector2(-1.5f, 1.5f)),
            };

            var navResult = NavHelper.Build(segments, nodePathSettings, jumpSettings, dropSettings);

            var start = navResult.nodes.First(n => n.Position.Approximately(new Vector2(1.5f, -1.5f)));
            var end = navResult.nodes.First(n => n.Position.Approximately(new Vector2(0.0f, 3.5f)));

            var path = AStar.Resolve(navResult.nodes[0], navResult.nodes[4]);

            Assert.AreEqual(3, path.Length);
        }
    }
}
