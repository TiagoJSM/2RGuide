using Assets.Scripts._2RGuide;
using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

namespace Assets.Tests.PlayModeTests
{
    public class PathHelpersTests
    {
        [Test]
        public void Test2SquaresWith1Drop()
        {
            var segments = new LineSegment2D[]
            {
                // box 1
                new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 0.0f)),
                new LineSegment2D(new Vector2(10.0f, 0.0f), new Vector2(10.0f, 10.0f)),
                new LineSegment2D(new Vector2(10.0f, 10.0f), new Vector2(0.0f, 10.0f)),
                new LineSegment2D(new Vector2(0.0f, 10.0f), new Vector2(0.0f, 0.0f)),

                // box 2
                new LineSegment2D(new Vector2(5.0f, 20.0f), new Vector2(15.0f, 20.0f)),
                new LineSegment2D(new Vector2(15.0f, 20.0f), new Vector2(15.0f, 21.0f)),
                new LineSegment2D(new Vector2(15.0f, 21.0f), new Vector2(5.0f, 21.0f)),
                new LineSegment2D(new Vector2(5.0f, 21.0f), new Vector2(5.0f, 20.0f)),
            };
            var nodes = NodeHelpers.BuildNodes(segments);

            Assert.AreEqual(8, nodes.Count);

            var dropSegments = DropsHelper.BuildDrops(nodes, segments, new DropsHelper.Settings() { maxDropHeight = 20.0f, maxSlope = 60f, horizontalDistance = 0.5f });

            Assert.AreEqual(1, dropSegments.Length);
        }
    }
}