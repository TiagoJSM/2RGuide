using Assets.Scripts._2RGuide.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace Assets.Scripts._2RGuide.Math
{
    [Serializable]
    public struct LineSegment2D
    {
        public Vector2 P1;
        public Vector2 P2;

        public float B
        {
            get
            {
                //y = m.x + b
                //b = y - m.x
                return P1.y - (Slope.Value * P1.x);
            }
        }

        public Vector2 NormalVector
        {
            get
            {
                return new Vector2(-(P2.y - P1.y), (P2.x - P1.x));
            }
        }

        public LineSegment2D(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public float? Slope
        {
            get
            {
                var xDif = P2.x - P1.x;
                if (xDif == 0)
                {
                    return null;
                }
                return (P2.y - P1.y) / (xDif);
            }
        }

        public float Lenght
        {
            get
            {
                return Mathf.Abs(Vector2.Distance(P1, P2));
            }
        }

        public float? XWhenYIs(float y)
        {
            if (!ContainsY(y))
            {
                return null;
            }
            //y = m.x + b
            //x = (y - b) / m
            return (y - B) / Slope.Value;
        }

        public float? YWhenXIs(float x)
        {
            if (!ContainsX(x))
            {
                return null;
            }
            if(!Slope.HasValue)
            {
                return null;
            }
            //y = m.x + b
            return Slope.Value * x + B;
        }

        public Vector2? PositionInX(float x)
        {
            var y = YWhenXIs(x);
            return y.HasValue ? new Vector2(x, y.Value) : default(Vector2?);
        }

        public Vector2? PositionInY(float y)
        {
            var x = XWhenYIs(y);
            return x.HasValue ? new Vector2(x.Value, y) : default(Vector2?);
        }

        public bool PositiveSlope
        {
            get
            {
                return P2.y > P1.y;
            }
        }

        public bool NegativeSlope
        {
            get
            {
                return P1.y > P2.y;
            }
        }

        public bool ContainsX(float xValue)
        {
            if (P1.x <= xValue && xValue <= P2.x)
            {
                return true;
            }
            if (P2.x <= xValue && xValue <= P1.x)
            {
                return true;
            }
            return false;
        }

        public bool ContainsY(float yValue)
        {
            if (P1.y <= yValue && yValue <= P2.y)
            {
                return true;
            }
            if (P2.y <= yValue && yValue <= P1.y)
            {
                return true;
            }
            return false;
        }

        public Vector2 HalfPoint
        {
            get
            {
                var halfLenght = (P2 - P1) / 2;
                return P1 + halfLenght;
            }
        }

        public float SlopeRadians => Slope == null ? 90.0f : Mathf.Atan(Slope.Value);

        public bool DoLinesIntersect(LineSegment2D other, bool validateLineEndingIntersection = true)
        {
            return DoIntersect(P1, P2, other.P1, other.P2, validateLineEndingIntersection);
        }

        public Vector2? GetIntersection(LineSegment2D other, bool validateLineEndingIntersection = true)
        {
            if (!DoLinesIntersect(other, validateLineEndingIntersection))
            {
                return null;
            }
            return LineIntersectionPoint(other);
        }

        public static LineSegment2D operator +(LineSegment2D segment, Vector2 offset)
        {
            return new LineSegment2D(segment.P1 + offset, segment.P2 + offset);
        }

        public override string ToString()
        {
            return "(" + P1.ToString() + ";" + P2.ToString() + ")";
        }

        public static float CalculateY(float x, float b, float m)
        {
            return x * m + b;
        }

        public static float CalculateB(float x, float y, float m)
        {
            return y - (x * m);
        }

        public static implicit operator bool(LineSegment2D segment)
        {
            return segment.P1 != Vector2.zero || segment.P2 != Vector2.zero;
        }

        private Vector2 LineIntersectionPoint(LineSegment2D other)
        {
            // Get A,B,C of first line
            float A1 = P2.y - P1.y;
            float B1 = P1.x - P2.x;
            float C1 = A1 * P1.x + B1 * P1.y;

            // Get A,B,C of second line
            float A2 = other.P2.y - other.P1.y;
            float B2 = other.P1.x - other.P2.x;
            float C2 = A2 * other.P1.x + B2 * other.P1.y;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
                return Vector2.zero;

            // now return the intersection point
            return new Vector2(
                (B2 * C1 - B1 * C2) / delta,
                (A1 * C2 - A2 * C1) / delta
            );
        }

        // Given three collinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are collinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        private static int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
            // for details of below formula.
            var val = (q.y - p.y) * (r.x - q.x) -
                    (q.x - p.x) * (r.y - q.y);

            if (Mathf.Approximately(val, 0)) return 0; // collinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The main function that returns true if line segment 'p1q1'
        // and 'p2q2' intersect.
        private static bool DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2, bool validateLineEndingIntersection = true)
        {
            if (!validateLineEndingIntersection)
            {
                var intersectsAtLineEnd = p1.Approximately(p2) || p1.Approximately(q2) || q1.Approximately(p2) || q1.Approximately(q2);

                if (intersectsAtLineEnd)
                {
                    return false;
                }
            }

            // Find the four orientations needed for general and
            // special cases
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases
            // p1, q1 and p2 are collinear and p2 lies on segment p1q1
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are collinear and q2 lies on segment p1q1
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are collinear and p1 lies on segment p2q2
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are collinear and q1 lies on segment p2q2
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases
        }
    }
}