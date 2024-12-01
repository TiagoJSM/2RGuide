using Assets._2RGuide.Runtime.Math;
using Clipper2Lib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._2RGuide.Runtime.Helpers
{
    public static class ClipperUtils
    {
        public static ClipperD ConfiguredClipperD()
        {
            return new ClipperD(Constants.RoundingDecimalPrecision);
        }

        public static PathD MakePath(BoxCollider2D collider)
        {
            var bounds = new Box(collider);

            var vertices = new[]
            {
                bounds.BottomLeft,
                bounds.TopLeft,
                bounds.TopRight,
                bounds.BottomRight
            }
            .SelectMany(v => new double[] { v.x, v.y })
            .ToArray();

            return Clipper.MakePath(vertices);
        }

        public static (PathsD, PathsD) SplitPath(LineSegment2D openPathSegment, PathsD closedPaths)
        {
            var openPath = Clipper.MakePath(new double[]
            {
                openPathSegment.P1.x, openPathSegment.P1.y,
                openPathSegment.P2.x, openPathSegment.P2.y,
            });

            var clipper = ConfiguredClipperD();
            clipper.AddPath(openPath, PathType.Subject, true); // a line is open
            clipper.AddPaths(closedPaths, PathType.Clip, false); // a polygon is closed
            var resultClosedPath = new PathsD();
            var resultOutsidePath = new PathsD();
            var resultInsidePath = new PathsD();
            var diffRes = clipper.Execute(ClipType.Difference, FillRule.NonZero, resultClosedPath, resultOutsidePath);
            var diffIntersection = clipper.Execute(ClipType.Intersection, FillRule.NonZero, resultClosedPath, resultInsidePath);

            return diffRes ? (resultOutsidePath, resultInsidePath) : (new PathsD(), new PathsD());
        }
    }
}