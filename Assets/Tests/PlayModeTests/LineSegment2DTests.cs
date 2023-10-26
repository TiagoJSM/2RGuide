using _2RGuide.Helpers;
using _2RGuide.Math;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace _2RGuide.Tests.PlayModeTests
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
            var splits = ls.DivideSegment(5.0f, Array.Empty<LineSegment2D>(), 50.0f);

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
                    50.0f);

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
                    50.0f);

            Assert.AreEqual(4, splits.Length);
            Assert.AreEqual(10.0f, splits[0].maxHeight);
            Assert.AreEqual(50.0f, splits[1].maxHeight);
            Assert.AreEqual(6.0f, splits[2].maxHeight);
            Assert.AreEqual(50.0f, splits[3].maxHeight);
        }

        //[Test]
        //public void TestIntersectionValueBetweenLinesWithSameDirection()
        //{
        //    var ray = new LineSegment2D(new Vector2(7.3f, -3.6f), new Vector2(7.3f, 6.4f));
        //    var s = new LineSegment2D(new Vector2(7.3f, 6.3f), new Vector2(7.3f, 9.2f));
        //    var intersection = ray.GetIntersection(s);

        //    Assert.True(intersection.HasValue);
        //    var comparer = new Vector3EqualityComparer(0.01f);
        //    Assert.That(intersection.Value, Is.EqualTo(new Vector2(7.3f, 6.3f)).Using(comparer));
        //}
    }
}