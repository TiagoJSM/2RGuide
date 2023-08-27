using System;
using System.Collections;
using UnityEngine;

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
    }
}