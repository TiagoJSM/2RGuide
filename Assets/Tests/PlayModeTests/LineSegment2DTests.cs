﻿using Assets.Scripts._2RGuide.Helpers;
using Assets.Scripts._2RGuide.Math;
using NUnit.Framework;
using System.Collections;
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
    }
}