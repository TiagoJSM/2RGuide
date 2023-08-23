﻿using Assets.Scripts;
using Clipper2Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Editor
{
    [CustomEditor(typeof(World))]
    public class WorldEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var world = (World)target;
            if (GUILayout.Button("Bake Pathfinding"))
            {
                CollectSegments(world);
            }
        }

        public static Paths64 Polytree_Union(Paths64 subjects, FillRule fillrule)
        {
            // of course this function is inefficient, 
            // but it's purpose is simply to test polytrees.
            PolyTree64 polytree = new PolyTree64();
            Clipper.BooleanOp(ClipType.Union, subjects, null, polytree, fillrule);
            return Clipper.PolyTreeToPaths64(polytree);
        }

        //public static void SquaresTest(bool test_polytree = false)
        //{
        //    FillRule fillrule = FillRule.NonZero;

        //    var shape1 = Clipper.MakePath(new double[] { 0,0, 10,0, 10,10, 0,10 });
        //    var shape2 = Clipper.MakePath(new double[] { 5,5, 15,5, 15,15, 5,15 });
        //    var subjects = new PathsD
        //    {
        //        shape1,
        //        shape2
        //    };

        //    var solution = Clipper.Union(subjects, fillrule);

        //    Handles.color = Color.blue;
        //    foreach (var path in solution)
        //    {
        //        var p1 = path[0];
        //        for (var idx = 1; idx < path.Count; idx ++)
        //        {
        //            var p2 = path[idx];
        //            Handles.DrawLine(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y), EditorGUIUtility.pixelsPerPoint * 3);
        //            p1 = p2;
        //        }
        //        var start = path[0];
        //        Handles.DrawLine(new Vector2(start.X, start.Y), new Vector2(p1.X, p1.Y), EditorGUIUtility.pixelsPerPoint * 3);
        //    }
        //}

        private PathsD UnionShapes(PathsD shapes)
        {
            FillRule fillrule = FillRule.NonZero;
            return Clipper.Union(shapes, fillrule);
        }

        private void CollectSegments(World world)
        {
            var paths = new PathsD();
            var children = world.transform.childCount;
            for (var idx = 0; idx < children; idx++)
            {
                var child = world.transform.GetChild(idx);
                if (child.gameObject.activeInHierarchy)
                {
                    var collider = child.GetComponent<Collider2D>();
                    CollectSegments(collider, paths);
                }
            }
            paths = UnionShapes(paths);
            var segments = ConvertToSegments(paths);
            AssignSegments(segments);
        }

        private List<Segment> ConvertToSegments(PathsD paths)
        {
            var segments = new List<Segment>();

            foreach (var path in paths)
            {
                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = path[idx];
                    segments.Add(new Segment() { p1 = new Vector2((float)p1.x, (float)p1.y), p2 = new Vector2((float)p2.x, (float)p2.y) });
                    p1 = p2;
                }
                var start = path[0];
                segments.Add(new Segment() { p1 = new Vector2((float)start.x, (float)start.y), p2 = new Vector2((float)p1.x, (float)p1.y) });
            }

            return segments;
        }

        private void CollectSegments(Collider2D collider, PathsD paths)
        {
            if(collider is BoxCollider2D box)
            {
                CollectSegments(box, paths);
            }
            //else if(collider is EdgeCollider2D edge)
            //{
            //    CollectSegments(edge, paths);
            //}
            else if (collider is PolygonCollider2D polygon)
            {
                CollectSegments(polygon, paths);
            }
            else if (collider is CompositeCollider2D composite)
            {
                CollectSegments(composite, paths);
            }
        }

        private void CollectSegments(BoxCollider2D collider, PathsD paths)
        {
            var bounds = collider.bounds;

            var shape = Clipper.MakePath(new double[] 
                { 
                    bounds.min.x, bounds.min.y, 
                    bounds.min.x, bounds.max.y,
                    bounds.max.x, bounds.max.y,
                    bounds.max.x, bounds.min.y,
                });

            paths.Add(shape);
        }

        //private void CollectSegments(EdgeCollider2D collider, PathsD paths)
        //{
        //    if(collider.pointCount < 1)
        //    {
        //        return;
        //    }

        //    var p1 = collider.transform.TransformPoint(collider.points[0]);
        //    for (var idx = 1; idx < collider.pointCount; idx++)
        //    {
        //        var p2 = collider.transform.TransformPoint(collider.points[idx]);
        //        buffer.Add(new Segment { p1 = p1, p2 = p2 });
        //        p1 = p2;
        //    }
        //}

        private void CollectSegments(PolygonCollider2D collider, PathsD paths)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = collider.GetPath(pathIdx);

                if (path.Length < 1)
                {
                    return;
                }

                var points = path.SelectMany(p => 
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                var shape = Clipper.MakePath(points.ToArray());

                paths.Add(shape);
            }
        }

        private void CollectSegments(CompositeCollider2D collider, PathsD paths)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = new List<Vector2>();
                var _ = collider.GetPath(pathIdx, path);

                if (path.Count < 1)
                {
                    return;
                }

                var points = path.SelectMany(p =>
                {
                    p = collider.transform.TransformPoint(p);
                    return new double[] { p.x, p.y };
                });

                var shape = Clipper.MakePath(points.ToArray());

                paths.Add(shape);
                //var path = new List<Vector2>();
                //var _ = collider.GetPath(pathIdx, path);

                //if (path.Count < 1)
                //{
                //    return;
                //}

                //var pathSegments = new List<Segment>();
                //var p1 = collider.transform.TransformPoint(path[0]);
                //for (var idx = 1; idx < path.Count; idx++)
                //{
                //    var p2 = collider.transform.TransformPoint(path[idx]);
                //    pathSegments.Add(new Segment { p1 = p1, p2 = p2 });
                //    p1 = p2;
                //}
                //pathSegments.Add(new Segment { p1 = pathSegments[pathSegments.Count - 1].p2, p2 = pathSegments[0].p1 });
                //buffer.AddRange(pathSegments);
            }
        }

        private void AssignSegments(List<Segment> segments)
        {
            var world = (World)target;
            var so = new SerializedObject(world);

            var prop = so.FindProperty(nameof(world.segments));

            prop.arraySize = segments.Count;
            for (var idx = 0; idx < segments.Count; idx++)
            {
                var serializedElement = prop.GetArrayElementAtIndex(idx);
                serializedElement.FindPropertyRelative(nameof(Segment.p1)).vector2Value = segments[idx].p1;
                serializedElement.FindPropertyRelative(nameof(Segment.p2)).vector2Value = segments[idx].p2;
            }
            so.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var world = (World)target;
            var segments = world.segments;

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                Handles.color = Color.blue;
                Handles.DrawLine(segment.p1, segment.p2, EditorGUIUtility.pixelsPerPoint * 3);
            }

            //SquaresTest(true);
        }
    }
}