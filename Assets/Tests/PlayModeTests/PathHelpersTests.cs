using Assets.Scripts._2RGuide;
using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Tests.PlayModeTests
{
    public class PathHelpersTests
    {
        [Test]
        public void Test2SquaresWith1Drop()
        {
            var settings = new NodeHelpers.Settings() { segmentDivision = 1.0f };
            var nodes = new NodeStore();
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

            var navSegments = segments.SelectMany(s =>
                s.DivideSegment(float.MaxValue, 1.0f, segments.Except(new LineSegment2D[] { s }))).ToArray();

            NodeHelpers.BuildNodes(nodes, navSegments, settings);

            Assert.AreEqual(8, nodes.ToArray().Length);

            var dropSegments = DropsHelper.BuildDrops(nodes, navSegments, new LineSegment2D[0], new DropsHelper.Settings() { maxDropHeight = 20.0f, maxSlope = 60f, horizontalDistance = 0.5f });

            Assert.AreEqual(1, dropSegments.Length);
        }
    }
}