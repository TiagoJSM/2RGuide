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
            return new ClipperD(FloatHelper.RoundingDecimalPrecision);
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

        public static PathsD MakePaths(PolygonCollider2D collider)
        {
            var paths = new PathsD();
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = collider.GetPath(pathIdx);

                if (path.Length < 1)
                {
                    continue;
                }

                var points = path.SelectMany(p =>
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                paths.Add(Clipper.MakePath(points.ToArray()));
            }

            return paths;
        }

        public static PathsD MakePaths(CompositeCollider2D collider)
        {
            var paths = new PathsD();
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = new List<Vector2>();
                var _ = collider.GetPath(pathIdx, path);

                if (path.Count < 1)
                {
                    continue;
                }

                var points = path.SelectMany(p =>
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                var shape = Clipper.MakePath(points.ToArray());
                paths.Add(shape);
            }

            return paths;
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
            var res = clipper.Execute(ClipType.Difference, FillRule.NonZero, resultClosedPath, resultOpenPath);

            return res ? resultOpenPath : new PathsD();
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