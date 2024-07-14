using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public class NavTagBoxBounds
    {
        private PathD _clipperPath;
        public NavTag NavTag { get; }

        public NavTagBoxBounds(PathD clipperPath, NavTag navTag)
        {
            NavTag = navTag;
            _clipperPath = clipperPath;
        }

        public bool Contains(LineSegment2D lineSeg)
        {
            var p1Result = Clipper.PointInPolygon(new PointD(lineSeg.P1.x, lineSeg.P1.y), _clipperPath);
            var p2Result = Clipper.PointInPolygon(new PointD(lineSeg.P2.x, lineSeg.P2.y), _clipperPath);

            return (p1Result == PointInPolygonResult.IsInside || p1Result == PointInPolygonResult.IsOn) && 
                (p2Result == PointInPolygonResult.IsInside || p2Result == PointInPolygonResult.IsOn);
        }
    }
}