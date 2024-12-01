﻿using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Tests.PlayModeTests
{
    public class LineSegment2DTests
    {
        [Test]
        public void CheckIfCollinearLinesIntersect()
        {
            var s1 = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new RGuideVector2(10.0f, 0.0f), new RGuideVector2(20.0f, 0.0f));

            Assert.AreEqual(true, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfNonTouchingLinesIntersect()
        {
            var s1 = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new RGuideVector2(15.0f, 0.0f), new RGuideVector2(20.0f, 0.0f));

            Assert.AreEqual(false, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfLinesIntersectOnEnd()
        {
            var s1 = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(10.0f, 10.0f));
            var s2 = new LineSegment2D(new RGuideVector2(10.0f, 0.0f), new RGuideVector2(10.0f, 10.0f));

            Assert.AreEqual(true, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfCrossingLinesIntersect()
        {
            var s1 = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new RGuideVector2(5.0f, -5.0f), new RGuideVector2(5.0f, 5.0f));

            Assert.AreEqual(true, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void CheckIfNonCrossingLinesIntersect()
        {
            var s1 = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(10.0f, 0.0f));
            var s2 = new LineSegment2D(new RGuideVector2(15.0f, -5.0f), new RGuideVector2(15.0f, 5.0f));

            Assert.AreEqual(false, s1.DoLinesIntersect(s2));
        }

        [Test]
        public void DoNotIntersectLinesOnEnd()
        {
            var s1 = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(10.0f, 10.0f));
            var s2 = new LineSegment2D(new RGuideVector2(10.0f, 0.0f), new RGuideVector2(10.0f, 10.0f));

            Assert.AreEqual(false, s1.DoLinesIntersect(s2, false));
        }

        [Test]
        public void LineIntersectsCircle()
        {
            var ls = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(10.0f, 10.0f));
            var circle = new Circle(new RGuideVector2(5.0f, 5.0f), 1.0f);

            Assert.AreEqual(true, ls.IntersectsCircle(circle));
        }

        [Test]
        public void LineTouchesCircle()
        {
            var ls = new LineSegment2D(new RGuideVector2(5.0f, 0.0f), new RGuideVector2(5.0f, 10.0f));
            var circle = new Circle(new RGuideVector2(0.0f, 5.0f), 5.0f);

            Assert.AreEqual(true, ls.IntersectsCircle(circle));
        }

        [Test]
        public void LineDoesNotTouchCircle()
        {
            var ls = new LineSegment2D(new RGuideVector2(15.0f, 0.0f), new RGuideVector2(15.0f, 10.0f));
            var circle = new Circle(new RGuideVector2(0.0f, 5.0f), 5.0f);

            Assert.AreEqual(false, ls.IntersectsCircle(circle));
        }

        [Test]
        public void LineInsideCircle()
        {
            var ls = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(0.0f, 10.0f));
            var circle = new Circle(new RGuideVector2(0.0f, 0.0f), 50.0f);

            Assert.AreEqual(true, ls.IntersectsCircle(circle));
        }

        [Test]
        public void TestClosestPointOnLine()
        {
            var ls = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(0.0f, 10.0f));
            var p = new RGuideVector2(5.0f, 5.0f);
            var closestPoint = ls.ClosestPointOnLine(p);

            Assert.AreEqual(0.0f, closestPoint.x);
            Assert.AreEqual(5.0f, closestPoint.y);
        }

        [Test]
        public void TestSplitCount()
        {
            var ls = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(15.0f, 0.0f));
            var splits = ls.DivideSegment(5.0f, Array.Empty<LineSegment2D>(), 50.0f, ConnectionType.Walk);

            Assert.AreEqual(3, splits.Length);
        }

        [Test]
        public void TestSplitCountAgainstOthers()
        {
            var ls = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(15.0f, 0.0f));
            var splits =
                ls.DivideSegment(
                    5.0f,
                    0.001f,
                    new LineSegment2D[]
                    {
                        new LineSegment2D(new RGuideVector2(0.0f, 10.0f), new RGuideVector2(4.5f, 8.0f)),
                        new LineSegment2D(new RGuideVector2(11.0f, 6.0f), new RGuideVector2(20.0f, 6.0f))
                    },
                    50.0f,
                    ConnectionType.Walk);

            Assert.AreEqual(3, splits.Length);
            Assert.That(splits[0].maxHeight, Is.EqualTo(10.0f).Within(Constants.RGuideEpsilon));
            Assert.That(splits[1].maxHeight, Is.EqualTo(50.0f).Within(Constants.RGuideEpsilon));
            Assert.That(splits[2].maxHeight, Is.EqualTo(6.0f).Within(Constants.RGuideEpsilon));
        }

        [Test]
        public void TestMultipleContinuousSplitCountAgainstOthers()
        {
            var ls = new LineSegment2D(new RGuideVector2(0.0f, 0.0f), new RGuideVector2(100.0f, 0.0f));
            var splits =
                ls.DivideSegment(
                    5.0f,
                    0.001f,
                    new LineSegment2D[]
                    {
                        new LineSegment2D(new RGuideVector2(0.0f, 10.0f), new RGuideVector2(30.0f, 10.0f)),
                        new LineSegment2D(new RGuideVector2(60.0f, 6.0f), new RGuideVector2(70.0f, 6.0f))
                    },
                    50.0f,
                    ConnectionType.Walk);

            Assert.AreEqual(4, splits.Length);
            Assert.AreEqual(10f, splits[0].maxHeight);
            Assert.AreEqual(50f, splits[1].maxHeight);
            Assert.AreEqual(6f, splits[2].maxHeight);
            Assert.AreEqual(50f, splits[3].maxHeight);
        }


        [Test]
        public void TestSlopeRadians()
        {
            var segment = new LineSegment2D(RGuideVector2.zero, new RGuideVector2(10.0f, 10.0f));

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

            var line = new LineSegment2D(new RGuideVector2(-10.0f, 0.0f), new RGuideVector2(10.0f, 0.0f));

            var (resultOutsidePath, resultInsidePath) = ClipperUtils.SplitPath(line, new PathsD() { closedPath });

            Assert.That(resultOutsidePath.Count, Is.EqualTo(2));
            Assert.That(resultInsidePath.Count, Is.EqualTo(1));

            var outsideSegments = NavHelper.ConvertOpenPathToSegments(resultOutsidePath);
            var insideSegments = NavHelper.ConvertOpenPathToSegments(resultInsidePath);

            Assert.That(outsideSegments[0].P1.Approximately(new RGuideVector2(-10.0f, 0.0f)));
            Assert.That(outsideSegments[0].P2.Approximately(new RGuideVector2(-5.0f, 0.0f)));

            Assert.That(outsideSegments[1].P1.Approximately(new RGuideVector2(5.0f, 0.0f)));
            Assert.That(outsideSegments[1].P2.Approximately(new RGuideVector2(10.0f, 0.0f)));

            Assert.That(insideSegments[0].P1.Approximately(new RGuideVector2(-5.0f, 0.0f)));
            Assert.That(insideSegments[0].P2.Approximately(new RGuideVector2(5.0f, 0.0f)));
        }

        public class ContainsParams
        {
            public LineSegment2D Line { get; }
            public RGuideVector2 Point { get; }
            public bool Result { get; }

            public ContainsParams(LineSegment2D line, RGuideVector2 point, bool result)
            {
                Line = line;
                Point = point;
                Result = result;
            }
        }

        static readonly ContainsParams[] ContainsTestValues = new[]
        {
            new ContainsParams(
                new LineSegment2D(
                    new RGuideVector2(-0.066f, 2.635f),
                    new RGuideVector2(0.434f, 3.0599f)),
               new RGuideVector2(0.114f, 3.0f),
               false),
            new ContainsParams(
                new LineSegment2D(
                    new RGuideVector2(-0.066f, 1.0f),
                    new RGuideVector2(0.434f, 1.0f)),
               new RGuideVector2(0.114f, 1.0f),
               true),
        };

        [Test]
        public void Contains([ValueSource(nameof(ContainsTestValues))] ContainsParams values)
        {
            Assert.AreEqual(values.Result, values.Line.Contains(values.Point));
        }

        public class SplitLineSegmentParams
        {
            public IEnumerable<NavTagBoxBounds> NavTagBoxesBounds { get; }
            public LineSegment2D Line { get; }
            public IEnumerable<LineSegment2D> ResultInsidePath { get; }
            public IEnumerable<LineSegment2D> ResultOutsidePath { get; }

            public SplitLineSegmentParams(
                IEnumerable<NavTagBoxBounds> navTagBoxesBounds, 
                LineSegment2D line, 
                IEnumerable<LineSegment2D> resultInsidePath, 
                IEnumerable<LineSegment2D> resultOutsidePath)
            {
                NavTagBoxesBounds = navTagBoxesBounds;
                Line = line;
                ResultInsidePath = resultInsidePath;
                ResultOutsidePath = resultOutsidePath;
            }
        }

        static readonly SplitLineSegmentParams[] SplitLineSegmentTestValues = new[]
        {
            new SplitLineSegmentParams(
                new[] { new NavTagBoxBounds(new NavTag(), new Polygon(new Box(RGuideVector2.zero, new RGuideVector2(10f)))) },
                new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, 10f)),
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -5f), new RGuideVector2(0f, 5f)),
                },
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, -5f)),
                    new LineSegment2D(new RGuideVector2(0f, 5f), new RGuideVector2(0f, 10f))
                }),
            new SplitLineSegmentParams(
                new [] { new NavTagBoxBounds(new NavTag(), new Polygon(new Box(RGuideVector2.zero, new RGuideVector2(10f)))) },
                new LineSegment2D(new RGuideVector2(20f, -10f), new RGuideVector2(20f, 10f)),
                new LineSegment2D[] {},
                new [] {
                    new LineSegment2D(new RGuideVector2(20f, -10f), new RGuideVector2(20f, 10f)),
                }),
            new SplitLineSegmentParams(
                new[] { new NavTagBoxBounds(new NavTag(), new Polygon(new Box(RGuideVector2.zero, new RGuideVector2(100f)))) },
                new LineSegment2D(new RGuideVector2(20f, -10f), new RGuideVector2(20f, 10f)),
                new [] {
                new LineSegment2D(new RGuideVector2(20f, -10f), new RGuideVector2(20f, 10f)),},
                new LineSegment2D[] {}),
            new SplitLineSegmentParams(
                new[] {
                    new NavTagBoxBounds(new NavTag(), new Polygon(new Box(RGuideVector2.zero, new RGuideVector2(10f)))),
                    new NavTagBoxBounds(new NavTag(), new Polygon(new Box(RGuideVector2.zero, new RGuideVector2(5f))))
                },
                new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, 10f)),
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -5f), new RGuideVector2(0f, 5f)),
                },
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, -5f)),
                    new LineSegment2D(new RGuideVector2(0f, 5f), new RGuideVector2(0f, 10f))
                }),
        };

        [Test]
        public void SplitLineSegment([ValueSource(nameof(SplitLineSegmentTestValues))] SplitLineSegmentParams values)
        {
            var segment = values.Line;
            var splits = segment.SplitLineSegment(values.NavTagBoxesBounds);
            Assert.AreEqual(values.ResultInsidePath, splits.resultInsidePath);
            Assert.AreEqual(values.ResultOutsidePath, splits.resultOutsidePath);
        }
    }
}