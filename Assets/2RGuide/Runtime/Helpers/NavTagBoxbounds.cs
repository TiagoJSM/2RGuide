using Assets._2RGuide.Runtime.Math;

namespace Assets._2RGuide.Runtime.Helpers
{
    public class NavTagBoxBounds
    {
        public NavTag NavTag { get; }
        public Polygon Polygon { get; }

        public NavTagBoxBounds(NavTag navTag, Polygon polygon)
        {
            NavTag = navTag;
            Polygon = polygon;
        }

        public NavTagBoxBounds(NavTagBounds navTagBounds)
            : this(navTagBounds.NavTag, new Polygon(navTagBounds.Collider))
        {
        }

        public bool Contains(LineSegment2D lineSeg)
        {
            var p1Result = Polygon.IsPointInPolygon(lineSeg.P1);
            var p2Result = Polygon.IsPointInPolygon(lineSeg.P2);

            return p1Result && p2Result;
        }
    }
}