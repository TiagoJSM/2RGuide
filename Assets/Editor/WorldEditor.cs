using Assets.Scripts;
using Clipper2Lib;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public static void SquaresTest(bool test_polytree = false)
        {
            FillRule fillrule = FillRule.NonZero;

            var shape1 = Clipper.MakePath(new double[] { 0,0, 10,0, 10,10, 0,10 });
            var shape2 = Clipper.MakePath(new double[] { 5,5, 15,5, 15,15, 5,15 });
            var subjects = new PathsD();
            subjects.Add(shape1);
            subjects.Add(shape2);

            var solution = Clipper.Union(subjects, fillrule);

            Handles.color = Color.blue;
            foreach (var path in solution)
            {
                var p1 = path[0];
                for (var idx = 1; idx < path.Count; idx ++)
                {
                    var p2 = path[idx];
                    Handles.DrawLine(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y), EditorGUIUtility.pixelsPerPoint * 3);
                    p1 = p2;
                }
                var start = path[0];
                Handles.DrawLine(new Vector2(start.X, start.Y), new Vector2(p1.X, p1.Y), EditorGUIUtility.pixelsPerPoint * 3);
            }
        }

        private void CollectSegments(World world)
        {
            var segments = new List<Segment>();
            var children = world.transform.childCount;
            for (var idx = 0; idx < children; idx++)
            {
                var child = world.transform.GetChild(idx);
                var collider = child.GetComponent<Collider2D>();
                CollectSegments(collider, segments);
            }
            AssignSegments(segments);
        }

        private void CollectSegments(Collider2D collider, List<Segment> buffer)
        {
            if(collider is BoxCollider2D box)
            {
                CollectSegments(box, buffer);
            }
            else if(collider is EdgeCollider2D edge)
            {
                CollectSegments(edge, buffer);
            }
            else if (collider is PolygonCollider2D polygon)
            {
                CollectSegments(polygon, buffer);
            }
            else if (collider is CompositeCollider2D composite)
            {
                CollectSegments(composite, buffer);
            }
        }

        private void CollectSegments(BoxCollider2D collider, List<Segment> buffer)
        {
            var bounds = collider.bounds;

            var s1 = new Segment { p1 = bounds.min, p2 = new Vector2(bounds.min.x, bounds.max.y) };
            var s2 = new Segment { p1 = s1.p2, p2 = bounds.max };
            var s3 = new Segment { p1 = s2.p2, p2 = new Vector2(bounds.max.x, bounds.min.y) };
            var s4 = new Segment { p1 = s3.p2, p2 = bounds.min };

            buffer.Add(s1);
            buffer.Add(s2);
            buffer.Add(s3);
            buffer.Add(s4); 
        }

        private void CollectSegments(EdgeCollider2D collider, List<Segment> buffer)
        {
            if(collider.pointCount < 1)
            {
                return;
            }

            var p1 = collider.transform.TransformPoint(collider.points[0]);
            for (var idx = 1; idx < collider.pointCount; idx++)
            {
                var p2 = collider.transform.TransformPoint(collider.points[idx]);
                buffer.Add(new Segment { p1 = p1, p2 = p2 });
                p1 = p2;
            }
        }

        private void CollectSegments(PolygonCollider2D collider, List<Segment> buffer)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = collider.GetPath(pathIdx);

                if (path.Length < 1)
                {
                    return;
                }

                var pathSegments = new List<Segment>();
                var p1 = collider.transform.TransformPoint(path[0]);
                for (var idx = 1; idx < path.Length; idx++)
                {
                    var p2 = collider.transform.TransformPoint(path[idx]);
                    pathSegments.Add(new Segment { p1 = p1, p2 = p2 });
                    p1 = p2;
                }
                pathSegments.Add(new Segment { p1 = pathSegments[pathSegments.Count - 1].p2, p2 = pathSegments[0].p1 });
                buffer.AddRange(pathSegments);
            }
        }

        private void CollectSegments(CompositeCollider2D collider, List<Segment> buffer)
        {
            for (var pathIdx = 0; pathIdx < collider.pathCount; pathIdx++)
            {
                var path = new List<Vector2>();
                var _ = collider.GetPath(pathIdx, path);

                if (path.Count < 1)
                {
                    return;
                }

                var pathSegments = new List<Segment>();
                var p1 = collider.transform.TransformPoint(path[0]);
                for (var idx = 1; idx < path.Count; idx++)
                {
                    var p2 = collider.transform.TransformPoint(path[idx]);
                    pathSegments.Add(new Segment { p1 = p1, p2 = p2 });
                    p1 = p2;
                }
                pathSegments.Add(new Segment { p1 = pathSegments[pathSegments.Count - 1].p2, p2 = pathSegments[0].p1 });
                buffer.AddRange(pathSegments);
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

            //for (int i = 0; i < segments.Length; i++)
            //{
            //    var segment = segments[i];
            //    Handles.color = Color.blue;
            //    Handles.DrawLine(segment.p1, segment.p2, EditorGUIUtility.pixelsPerPoint * 3);
            //}

            SquaresTest(true);
        }
    }
}