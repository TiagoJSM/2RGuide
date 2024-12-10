using Assets._2RGuide.Runtime;
using Assets._2RGuide.Runtime.Helpers;
using Assets._2RGuide.Runtime.Math;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Tests.PlayModeTests
{
    public class LineSegment2DTests
    {
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

        public class MergeParams
        {
            public IEnumerable<LineSegment2D> Lines { get; }
            public IEnumerable<LineSegment2D> MergeResult { get; }

            public MergeParams(
                IEnumerable<LineSegment2D> lines,
                IEnumerable<LineSegment2D> mergeResult)
            {
                Lines = lines;
                MergeResult = mergeResult;
            }
        }

        static readonly MergeParams[] MergeTestValues = new[]
        {
            new MergeParams(
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, -5f)),
                    new LineSegment2D(new RGuideVector2(0f, -5f), new RGuideVector2(0f, 5f)),
                    new LineSegment2D(new RGuideVector2(0f, 5f), new RGuideVector2(0f, 10f)),
                },
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, 10f))
                }),
            new MergeParams(
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, -5f)),
                    new LineSegment2D(new RGuideVector2(0f, 5f), new RGuideVector2(0f, 10f)),
                },
                new [] {
                    new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, -5f)),
                    new LineSegment2D(new RGuideVector2(0f, 5f), new RGuideVector2(0f, 10f)),
                }),
        };

        [Test]
        public void Merge([ValueSource(nameof(MergeTestValues))] MergeParams values)
        {
            var merged = values.Lines.Merge();
            Assert.AreEqual(values.MergeResult, merged);
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
            var (resultOutsidePath, resultInsidePath) = segment.SplitLineSegment(values.NavTagBoxesBounds);
            Assert.AreEqual(values.ResultInsidePath, resultInsidePath);
            Assert.AreEqual(values.ResultOutsidePath, resultOutsidePath);
        }

        public class CutSegmentParams
        {
            public LineSegment2D Line { get; }
            public float X { get; }
            public LineSegment2D Result { get; }

            public CutSegmentParams(
                LineSegment2D line,
                float x,
                LineSegment2D result)
            {
                Line = line;
                X = x;
                Result = result;
            }
        }

        static readonly CutSegmentParams[] CutSegmentToTheLeftTestValues = new[]
        {
            new CutSegmentParams(
                new LineSegment2D(new RGuideVector2(-10f, 0f), new RGuideVector2(10f, 0f)),
                0f,
                new LineSegment2D(new RGuideVector2(-10f, 0f), new RGuideVector2(0f, 0f))),
            new CutSegmentParams(
                new LineSegment2D(new RGuideVector2(-10f, 0f), new RGuideVector2(10f, 10f)),
                0f,
                new LineSegment2D(new RGuideVector2(-10f, 0f), new RGuideVector2(0f, 5f))),
            new CutSegmentParams(
                new LineSegment2D(new RGuideVector2(-10f, 10f), new RGuideVector2(10f, 0f)),
                0f,
                new LineSegment2D(new RGuideVector2(-10f, 10f), new RGuideVector2(0f, 5f))),
        };

        [Test]
        public void CutSegmentToTheLeft([ValueSource(nameof(CutSegmentToTheLeftTestValues))] CutSegmentParams values)
        {
            var cutSegment = values.Line.CutSegmentToTheLeft(values.X);
            Assert.AreEqual(values.Result, cutSegment);
        }

        static readonly CutSegmentParams[] CutSegmentToTheRightTestValues = new[]
        {
            new CutSegmentParams(
                new LineSegment2D(new RGuideVector2(-10f, 0f), new RGuideVector2(10f, 0f)),
                0f,
                new LineSegment2D(new RGuideVector2(0f, 0f), new RGuideVector2(10f, 0f))),
            new CutSegmentParams(
                new LineSegment2D(new RGuideVector2(-10f, 0f), new RGuideVector2(10f, 10f)),
                0f,
                new LineSegment2D(new RGuideVector2(0f, 5f), new RGuideVector2(10f, 10f))),
            new CutSegmentParams(
                new LineSegment2D(new RGuideVector2(-10f, 10f), new RGuideVector2(10f, 0f)),
                0f,
                new LineSegment2D(new RGuideVector2(0f, 5f), new RGuideVector2(10f, 0f))),
        };

        [Test]
        public void CutSegmentToTheRight([ValueSource(nameof(CutSegmentToTheRightTestValues))] CutSegmentParams values)
        {
            var cutSegment = values.Line.CutSegmentToTheRight(values.X);
            Assert.AreEqual(values.Result, cutSegment);
        }

        public class GetIntersectionParams
        {
            public LineSegment2D Line1 { get; }
            public LineSegment2D Line2 { get; }
            public RGuideVector2? Result { get; }

            public GetIntersectionParams(
                LineSegment2D line1,
                LineSegment2D line2,
                RGuideVector2? result = null)
            {
                Line1 = line1;
                Line2 = line2;
                Result = result;
            }
        }

        static readonly GetIntersectionParams[] GetIntersectionTestValues = new[]
        {
            new GetIntersectionParams(
                new LineSegment2D(new RGuideVector2(-10f, 0f), new RGuideVector2(10f, 0f)),
                new LineSegment2D(new RGuideVector2(0f, -10f), new RGuideVector2(0f, 10f)),
                new RGuideVector2(0f, 0f)),
            new GetIntersectionParams(
                new LineSegment2D(new RGuideVector2(-7.596400f, -4.290000f), new RGuideVector2(-7.596500f, -4.951000f)),
                new LineSegment2D(new RGuideVector2(-7.596400f, -0.250000f), new RGuideVector2(-7.596400f, -3.614800f))),
        };

        [Test]
        public void GetIntersection([ValueSource(nameof(GetIntersectionTestValues))] GetIntersectionParams values)
        {
            var intersection = values.Line1.GetIntersection(values.Line2);
            Assert.AreEqual(values.Result, intersection);
        }
    }
}