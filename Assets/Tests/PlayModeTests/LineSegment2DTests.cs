﻿using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using NUnit.Framework;
using System;
using UnityEngine;

namespace Assets.Tests.PlayModeTests
{
    public class LineSegment2DTests
    {
        [Test]
        public void CheckIfCollinearLinesIntersect()
        {
            var s1 = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new Vector2(10.0f, 0.0f), new Vector2(20.0f, 0.0f));

            Assert.AreEqual(true, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfNonTouchingLinesIntersect()
        {
            var s1 = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new Vector2(15.0f, 0.0f), new Vector2(20.0f, 0.0f));

            Assert.AreEqual(false, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfLinesIntersectOnEnd()
        {
            var s1 = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 10.0f));
            var s2 = new LineSegment2D(new Vector2(10.0f, 0.0f), new Vector2(10.0f, 10.0f));

            Assert.AreEqual(true, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfCrossingLinesIntersect()
        {
            var s1 = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new Vector2(5.0f, -5.0f), new Vector2(5.0f, 5.0f));

            Assert.AreEqual(true, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfNonCrossingLinesIntersect()
        {
            var s1 = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new Vector2(15.0f, -5.0f), new Vector2(15.0f, 5.0f));

            Assert.AreEqual(false, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void DoNotIntersectLinesOnEnd()
        {
            var s1 = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 10.0f));
            var s2 = new LineSegment2D(new Vector2(10.0f, 0.0f), new Vector2(10.0f, 10.0f));

            Assert.AreEqual(false, s1.DoLinesIntersect(s2, false));
        }

        [Test]
        public void LineIntersectsCircle()
        {
            var ls = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 10.0f));
            var circle = new Circle(new Vector2(5.0f, 5.0f), 1.0f);

            Assert.AreEqual(true, ls.IntersectsCircle(circle));
        }

        [Test]
        public void LineTouchesCircle()
        {
            var ls = new LineSegment2D(new Vector2(5.0f, 0.0f), new Vector2(5.0f, 10.0f));
            var circle = new Circle(new Vector2(0.0f, 5.0f), 5.0f);

            Assert.AreEqual(true, ls.IntersectsCircle(circle));
        }

        [Test]
        public void LineDoesNotTouchCircle()
        {
            var ls = new LineSegment2D(new Vector2(15.0f, 0.0f), new Vector2(15.0f, 10.0f));
            var circle = new Circle(new Vector2(0.0f, 5.0f), 5.0f);

            Assert.AreEqual(false, ls.IntersectsCircle(circle));
        }

        [Test]
        public void LineInsideCircle()
        {
            var ls = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 10.0f));
            var circle = new Circle(new Vector2(0.0f, 0.0f), 50.0f);

            Assert.AreEqual(true, ls.IntersectsCircle(circle));
        }

        [Test]
        public void TestClosestPointOnLine()
        {
            var ls = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 10.0f));
            var p = new Vector2(5.0f, 5.0f);
            var closestPoint = ls.ClosestPointOnLine(p);

            Assert.AreEqual(0.0f, closestPoint.x);
            Assert.AreEqual(5.0f, closestPoint.y);
        }

        [Test]
        public void TestSplitCount()
        {
            var ls = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(15.0f, 0.0f));
            var splits = ls.DivideSegment(5.0f, Array.Empty<LineSegment2D>(), 50.0f, ConnectionType.Walk);

            Assert.AreEqual(3, splits.Length);
        }

        [Test]
        public void TestSplitCountAgainstOthers()
        {
            var ls = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(15.0f, 0.0f));
            var splits =
                ls.DivideSegment(
                    5.0f,
                    0.001f,
                    new LineSegment2D[]
                    {
                        new LineSegment2D(new Vector2(0.0f, 10.0f), new Vector2(4.5f, 8.0f)),
                        new LineSegment2D(new Vector2(11.0f, 6.0f), new Vector2(20.0f, 6.0f))
                    },
                    50.0f,
                    ConnectionType.Walk);

            Assert.AreEqual(3, splits.Length);
            Assert.AreEqual(10.0f, splits[0].maxHeight);
            Assert.AreEqual(50.0f, splits[1].maxHeight);
            Assert.AreEqual(6.0f, splits[2].maxHeight);
        }

        [Test]
        public void TestMultipleContinuousSplitCountAgainstOthers()
        {
            var ls = new LineSegment2D(new Vector2(0.0f, 0.0f), new Vector2(100.0f, 0.0f));
            var splits =
                ls.DivideSegment(
                    5.0f,
                    0.001f,
                    new LineSegment2D[]
                    {
                        new LineSegment2D(new Vector2(0.0f, 10.0f), new Vector2(30.0f, 10.0f)),
                        new LineSegment2D(new Vector2(60.0f, 6.0f), new Vector2(70.0f, 6.0f))
                    },
                    50.0f,
                    ConnectionType.Walk);

            Assert.AreEqual(4, splits.Length);
            Assert.AreEqual(10.0f, splits[0].maxHeight);
            Assert.AreEqual(50.0f, splits[1].maxHeight);
            Assert.AreEqual(6.0f, splits[2].maxHeight);
            Assert.AreEqual(50.0f, splits[3].maxHeight);
        }


        [Test]
        public void TestSlopeRadians()
        {
            var segment = new LineSegment2D(Vector2.zero, new Vector2(10.0f, 10.0f));

            Assert.That(segment.SlopeRadians, Is.EqualTo(0.785398f).Within(0.1f));
        }

        [Test]
        public void SplitPath()
        {
            var closedPath = Clipper.MakePath(new double[]
                {
                    -5f, -5f,
                    -5f, 5f,
                    5f, 5f,
                    5f, -5f,
                });

            var line = new LineSegment2D(new Vector2(-10.0f, 0.0f), new Vector2(10.0f, 0.0f));

            var (resultOutsidePath, resultInsidePath) = ClipperUtils.SplitPath(line, new PathsD() { closedPath });

            Assert.That(resultOutsidePath.Count, Is.EqualTo(2));
            Assert.That(resultInsidePath.Count, Is.EqualTo(1));

            var outsideSegments = NavHelper.ConvertOpenPathToSegments(resultOutsidePath);
            var insideSegments = NavHelper.ConvertOpenPathToSegments(resultInsidePath);

            Assert.That(outsideSegments[0].P1.Approximately(new Vector2(-10.0f, 0.0f)));
            Assert.That(outsideSegments[0].P2.Approximately(new Vector2(-5.0f, 0.0f)));

            Assert.That(outsideSegments[1].P1.Approximately(new Vector2(5.0f, 0.0f)));
            Assert.That(outsideSegments[1].P2.Approximately(new Vector2(10.0f, 0.0f)));

            Assert.That(insideSegments[0].P1.Approximately(new Vector2(-5.0f, 0.0f)));
            Assert.That(insideSegments[0].P2.Approximately(new Vector2(5.0f, 0.0f)));
        }
    }
}