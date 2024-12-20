using Assets._2RGuide.Runtime.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    public class Polygon
    {
        private List<RGuideVector2> _polygonVertices;
        private Bounds _bounds;

        public static Polygon Infinite
        {
            get
            {
                var polygon = new Polygon();

                polygon._polygonVertices = new List<RGuideVector2> 
                {
                    new RGuideVector2(float.NegativeInfinity, float.NegativeInfinity),
                    new RGuideVector2(float.NegativeInfinity, float.PositiveInfinity),
                    new RGuideVector2(float.PositiveInfinity, float.PositiveInfinity),
                    new RGuideVector2(float.PositiveInfinity, float.NegativeInfinity),
                };
                polygon._bounds = new Bounds(Vector2.zero, new Vector2(float.PositiveInfinity, float.PositiveInfinity));

                return polygon;
            }
        }

        public bool IsInfinite
        {
            get
            {
                return float.IsPositiveInfinity(_bounds.extents.x) && float.IsPositiveInfinity(_bounds.extents.y);
            }
        }

        private Polygon() { }
        public Polygon(IEnumerable<RGuideVector2> polygonVertices) 
        {
            _polygonVertices = polygonVertices.ToList();

            var maxX = polygonVertices.Max(v => v.x);
            var maxY = polygonVertices.Max(v => v.y);

            var minX = polygonVertices.Min(v => v.x);
            var minY = polygonVertices.Min(v => v.y);

            var center = new Vector2((maxX + minX) / 2, (maxY + minY) / 2);
            var size = new Vector2(maxX - minX, maxY - minY);

            _bounds = new Bounds(center, size);
        }

        public Polygon(Box box)
            :this(new[]
                {
                    box.BottomLeft,
                    box.TopLeft,
                    box.TopRight,
                    box.BottomRight
                })
        {
        }

        public Polygon(BoxCollider2D boxCollider)
            :this(new Box(boxCollider))
        {
        }

        /// <summary>
        /// Determines if the given point is inside the polygon, taken from https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public bool IsPointInPolygon(RGuideVector2 testPoint)
        {
            if (IsInfinite)
            {
                return true;
            }

            if (!_bounds.Contains(testPoint.ToVector2()))
            {
                return false;
            }

            var result = false;
            var j = _polygonVertices.Count - 1;
            for (int i = 0; i < _polygonVertices.Count; i++)
            {
                if (_polygonVertices[i].y < testPoint.y && _polygonVertices[j].y >= testPoint.y ||
                    _polygonVertices[j].y < testPoint.y && _polygonVertices[i].y >= testPoint.y)
                {
                    if (_polygonVertices[i].x + (testPoint.y - _polygonVertices[i].y) /
                       (_polygonVertices[j].y - _polygonVertices[i].y) *
                       (_polygonVertices[j].x - _polygonVertices[i].x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public bool Contains(Polygon other)
        {
            if (IsInfinite)
            {
                return true;
            }

            if (!ContainBounds(other._bounds))
            {
                return false;
            }

            var allVerticesInsidePolygon = other._polygonVertices.All(IsPointInPolygon);

            if (!allVerticesInsidePolygon)
            {
                return false;
            }

            if (SegmentsIntersect(other))
            {
                return false;
            }

            return true;
        }

        public IEnumerable<RGuideVector2> Intersections(LineSegment2D line)
        {
            var polygonVertices = _polygonVertices;
            for (var idx = 0; idx < polygonVertices.Count; idx++)
            {
                var p1 = polygonVertices[idx];
                var p2Idx = idx + 1;
                var p2 = p2Idx >= polygonVertices.Count ? polygonVertices[0] : polygonVertices[p2Idx];
                var otherLine = new LineSegment2D(p1, p2);

                var intersection = line.GetIntersection(otherLine);
                if (intersection.HasValue)
                {
                    yield return intersection.Value;
                }
            }
        }
        private bool SegmentsIntersect(Polygon other)
        {
            for (var idx = 0; idx < _polygonVertices.Count; idx++)
            {
                var p1 = _polygonVertices[idx];
                var p2Idx = idx + 1;
                var p2 = p2Idx >= _polygonVertices.Count ? _polygonVertices[0] : _polygonVertices[p2Idx];
                var line = new LineSegment2D(p1, p2);

                var intersections = other.Intersections(line).ToArray();
                if (intersections.Any())
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainBounds(Bounds target)
        {
            return _bounds.Contains(target.min) && _bounds.Contains(target.max);
        }
    }
}
