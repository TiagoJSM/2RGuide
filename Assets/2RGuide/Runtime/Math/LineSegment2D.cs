using Assets._2RGuide.Runtime.Helpers;
using System;
using System.Drawing;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Math
{
    [Serializable]
    public struct LineSegment2D
    {
        public RGuideVector2 P1;
        public RGuideVector2 P2;

        public float B
        {
            get
            {
                //y = m.x + b
                //b = y - m.x
                return P1.y - (Slope.Value * P1.x);
            }
        }

        public RGuideVector2 NormalizedNormalVector
        {
            get
            {
                return (new RGuideVector2(-(P2.y - P1.y), P2.x - P1.x)).normalized;
            }
        }

        public LineSegment2D(RGuideVector2 p1, RGuideVector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public float? Slope
        {
            get
            {
                var xDif = P2.x - P1.x;
                if (xDif.Approximately(0.0f))
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
                return Mathf.Abs(RGuideVector2.Distance(P1, P2));
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
            if (!Slope.HasValue)
            {
                return null;
            }
            //y = m.x + b
            return Slope.Value * x + B;
        }

        public RGuideVector2? PositionAtX(float x)
        {
            var y = YWhenXIs(x);
            return y.HasValue ? new RGuideVector2(x, y.Value) : default(RGuideVector2?);
        }

        public RGuideVector2? PositionAtY(float y)
        {
            var x = XWhenYIs(y);
            return x.HasValue ? new RGuideVector2(x.Value, y) : default(RGuideVector2?);
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

        public RGuideVector2 HalfPoint
        {
            get
            {
                var halfLenght = (P2 - P1) / 2;
                return P1 + halfLenght;
            }
        }

        public float SlopeRadians => Slope == null ? (90.0f * NormalizedNormalVector.x) * Mathf.Deg2Rad : Mathf.Atan(Slope.Value);
        public float SlopeDegrees => SlopeRadians * Mathf.Rad2Deg;

        public RGuideVector2? GetIntersection(LineSegment2D other, bool validateLineEndingIntersection = true)
        {
            FindIntersection(
                other, 
                out bool linesIntersect, 
                out bool segmentsIntersect,
                out RGuideVector2 intersection,
                out RGuideVector2 closeP1, out RGuideVector2 closeP2);

            if(!segmentsIntersect)
            {
                return default;
            }

            if(!validateLineEndingIntersection)
            {
                if(P1.Approximately(intersection) || P2.Approximately(intersection) || other.P1.Approximately(intersection) || other.P2.Approximately(intersection))
                {
                    return default;
                }
            }

            return intersection;
        }

        // https://www.csharphelper.com/howtos/howto_segment_intersection.html#:~:text=If%20t1%20and%20t2%20are%20both%20between%200%20and%201,to%20find%20those%20closest%20points.
        private void FindIntersection(
            LineSegment2D other,
            out bool linesIntersect, out bool segmentsIntersect,
            out RGuideVector2 intersection,
            out RGuideVector2 closeP1, out RGuideVector2 closeP2)
        {
            var p1 = this.P1;
            var p2 = this.P2;
            var p3 = other.P1;
            var p4 = other.P2;

            // Get the segments' parameters.
            float dx12 = p2.x - p1.x;
            float dy12 = p2.y - p1.y;
            float dx34 = p4.x - p3.x;
            float dy34 = p4.y - p3.y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                linesIntersect = false;
                segmentsIntersect = false;
                intersection = RGuideVector2.NaN;
                closeP1 = RGuideVector2.NaN;
                closeP2 = RGuideVector2.NaN;
                return;
            }
            linesIntersect = true;

            float t2 =
                ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new RGuideVector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segmentsIntersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            closeP1 = new RGuideVector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
            closeP2 = new RGuideVector2(p3.x + dx34 * t2, p3.y + dy34 * t2);
        }

        // https://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm
        public bool IntersectsCircle(Circle circle)
        {
            //E is the starting point of the ray,
            //L is the end point of the ray,
            //C is the center of sphere you're testing against
            //r is the radius of that sphere
            //Compute:
            //d = L - E ( Direction vector of ray, from start to end )
            //f = E - C ( Vector from center sphere to ray start )

            var d = P2 - P1;
            var f = P1 - circle.center;
            var r = circle.radius;

            float a = RGuideVector2.Dot(d, d);
            float b = 2 * RGuideVector2.Dot(f, d);
            float c = RGuideVector2.Dot(f, f) - r * r;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                if (circle.IsInside(P1) || circle.IsInside(P2))
                {
                    return true;
                }
                // no intersection
                return false;
            }
            else
            {
                // ray didn't totally miss sphere,
                // so there is a solution to
                // the equation.

                discriminant = Mathf.Sqrt(discriminant);

                // either solution may be on or off the ray so need to test both
                // t1 is always the smaller value, because BOTH discriminant and
                // a are nonnegative.
                float t1 = (-b - discriminant) / (2 * a);
                float t2 = (-b + discriminant) / (2 * a);

                // 3x HIT cases:
                //          -o->             --|-->  |            |  --|->
                // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

                // 3x MISS cases:
                //       ->  o                     o ->              | -> |
                // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

                if (t1 >= 0 && t1 <= 1)
                {
                    // t1 is the intersection, and it's closer than t2
                    // (since t1 uses -b - discriminant)
                    // Impale, Poke
                    return true;
                }

                // here t1 didn't intersect so we are either started
                // inside the sphere or completely past it
                if (t2 >= 0 && t2 <= 1)
                {
                    // ExitWound
                    return true;
                }

                if (circle.IsInside(P1) || circle.IsInside(P2))
                {
                    return true;
                }

                // no intn: FallShort, Past, CompletelyInside
                return false;
            }
        }

        public RGuideVector2 ClosestPointOnLine(RGuideVector2 point)
        {
            RGuideVector2 AP = point - P1;       //Vector from A to P   
            RGuideVector2 AB = P2 - P1;       //Vector from A to B  

            float magnitudeAB = AB.sqrMagnitude;     //Magnitude of AB vector (it's length squared)     
            float ABAPproduct = RGuideVector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return P1;

            }
            else if (distance > 1)
            {
                return P2;
            }
            else
            {
                return P1 + AB * distance;
            }
        }

        //https://stackoverflow.com/questions/7050186/find-if-point-lies-on-line-segment#:~:text=If%20V1%3D%3D(%2DV2,else%20it%20is%20past%20B.
        public bool Contains(RGuideVector2 p)
        {
            // Thank you @Rob Agar           
            // (x - x1) / (x2 - x1) = (y - y1) / (y2 - y1)
            // x1 < x < x2, assuming x1 < x2
            // y1 < y < y2, assuming y1 < y2          

            var minX = Mathf.Min(P1.x, P2.x);
            var maxX = Mathf.Max(P1.x, P2.x);

            var minY = Mathf.Min(P1.y, P2.y);
            var maxY = Mathf.Max(P1.y, P2.y);

            if (!(minX <= p.x) || !(p.x <= maxX) || !(minY <= p.y) || !(p.y <= maxY))
            {
                return false;
            }

            if (Mathf.Abs(P1.x - P2.x) < Constants.RGuideDelta)
            {
                return Mathf.Abs(P1.x - p.x) < Constants.RGuideDelta || Mathf.Abs(P2.x - p.x) < Constants.RGuideDelta;
            }

            if (Mathf.Abs(P1.y - P2.y) < Constants.RGuideDelta)
            {
                return Mathf.Abs(P1.y - p.y) < Constants.RGuideDelta || Mathf.Abs(P2.y - p.y) < Constants.RGuideDelta;
            }

            if (Mathf.Abs((p.x - P1.x) / (P2.x - P1.x) - (p.y - P1.y) / (P2.y - P1.y)) < Constants.RGuideDelta)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsCoincident(LineSegment2D other)
        {
            return (P1.Approximately(other.P1) && P2.Approximately(other.P2)) || (P1.Approximately(other.P2) && P2.Approximately(other.P1));
        }

        public static LineSegment2D operator +(LineSegment2D segment, RGuideVector2 offset)
        {
            return new LineSegment2D(segment.P1 + offset, segment.P2 + offset);
        }

        public override string ToString()
        {
            return "(" + P1.ToString("F6") + ";" + P2.ToString("F6") + ")";
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
            return segment.P1 != RGuideVector2.zero || segment.P2 != RGuideVector2.zero;
        }

        public static bool operator ==(LineSegment2D line1, LineSegment2D line2)
        {
            return line1.Equals(line2);
        }

        public static bool operator !=(LineSegment2D line1, LineSegment2D line2)
        {
            return !(line1 == line2);
        }

        // Given three collinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        private static bool OnSegment(RGuideVector2 p, RGuideVector2 q, RGuideVector2 r)
        {
            if (q.x.LessThanOrEquals(Mathf.Max(p.x, r.x)) && q.x.GreaterThanOrEquals(Mathf.Min(p.x, r.x)) &&
                q.y.LessThanOrEquals(Mathf.Max(p.y, r.y)) && q.y.GreaterThanOrEquals(Mathf.Min(p.y, r.y)))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are collinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        private static int Orientation(RGuideVector2 p, RGuideVector2 q, RGuideVector2 r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
            // for details of below formula.
            var val = (q.y - p.y) * (r.x - q.x) -
                    (q.x - p.x) * (r.y - q.y);

            if (val.Approximately(0.0f)) return 0; // collinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The main function that returns true if line segment 'p1q1'
        // and 'p2q2' intersect.
        private static bool DoIntersect(RGuideVector2 p1, RGuideVector2 q1, RGuideVector2 p2, RGuideVector2 q2, bool validateLineEndingIntersection = true)
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

            var collinear = ValidateIfCollinear(p1, q1, p2, q2, o1, o2, o3, o4);

            if (collinear)
            {
                if (!validateLineEndingIntersection)
                {
                    return false;
                }
                return true;
            }

            // General case
            return o1 != o2 && o3 != o4;
        }

        private static bool ValidateIfCollinear(RGuideVector2 p1, RGuideVector2 q1, RGuideVector2 p2, RGuideVector2 q2, int o1, int o2, int o3, int o4)
        {
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

        public override bool Equals(object obj)
        {
            if(obj is LineSegment2D ls)
            {
                return ls.P1 == P1 && ls.P2 == P2;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}