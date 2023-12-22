using _2RGuide.Math;
using Clipper2Lib;
using System.Collections;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class ClipperUtils
    {
        public static ClipperD ConfiguredClipperD()
        {
            return new ClipperD(4);
        }

        public static PathsD GetSubtractedPathFromClosedPaths(LineSegment2D openPathSegment, PathsD closedPaths)
        {
            var openPath = Clipper.MakePath(new double[]
            {
                openPathSegment.P1.x, openPathSegment.P1.y,
                openPathSegment.P2.x, openPathSegment.P2.y,
            });

            var clipper = ConfiguredClipperD();
            clipper.AddPath(openPath, PathType.Subject, true); // a line is open
            clipper.AddPaths(closedPaths, PathType.Clip, false); // a polygon is closed
            var resultOpenPath = new PathsD();
            var resultClosedPath = new PathsD();
            var res = clipper.Execute(ClipType.Difference, FillRule.NonZero, resultOpenPath, resultClosedPath);

            return res ? resultClosedPath : new PathsD();
        }
    }
}