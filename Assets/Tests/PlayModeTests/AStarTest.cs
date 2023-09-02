using System.Collections;
using System.Collections.Generic;
using Assets.Scripts._2RGuide;
using Assets.Scripts._2RGuide.Math;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    public class AStarTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestAStar2Nodes()
        {
            // Use the Assert class to test conditions
            var astar = new AStar();

            var n1 = new Node() { Position = Vector3.zero };
            var n2 = new Node() { Position = Vector2.one };

            n1.Connections.Add(NodeConnection.Walk(n2, new LineSegment2D()));
            n2.Connections.Add(NodeConnection.Walk(n1, new LineSegment2D()));

            var path = astar.Resolve(n1, n2);

            Assert.AreEqual(2, path.Length);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
