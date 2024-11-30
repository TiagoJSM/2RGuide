using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public class NavTagBoxBounds
    {
        private PathD _clipperPath;
        private Polygon _polygon;
        public NavTag NavTag { get; }

        public NavTagBoxBounds(NavTagBounds navTagBounds)
        {
            NavTag = navTagBounds.NavTag;
            var bounds = new Box(navTagBounds.Collider);
            _polygon = new Polygon(
                new[] 
                { 
                    bounds.BottomLeft, 
                    bounds.TopLeft,
                    bounds.TopRight,
                    bounds.BottomRight
                });
        }

        public bool Contains(LineSegment2D lineSeg)
        {
            var p1Result = _polygon.IsPointInPolygon(lineSeg.P1);
            var p2Result = _polygon.IsPointInPolygon(lineSeg.P2);

            return p1Result && p2Result;
        }
    }
}